using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class TimesTableGridPopulator : MonoBehaviour
{[Header("Stars")]
[SerializeField] private StarMeter starMeter;

    public static TimesTableGridPopulator Instance;
    public AudioSource clickSound;

    public enum InnerCellsMode { Blank, Products }
    public enum Operation { Addition, Multiplication }

    [Header("Grid Setup")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private int maxFactor = 9;
    [SerializeField] private InnerCellsMode innerMode = InnerCellsMode.Blank;
    [SerializeField] private Operation operation = Operation.Addition;   // <<< set Addition for Beginner

    [Header("Colors")]
    [SerializeField] private Color headerColor   = new Color(0.90f, 0.35f, 0.25f);
    [SerializeField] private Color bodyColor     = new Color(0.22f, 0.25f, 0.85f);
    [SerializeField] private Color targetColor   = new Color(0.12f, 0.12f, 0.45f);
    [SerializeField] private Color hoverColor    = new Color(0.05f, 0.55f, 0.15f);

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
    [SerializeField] private int   revealPopVibrato    = 8;
    [SerializeField] private ScreenShakerUI shaker;

    [Header("Question UI")]
    [SerializeField] private TMP_Text firstNumberText;
    [SerializeField] private TMP_Text opText;
    [SerializeField] private TMP_Text secondNumberText;
    [SerializeField] private TMP_Text equalsText;
    [SerializeField] private TMP_Text finalNumberText;   // shows "?" until success

    [Header("Answer Rules")]
    [Tooltip("Accept (row,col) and (col,row) as correct (useful for Addition).")]
    [SerializeField] private bool acceptSymmetricPair = true;

    [Tooltip("If true, accept ANY cell whose value equals targetValue (many matches for Addition; off by default).")]
    [SerializeField] private bool acceptAnyValueMatch = false;

    // cache
    private Graphic[]      cellGraphics;
    private TMP_Text[]     cellTexts;
    private RectTransform[] cellRects;

    // bookkeeping
    private readonly List<CellRef> hiddenCells = new();
    private Coroutine jiggleCo;

    // question state
    private int targetRow, targetCol, targetValue;

    // hover state
    private int hoverRow = 0, hoverCol = 0;

    private int Rows => maxFactor + 1;
    private int Cols => maxFactor + 1;

    private struct CellRef
    {
        public RectTransform rt;
        public TMP_Text txt;
        public int r, c;
        public int Value(Operation op) => op == Operation.Addition ? r + c : r * c;
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
                if (g) g.color = isHeader ? headerColor : bodyColor;

                if (txt != null)
                {
                    if (r == 0 && c == 0)               txt.text = Symbol();
                    else if (r == 0)                    txt.text = c.ToString();
                    else if (c == 0)                    txt.text = r.ToString();
                    else if (innerMode == InnerCellsMode.Products) txt.text = CellValue(r, c).ToString();
                    else                                  txt.text = ""; // blank
                }

                var clickable = rt ? rt.GetComponent<TimesTableClickableCell>() : null;
                if (rt && clickable == null) clickable = rt.gameObject.AddComponent<TimesTableClickableCell>();

                bool disabled = isHeader || innerMode == InnerCellsMode.Products;
                clickable?.Init(this, r, c, disabled);

                if (!isHeader && innerMode == InnerCellsMode.Blank && txt && string.IsNullOrEmpty(txt.text))
                    hiddenCells.Add(new CellRef { rt = rt, txt = txt, r = r, c = c });
            }
        }

        SetOperatorSymbol();
        AskNextQuestion();
    }

    private int CellValue(int r, int c) => (operation == Operation.Addition) ? (r + c) : (r * c);
    private string Symbol() => (operation == Operation.Addition) ? "+" : "×";

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
            return; // Level complete hook
        }

        var cell = hiddenCells[Random.Range(0, hiddenCells.Count)];
        targetRow = cell.r;
        targetCol = cell.c;
        targetValue = cell.Value(operation);

        if (firstNumberText)  firstNumberText.text  = targetRow.ToString();
        if (secondNumberText) secondNumberText.text = targetCol.ToString();
        if (finalNumberText)  finalNumberText.text  = "?";  // show ? until success

        hoverRow = hoverCol = 0;
        RefreshColors();
    }

    // ============ Clicks ============
    public bool OnCellClicked(int r, int c, RectTransform rt)
    {
        if (clickSound) clickSound.Play();

        int idx = Index(r, c);
        var txt = cellTexts[idx];

        bool correct = IsCorrectPick(r, c);

        if (correct)
        {
            // reveal answer in the ? bubble
            if (finalNumberText) finalNumberText.text = targetValue.ToString();

            RevealCellPermanently(r, c, txt, rt);

            // remove from hidden
            hiddenCells.RemoveAll(cell => cell.r == r && cell.c == c);

            // award star progress if you’re using it
            starMeter?.AddCorrect();  // <- uncomment if you wired a StarMeter here

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
        if (acceptAnyValueMatch) return CellValue(r, c) == targetValue;

        if (acceptSymmetricPair)
        {
            // accept (row,col) or (col,row)
            return (r == targetRow && c == targetCol) || (r == targetCol && c == targetRow);
        }

        // exact target cell only
        return (r == targetRow && c == targetCol);
    }

    private IEnumerator NextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AskNextQuestion();
    }

    private void RevealCellPermanently(int r, int c, TMP_Text txt, RectTransform rt)
    {
        if (!txt || !rt) return;

        txt.text = CellValue(r, c).ToString();
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

        txt.text = CellValue(r, c).ToString();
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

    // ============ Hover cross ============
    public void OnHoverEnter(int r, int c) { hoverRow = r; hoverCol = c; RefreshColors(); }
    public void OnHoverExit (int r, int c) { if (hoverRow == r && hoverCol == c) { hoverRow = hoverCol = 0; RefreshColors(); } }

    private void RefreshColors()
    {
        // base
        for (int rr = 0; rr < Rows; rr++)
            for (int cc = 0; cc < Cols; cc++)
            {
                int idx = Index(rr, cc);
                bool isHeader = (rr == 0) || (cc == 0);
                if (cellGraphics[idx]) cellGraphics[idx].color = isHeader ? headerColor : bodyColor;
            }

        // target cross
        if (targetRow > 0 && targetCol > 0)
        {
            PaintCross(targetRow, targetCol, targetColor);
            if (acceptSymmetricPair && targetRow != targetCol)
                PaintCross(targetCol, targetRow, targetColor); // also show symmetric cross (optional, looks nice)
        }

        // hover cross on top
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
