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

    public static TimesTableGridPopulator Instance;
    public AudioSource clickSound;

    public enum InnerCellsMode { Blank, Products }
    public enum Operation { Addition, Subtraction, Multiplication, Division }

    [Header("Grid Setup")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private int maxFactor = 9;
    [SerializeField] private InnerCellsMode innerMode = InnerCellsMode.Blank;

    [Tooltip("Pick the operation for this level.")]
    [SerializeField] private Operation operation = Operation.Addition;

    [Header("Colors")]
    [SerializeField] private Color headerColor = new Color(0.90f, 0.35f, 0.25f);
    [SerializeField] private Color bodyColor   = new Color(0.22f, 0.25f, 0.85f);
    [SerializeField] private Color targetColor = new Color(0.12f, 0.12f, 0.45f);
    [SerializeField] private Color hoverColor  = new Color(0.05f, 0.55f, 0.15f);

    [Header("Division visual help")]
    [SerializeField] private bool dimNonTargetInDivision = true;
    [Range(0f,1f)] [SerializeField] private float nonTargetDim = 0.35f;

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
    [SerializeField] private TMP_Text finalNumberText; // "?" usually; in Division we show quotient

    [Header("Answer Rules")]
    [Tooltip("For + and ×, accept (row,col) and (col,row). For − and ÷ this is ignored.")]
    [SerializeField] private bool acceptSymmetricPair = true;

    [Tooltip("Accept ANY cell whose value equals the target value (ignored for Division).")]
    [SerializeField] private bool acceptAnyValueMatch = false;

    // cache
    private Graphic[]      cellGraphics;
    private TMP_Text[]     cellTexts;
    private RectTransform[] cellRects;

    private readonly List<CellRef> hiddenCells = new();
    private Coroutine jiggleCo;

    // current question (always refers to the *product cell* we picked)
    private int targetRow, targetCol, targetValue;

    // hover state
    private int hoverRow = 0, hoverCol = 0;

    // Division: which factor is hidden this question
    private enum MissingSide { First, Second } // First => row is "?", Second => col is "?"
    private MissingSide missingSide;

    private int Rows => maxFactor + 1; // includes header row
    private int Cols => maxFactor + 1; // includes header col

    private struct CellRef
    {
        public RectTransform rt;
        public TMP_Text txt;
        public int r, c;
        public int Value(Operation op)
        {
            return op switch
            {
                Operation.Addition       => r + c,
                Operation.Subtraction    => r - c,
                Operation.Multiplication => r * c,
                Operation.Division       => (c != 0) ? r / c : 0,
                _ => 0
            };
        }
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
    }

    void OnEnable()
    {
        Populate();
        if (jiggleEnabled && jiggleCo == null) jiggleCo = StartCoroutine(JiggleLoop());
    }

    void OnDisable()
    {
        if (jiggleCo != null) { StopCoroutine(jiggleCo); jiggleCo = null; }
        KillAllTweensAndResetScale();
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
                var rt  = cellRects[idx];

                if (rt) { rt.DOKill(true); rt.localScale = Vector3.one; }

                bool isHeader = (r == 0) || (c == 0);
                bool playable = IsCellPlayable(r, c);

                if (g) g.color = isHeader ? headerColor : bodyColor;

                if (txt != null)
                {
                    if (r == 0 && c == 0)               txt.text = Symbol();
                    else if (r == 0)                    txt.text = c.ToString();
                    else if (c == 0)                    txt.text = r.ToString();
                    else if (innerMode == InnerCellsMode.Products)
                    {
                        txt.text = playable ? DisplayCellValue(r, c).ToString() : "";
                    }
                    else
                    {
                        txt.text = ""; // training mode: hide values
                    }
                }

                var clickable = rt ? rt.GetComponent<TimesTableClickableCell>() : null;
                if (rt && clickable == null) clickable = rt.gameObject.AddComponent<TimesTableClickableCell>();

                bool disabled = isHeader || (innerMode == InnerCellsMode.Products) || !playable;
                clickable?.Init(this, r, c, disabled);

                if (!isHeader && innerMode == InnerCellsMode.Blank && playable && txt && string.IsNullOrEmpty(txt.text))
                    hiddenCells.Add(new CellRef { rt = rt, txt = txt, r = r, c = c });
            }
        }

        SetOperatorSymbol();
        AskNextQuestion();
    }

    private bool IsCellPlayable(int r, int c)
    {
        if (r == 0 || c == 0) return false;

        return operation switch
        {
            Operation.Subtraction    => r >= c, // keep non-negative
            Operation.Division       => true,   // any inner cell (we filter at question time)
            _                        => true,
        };
    }

    private int CellValue(int r, int c)
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

    // Value to DISPLAY inside a cell when peek/reveal.
    private int DisplayCellValue(int r, int c)
    {
        // Division shows multiplication table numbers on the grid
        return (operation == Operation.Division) ? (r * c) : CellValue(r, c);
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

        // default pick
        var cell = hiddenCells[Random.Range(0, hiddenCells.Count)];

        if (operation == Operation.Division)
        {
            // Pick a divisible cell so quotient is an integer
            var candidates = hiddenCells.FindAll(h => h.r % h.c == 0);
            if (candidates.Count > 0)
                cell = candidates[Random.Range(0, candidates.Count)];

            targetRow = cell.r; // dividend
            targetCol = cell.c; // divisor

            int quotient = targetRow / targetCol;     // ✅ correct
            int product  = targetRow * targetCol;     // shown on the grid product cell

            // Which side is the "?" in the bottom equation?
            missingSide = (Random.value < 0.5f) ? MissingSide.First : MissingSide.Second;

            if (opText)       opText.text = "÷";
            if (equalsText)   equalsText.text = "=";
            if (finalNumberText) finalNumberText.text = quotient.ToString();

            if (missingSide == MissingSide.First)
            {
                // ? ÷ divisor = quotient
                if (firstNumberText)  firstNumberText.text  = "?";
                if (secondNumberText) secondNumberText.text = targetCol.ToString();
            }
            else
            {
                // dividend ÷ ? = quotient
                if (firstNumberText)  firstNumberText.text  = targetRow.ToString();
                if (secondNumberText) secondNumberText.text = "?";
            }

            // store for consistency; correctness check uses (row,col) equality
            targetValue = product;

            hoverRow = hoverCol = 0;
            RefreshColors_DivisionFocus(); // cross highlight
        }
        else
        {
            targetRow = cell.r;
            targetCol = cell.c;
            targetValue = CellValue(targetRow, targetCol);

            if (firstNumberText)  firstNumberText.text  = targetRow.ToString();
            if (secondNumberText) secondNumberText.text = targetCol.ToString();
            if (finalNumberText)  finalNumberText.text  = "?";

            hoverRow = hoverCol = 0;
            RefreshColors();
        }
    }

    // ============ Clicks ============
    public bool OnCellClicked(int r, int c, RectTransform rt)
    {
        if (clickSound) clickSound.Play();

        int idx = Index(r, c);
        var txt = cellTexts[idx];

        // Division: ONLY the (row,col) product cell is correct.
        if (operation == Operation.Division && !(r == targetRow && c == targetCol))
        {
#if UNITY_EDITOR
            Debug.Log($"[Division] Wrong cell clicked at ({r},{c}). Must click product at ({targetRow},{targetCol}).");
#endif
            StartCoroutine(PeekAndHide(r, c, txt, rt));
            return false;
        }

        bool correct = IsCorrectPick(r, c);

        if (correct)
        {
            if (operation != Operation.Division && finalNumberText)
                finalNumberText.text = targetValue.ToString();

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

    private bool IsCorrectPick(int r, int c)
    {
        if (operation != Operation.Division && acceptAnyValueMatch)
            return CellValue(r, c) == targetValue;

        switch (operation)
        {
            case Operation.Addition:
            case Operation.Multiplication:
                if (acceptSymmetricPair)
                    return (r == targetRow && c == targetCol) || (r == targetCol && c == targetRow);
                return (r == targetRow && c == targetCol);

            case Operation.Subtraction:
                return (r == targetRow && c == targetCol);

            case Operation.Division:
                // ONLY the product cell (row × col) is correct.
                return (r == targetRow && c == targetCol);

            default:
                return (r == targetRow && c == targetCol);
        }
    }

    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AskNextQuestion();
    }

    private void RevealCellPermanently(int r, int c, TMP_Text txt, RectTransform rt)
    {
        if (!txt || !rt) return;

        txt.text = DisplayCellValue(r, c).ToString();
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

        txt.text = DisplayCellValue(r, c).ToString();
        rt.DOKill(true);
        rt.localScale = Vector3.one;

        yield return rt.DOScaleY(0f, 0.08f).SetEase(Ease.InQuad).WaitForCompletion();
        yield return rt.DOScaleY(1f, 0.12f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return rt.DOPunchScale(Vector3.one * 0.08f, 0.15f, 8).SetEase(Ease.OutQuad).WaitForCompletion();

        // hide again
        txt.text = "";
        yield return rt.DOScaleY(0f, 0.08f).SetEase(Ease.InQuad).WaitForCompletion();
        yield return rt.DOScaleY(1f, 0.10f).SetEase(Ease.OutQuad).WaitForCompletion();
        rt.localScale = Vector3.one;
    }

    // ============ Hover ============
    public void OnHoverEnter(int r, int c)
    {
        hoverRow = r; hoverCol = c;
        if (operation == Operation.Division) RefreshColors_DivisionFocus(); else RefreshColors();
    }
    public void OnHoverExit(int r, int c)
    {
        if (hoverRow == r && hoverCol == c) { hoverRow = hoverCol = 0; }
        if (operation == Operation.Division) RefreshColors_DivisionFocus(); else RefreshColors();
    }

    private void RefreshColors()
    {
        // reset
        for (int rr = 0; rr < Rows; rr++)
            for (int cc = 0; cc < Cols; cc++)
            {
                int idx = Index(rr, cc);
                bool isHeader = (rr == 0) || (cc == 0);
                if (cellGraphics[idx]) cellGraphics[idx].color = isHeader ? headerColor : bodyColor;
            }

        // target cross (+/× optional symmetric)
        if (targetRow > 0 && targetCol > 0)
        {
            PaintCross(targetRow, targetCol, targetColor);

            if (acceptSymmetricPair &&
                (operation == Operation.Addition || operation == Operation.Multiplication) &&
                targetRow != targetCol)
            {
                PaintCross(targetCol, targetRow, targetColor);
            }
        }

        // hover cross
        if (hoverRow > 0 && hoverCol > 0)
            PaintCross(hoverRow, hoverCol, hoverColor);
    }

    // Division: emphasize BOTH the quotient row and divisor column (a cross) and dim everything else if desired.
    private void RefreshColors_DivisionFocus()
    {
        // reset
        for (int rr = 0; rr < Rows; rr++)
            for (int cc = 0; cc < Cols; cc++)
            {
                int idx = Index(rr, cc);
                bool isHeader = (rr == 0) || (cc == 0);
                if (cellGraphics[idx]) cellGraphics[idx].color = isHeader ? headerColor : bodyColor;
            }

        if (targetRow <= 0 || targetCol <= 0) return;

        // Cross highlight
        PaintCross(targetRow, targetCol, targetColor);

        // Optional: dim non-cross cells to strongly guide the player
        if (dimNonTargetInDivision)
        {
            Color dim = Color.Lerp(bodyColor, Color.black, 1f - Mathf.Clamp01(nonTargetDim));
            for (int rr = 1; rr < Rows; rr++)
            {
                for (int cc = 1; cc < Cols; cc++)
                {
                    if (rr == targetRow || cc == targetCol) continue; // keep cross as-is
                    var g = cellGraphics[Index(rr, cc)];
                    if (g) g.color = dim;
                }
            }
        }

        // optional hover overlay
        if (hoverRow > 0 && hoverCol > 0)
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

    // ----------------- Jiggle -----------------
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
