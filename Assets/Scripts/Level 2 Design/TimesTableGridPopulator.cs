using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class TimesTableGridPopulator : MonoBehaviour
{
    [Header("Stars")]
    [SerializeField] private StarMeter starMeter;
    [SerializeField] ParticleSystem cofetti_PS;
    public static TimesTableGridPopulator Instance;
    public AudioSource clickSound;

    public enum InnerCellsMode { Blank, Products }
    public enum Operation { Addition, Subtraction, Multiplication, Division }

    // Multiple selectable fields
    [System.Flags]
    public enum GuessField
    {
        None   = 0,
        First  = 1 << 0,
        Second = 1 << 1,
        Final  = 1 << 2
    }

    // NEW: how the answer is given
    public enum InputMode { ClickGrid, Typed }

    [Header("Grid Setup")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private int maxFactor = 9;
    [SerializeField] private InnerCellsMode innerMode = InnerCellsMode.Blank;

    [Tooltip("Pick the operation for this level.")]
    [SerializeField] private Operation operation = Operation.Multiplication;

    [Header("Equation Setup")]
    [Tooltip("Which number(s) should be hidden with '?' for the player to guess.")]
    [SerializeField] private GuessField guessField = GuessField.Second;

    [Tooltip("How the player answers.")]
    [SerializeField] private InputMode inputMode = InputMode.Typed;

    [Header("Typed-Answer UI (used when InputMode = Typed)")]
    [SerializeField] private TMP_Text typedGuessText;   // shows user's current input
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip wrongClip;

    [Header("Colors")]
    [SerializeField] private Color headerColor = new Color(0.90f, 0.35f, 0.25f);
    [SerializeField] private Color bodyColor   = new Color(0.22f, 0.25f, 0.85f);
    [SerializeField] private Color targetColor = new Color(0.12f, 0.12f, 0.45f);
    [SerializeField] private Color hoverColor  = new Color(0.05f, 0.55f, 0.15f);

    [Header("Jiggle (DOTween)")]
    [SerializeField] private bool jiggleEnabled = true;
    [SerializeField] private Vector2 intervalRange = new Vector2(1.5f, 3.0f);
    [SerializeField] private Vector2Int burstCountRange = new Vector2Int(1, 2);
    [SerializeField] private float jiggleDuration = 0.28f;
    [SerializeField] private float jiggleStrength = 0.12f;
    [SerializeField] private int jiggleVibrato = 6;

    [Header("FX (Reveal + Shake)")]
    [SerializeField] private float revealPopExtraScale = 0.18f;
    [SerializeField] private float revealPopDuration   = 0.18f;
    [SerializeField] private int revealPopVibrato      = 8;
    [SerializeField] private ScreenShakerUI shaker;

    [Header("Question UI")]
    [SerializeField] private TMP_Text firstNumberText;
    [SerializeField] private TMP_Text opText;
    [SerializeField] private TMP_Text secondNumberText;
    [SerializeField] private TMP_Text equalsText;
    [SerializeField] private TMP_Text finalNumberText;

    [Header("Answer Rules")]
    [SerializeField] private bool acceptSymmetricPair = true;
    [SerializeField] private bool acceptAnyValueMatch = false;

    // cache
    private Graphic[] cellGraphics;
    private TMP_Text[] cellTexts;
    private RectTransform[] cellRects;

    private readonly List<CellRef> hiddenCells = new();
    private Coroutine jiggleCo;

    // current question (the actual hidden cell that defines the equation)
    private int targetRow, targetCol, targetValue;  // targetValue = op(row,col) except Division where it's quotient
    private int targetQuotient;                     // for Division only (quotient)

    // hover state (only for ClickGrid mode)
    private int hoverRow = 0, hoverCol = 0;

    private int Rows => maxFactor + 1;
    private int Cols => maxFactor + 1;

    public int cellText_Size = 48;

    // typed input buffer
    private string typedBuffer = "";

    private struct CellRef
    {
        public RectTransform rt;
        public TMP_Text txt;
        public int r, c;
    }

    void Reset() { gridParent = GetComponent<RectTransform>(); }

    void Awake()
    {
        Instance = this;
        if (!gridParent) gridParent = GetComponent<RectTransform>();

        int expected = Rows * Cols;
        int childCount = gridParent.childCount;
        if (childCount != expected)
            Debug.LogError($"Grid has {childCount} children but needs {expected} for a {Rows}x{Cols} table (maxFactor={maxFactor}).");

        cellGraphics = new Graphic[childCount];
        cellTexts    = new TMP_Text[childCount];
        cellRects    = new RectTransform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            var t = gridParent.GetChild(i);
            cellGraphics[i] = t.GetComponent<Graphic>();
            cellTexts[i]    = t.GetComponentInChildren<TMP_Text>(true);
            cellRects[i]    = t as RectTransform;
        }

        if (!shaker) shaker = FindObjectOfType<ScreenShakerUI>();
        UpdateTypedText();
    }

    void OnEnable()
    {
        Populate();
        if (jiggleEnabled && inputMode == InputMode.ClickGrid && jiggleCo == null)
            jiggleCo = StartCoroutine(JiggleLoop());
    }

    void OnDisable()
    {
        if (jiggleCo != null) { StopCoroutine(jiggleCo); jiggleCo = null; }
        KillAllTweensAndResetScale();
    }

    // ---------- PUBLIC UI HOOKS (for keypad/buttons) ----------
    public void AppendDigit(int d)
    {
        if (inputMode != InputMode.Typed) return;
        d = Mathf.Clamp(d, 0, 9);
        if (typedBuffer.Length >= 3) return; // simple cap
        typedBuffer += d.ToString();
        UpdateTypedText();
    }

    public void Backspace()
    {
        if (inputMode != InputMode.Typed) return;
        if (typedBuffer.Length > 0)
            typedBuffer = typedBuffer.Substring(0, typedBuffer.Length - 1);
        UpdateTypedText();
    }

    public void ClearGuess()
    {
        if (inputMode != InputMode.Typed) return;
        typedBuffer = "";
        UpdateTypedText();
    }

    public void OnCheckPressed()
    {
        if (inputMode != InputMode.Typed) return;

        if (!int.TryParse(typedBuffer, out int guess))
        {
            PlayWrong();
            return;
        }

        bool correct = false;

        // In typed mode, the correct value is exactly the hidden slot:
        // - If guessing First: guess must equal targetRow
        // - If guessing Second: guess must equal targetCol
        // - If guessing Final: guess must equal op(row,col) or quotient for Division
        if (guessField.HasFlag(GuessField.First))
        {
            correct = (guess == targetRow);
        }
        else if (guessField.HasFlag(GuessField.Second))
        {
            correct = (guess == targetCol);
        }
        else if (guessField.HasFlag(GuessField.Final))
        {
            correct = (operation == Operation.Division) ? (guess == targetQuotient)
                                                        : (guess == targetValue);
        }

        if (correct)
        {
            // Fill texts
            if (firstNumberText && guessField.HasFlag(GuessField.First))  firstNumberText.text  = targetRow.ToString();
            if (secondNumberText && guessField.HasFlag(GuessField.Second)) secondNumberText.text = targetCol.ToString();
            if (finalNumberText && guessField.HasFlag(GuessField.Final))
                finalNumberText.text = (operation == Operation.Division) ? targetQuotient.ToString()
                                                                          : targetValue.ToString();

            // Reveal the corresponding grid cell (row, col)
            var idx = Index(targetRow, targetCol);
            RevealCellPermanently(targetRow, targetCol, cellTexts[idx], cellRects[idx]);

            hiddenCells.RemoveAll(cell => cell.r == targetRow && cell.c == targetCol);

            PlayCorrect();
            starMeter?.AddCorrect();

            // next
            typedBuffer = "";
            UpdateTypedText();
            StartCoroutine(NextQuestionAfterDelay(0.35f));
        }
        else
        {
            PlayWrong();
            // keep the same question; let the user try again
        }
    }
    // ----------------------------------------------------------

    private void PlayCorrect()
    {
        if (cofetti_PS != null) cofetti_PS.Play();
        if (sfxSource && correctClip ) sfxSource.PlayOneShot(correctClip);
    }
    private void PlayWrong()
    {
        if (sfxSource && wrongClip && !sfxSource.isPlaying) sfxSource.PlayOneShot(wrongClip);
    }
    private void UpdateTypedText()
    {
        if (!typedGuessText) return;
        typedGuessText.text = string.IsNullOrEmpty(typedBuffer) ? "?" : typedBuffer;
    }

    private bool IsCorrectPick(int r, int c)
    {
        // Used only for ClickGrid mode
        switch (operation)
        {
            case Operation.Addition:
                if (acceptSymmetricPair)
                    return (r == targetRow && c == targetCol) || (r == targetCol && c == targetRow);
                return (r == targetRow && c == targetCol);

            case Operation.Multiplication:
                if (acceptSymmetricPair)
                    return (r == targetRow && c == targetCol) || (r == targetCol && c == targetRow);
                return (r == targetRow && c == targetCol);

            case Operation.Subtraction:
                return (r == targetRow && c == targetCol);

            case Operation.Division:
                if (c == 0) return false;
                return (r / c == targetRow / targetCol && r % c == 0);

            default:
                return (r == targetRow && c == targetCol);
        }
    }

    private int GridDisplayValue(int r, int c)
    {
        switch (operation)
        {
            case Operation.Addition:        return r + c;   // sum on Addition
            case Operation.Subtraction:     return r - c;   // optional
            case Operation.Multiplication:  return r * c;
            case Operation.Division:        return r * c;   // product table for Division (teaches facts)
            default:                        return r * c;
        }
    }

    public void Populate()
    {
        hiddenCells.Clear();

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                int idx = Index(r, c);
                var g   = cellGraphics[idx];
                var txt = cellTexts[idx];
                if (txt) txt.fontSize = cellText_Size;
                var rt  = cellRects[idx];

                if (rt) { rt.DOKill(true); rt.localScale = Vector3.one; }

                bool isHeader = (r == 0) || (c == 0);
                bool playable = (r > 0 && c > 0);

                if (g) g.color = isHeader ? headerColor : bodyColor;

                if (txt != null)
                {
                    if (r == 0 && c == 0)               txt.text = Symbol();
                    else if (r == 0)                    txt.text = c.ToString();
                    else if (c == 0)                    txt.text = r.ToString();
                    else if (innerMode == InnerCellsMode.Products)
                        txt.text = GridDisplayValue(r, c).ToString();
                    else
                        txt.text = "";
                }

                var clickable = rt ? rt.GetComponent<TimesTableClickableCell>() : null;
                if (rt && clickable == null) clickable = rt.gameObject.AddComponent<TimesTableClickableCell>();

                bool disabled = isHeader || (innerMode == InnerCellsMode.Products) || !playable
                                || (inputMode == InputMode.Typed); // disable clicks in typed mode
                clickable?.Init(this, r, c, disabled);

                if (!isHeader && innerMode == InnerCellsMode.Blank && playable && txt && string.IsNullOrEmpty(txt.text))
                    hiddenCells.Add(new CellRef { rt = rt, txt = txt, r = r, c = c });
            }
        }

        SetOperatorSymbol();
        AskNextQuestion();
    }

    private string Symbol()
    {
        return operation switch
        {
            Operation.Addition       => "+",
            Operation.Subtraction    => "−",
            Operation.Multiplication => "×",
            Operation.Division       => "÷",
            _ => "?"
        };
    }

    private void SetOperatorSymbol()
    {
        if (opText)     opText.text = Symbol();
        if (equalsText) equalsText.text = "=";
    }

    private void AskNextQuestion()
    {
        if (hiddenCells.Count == 0)
        {
            if (finalNumberText) finalNumberText.text = "";
            return;
        }

        var cell = hiddenCells[Random.Range(0, hiddenCells.Count)];

        if (operation == Operation.Division)
        {
            var candidates = hiddenCells.FindAll(h => h.r % h.c == 0);
            if (candidates.Count > 0) cell = candidates[Random.Range(0, candidates.Count)];
        }

        targetRow = cell.r;
        targetCol = cell.c;

        if (operation == Operation.Division)
        {
            targetQuotient = targetRow / targetCol; // integer
            targetValue    = targetRow * targetCol; // for grid display (product table)

            SetEquationTextsWithQuestion(
                targetRow.ToString(),
                targetCol.ToString(),
                targetQuotient.ToString(),
                guessField
            );
        }
        else
        {
            targetValue = CalcValue(targetRow, targetCol);

            SetEquationTextsWithQuestion(
                targetRow.ToString(),
                targetCol.ToString(),
                targetValue.ToString(),
                guessField
            );
        }

        // reset typed buffer for new question
        if (inputMode == InputMode.Typed)
        {
            typedBuffer = "";
            UpdateTypedText();
        }

        hoverRow = hoverCol = 0;
        RefreshColors();
    }

    private int CalcValue(int r, int c)
    {
        return operation switch
        {
            Operation.Addition       => r + c,
            Operation.Subtraction    => r - c,
            Operation.Multiplication => r * c,
            Operation.Division       => (c != 0) ? r / c : 0,
            _ => 0
        };
    }

    private void SetEquationTextsWithQuestion(string first, string second, string final, GuessField whichToHide)
    {
        if (opText)     opText.text = Symbol();
        if (equalsText) equalsText.text = "=";

        if (firstNumberText)
            firstNumberText.text  = whichToHide.HasFlag(GuessField.First)  ? "?" : first;
        if (secondNumberText)
            secondNumberText.text = whichToHide.HasFlag(GuessField.Second) ? "?" : second;
        if (finalNumberText)
            finalNumberText.text  = whichToHide.HasFlag(GuessField.Final)  ? "?" : final;
    }

    // -------- Click mode only --------
    public bool OnCellClicked(int r, int c, RectTransform rt)
    {
        if (inputMode != InputMode.ClickGrid) return false;

        if (clickSound) clickSound.Play();

        int idx = Index(r, c);
        var txt = cellTexts[idx];

        bool correct = IsCorrectPick(r, c);

        if (correct)
        {
            if (operation == Operation.Division)
            {
                if (guessField.HasFlag(GuessField.First) && firstNumberText)
                    firstNumberText.text = r.ToString();
                if (guessField.HasFlag(GuessField.Second) && secondNumberText)
                    secondNumberText.text = c.ToString();
                if (guessField.HasFlag(GuessField.Final) && finalNumberText)
                    finalNumberText.text = (r / c).ToString();
            }
            else
            {
                if (guessField.HasFlag(GuessField.First) && firstNumberText)
                    firstNumberText.text = r.ToString();
                if (guessField.HasFlag(GuessField.Second) && secondNumberText)
                    secondNumberText.text = c.ToString();
                if (guessField.HasFlag(GuessField.Final) && finalNumberText)
                    finalNumberText.text = CalcValue(r, c).ToString();
            }

            RevealCellPermanently(r, c, txt, rt);

            hiddenCells.RemoveAll(cell => cell.r == r && cell.c == c);

            starMeter?.AddCorrect();

            StartCoroutine(NextQuestionAfterDelay(0.35f));
            return true;
        }
        else
        {
            StartCoroutine(PeekAndHide(r, c, txt, rt));
            return false;
        }
    }
    // ---------------------------------

    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AskNextQuestion();
    }

    private void RevealCellPermanently(int r, int c, TMP_Text txt, RectTransform rt)
    {
        if (!txt || !rt) return;

        txt.text = GridDisplayValue(r, c).ToString();

        rt.DOKill(true);
        rt.localScale = Vector3.one;

        rt.DOPunchScale(Vector3.one * revealPopExtraScale, revealPopDuration, revealPopVibrato)
          .SetEase(Ease.OutQuad)
          .OnKill(() => { if (rt) rt.localScale = Vector3.one; })
          .OnComplete(() => { if (rt) rt.localScale = Vector3.one; });

        if (shaker) shaker.Shake();
    }

    private IEnumerator PeekAndHide(int r, int c, TMP_Text txt, RectTransform rt)
    {
        if (!txt || !rt) yield break;

        txt.text = GridDisplayValue(r, c).ToString();
        rt.DOKill(true);
        rt.localScale = Vector3.one;

        yield return rt.DOScaleY(0f, 0.08f).SetEase(Ease.InQuad).WaitForCompletion();
        yield return rt.DOScaleY(1f, 0.12f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return rt.DOPunchScale(Vector3.one * 0.08f, 0.15f, 8).SetEase(Ease.OutQuad).WaitForCompletion();

        txt.text = "";
        yield return rt.DOScaleY(0f, 0.08f).SetEase(Ease.InQuad).WaitForCompletion();
        yield return rt.DOScaleY(1f, 0.10f).SetEase(Ease.OutQuad).WaitForCompletion();
        rt.localScale = Vector3.one;
    }

    public void OnHoverEnter(int r, int c)
    {
        if (inputMode != InputMode.ClickGrid) return;
        hoverRow = r; hoverCol = c;
        RefreshColors();
    }
    public void OnHoverExit(int r, int c)
    {
        if (inputMode != InputMode.ClickGrid) return;
        if (hoverRow == r && hoverCol == c) { hoverRow = hoverCol = 0; }
        RefreshColors();
    }

    private void RefreshColors()
    {
        for (int rr = 0; rr < Rows; rr++)
            for (int cc = 0; cc < Cols; cc++)
            {
                int idx = Index(rr, cc);
                bool isHeader = (rr == 0) || (cc == 0);
                if (cellGraphics[idx]) cellGraphics[idx].color = isHeader ? headerColor : bodyColor;
            }

        if (inputMode == InputMode.ClickGrid && targetRow > 0 && targetCol > 0)
        {
            PaintCross(targetRow, targetCol, targetColor);

            if (acceptSymmetricPair &&
                (operation == Operation.Addition || operation == Operation.Multiplication) &&
                targetRow != targetCol)
            {
                PaintCross(targetCol, targetRow, targetColor);
            }
        }

        if (inputMode == InputMode.ClickGrid && hoverRow > 0 && hoverCol > 0)
            PaintCross(hoverRow, hoverCol, hoverColor);
    }

    private void PaintCross(int r, int c, Color color)
    {
        for (int cc = 0; cc < Cols; cc++)
        {
            var g = cellGraphics[Index(r, cc)];
            if (g) g.color = color;
        }
        for (int rr = 0; rr < Rows; rr++)
        {
            var g = cellGraphics[Index(rr, c)];
            if (g) g.color = color;
        }
    }

    private IEnumerator JiggleLoop()
    {
        while (true)
        {
            if (hiddenCells.Count > 0)
            {
                int burst = Mathf.Clamp(Random.Range(burstCountRange.x, burstCountRange.y + 1), 1, hiddenCells.Count);
                for (int i = 0; i < burst; i++)
                {
                    var cell = hiddenCells[Random.Range(0, hiddenCells.Count)];
                    JiggleCell(cell.rt);
                }
            }
            yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));
        }
    }

    private void JiggleCell(RectTransform rt)
    {
        if (!rt) return;
        rt.DOKill(true);
        rt.localScale = Vector3.one;

        rt.DOPunchScale(Vector3.one * jiggleStrength, jiggleDuration, jiggleVibrato)
          .SetEase(Ease.OutQuad)
          .OnKill(() => { if (rt) rt.localScale = Vector3.one; })
          .OnComplete(() => { if (rt) rt.localScale = Vector3.one; });
    }

    private void KillAllTweensAndResetScale()
    {
        if (cellRects == null) return;
        for (int i = 0; i < cellRects.Length; i++)
        {
            var rt = cellRects[i];
            if (!rt) continue;
            rt.DOKill(true);
            rt.localScale = Vector3.one;
        }
    }

    private int Index(int row, int col) => row * Cols + col;
}
