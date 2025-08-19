using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class TimesTableGridPopulator : MonoBehaviour
{
    public static TimesTableGridPopulator Instance;
    public AudioSource clickSound;
    public enum InnerCellsMode { Blank, Products }

    [Header("Grid Setup")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private int maxFactor = 9;
    [SerializeField] private InnerCellsMode innerMode = InnerCellsMode.Blank;

    [Header("Colors (optional)")]
    [SerializeField] private Color headerColor = new Color(0.90f, 0.35f, 0.25f);
    [SerializeField] private Color bodyColor   = new Color(0.22f, 0.25f, 0.85f);
    [SerializeField] private Color highlightColor = new Color(0.12f, 0.12f, 0.45f);

    [Header("Jiggle (DOTween)")]
    [SerializeField] private bool jiggleEnabled = true;
    [SerializeField] private Vector2 intervalRange = new Vector2(1.5f, 3.0f);
    [SerializeField] private Vector2Int burstCountRange = new Vector2Int(1, 2);
    [SerializeField] private float jiggleDuration = 0.28f;
    [SerializeField] private float jiggleStrength = 0.12f;
    [SerializeField] private int jiggleVibrato = 6;

    [Header("FX (Reveal + Shake)")]
    [SerializeField] private float revealPopExtraScale = 0.18f; // 0.18 -> target ≈ 1.18 scale peak
    [SerializeField] private float revealPopDuration = 0.18f;
    [SerializeField] private int revealPopVibrato = 8;
    [SerializeField] private ScreenShakerUI shaker; // drag your ScreenShakerUI here (or leave null to auto-find)

   // [SerializeField] float shakeIntensity;
    // cache
    private Graphic[] cellGraphics;
    private TMP_Text[] cellTexts;
    private RectTransform[] cellRects;

    private readonly List<RectTransform> blankCells = new List<RectTransform>();
    private Coroutine jiggleCo;

    private int Rows => maxFactor + 1; // header row included
    private int Cols => maxFactor + 1; // header col included

    void Reset() { gridParent = GetComponent<RectTransform>(); }

    void Awake()
    {
        Instance = this;
        if (gridParent == null) gridParent = GetComponent<RectTransform>();

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

        if (shaker == null) shaker = FindObjectOfType<ScreenShakerUI>();
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

    /// Fill headers and inner cells, wire click handlers.
    public void Populate()
    {
        blankCells.Clear();

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                int idx = Index(r, c);
                var g   = cellGraphics[idx];
                var txt = cellTexts[idx];
                var rt  = cellRects[idx];

                if (rt)
                {
                    rt.DOKill(true);
                    rt.localScale = Vector3.one;
                }

                bool isHeader = (r == 0) || (c == 0);
                if (g) g.color = isHeader ? headerColor : bodyColor;

                if (txt != null)
                {
                    if (r == 0 && c == 0)           txt.text = "×";
                    else if (r == 0)                txt.text = c.ToString();
                    else if (c == 0)                txt.text = r.ToString();
                    else if (innerMode == InnerCellsMode.Products) txt.text = (r * c).ToString();
                    else                              txt.text = ""; // blank
                }

                // Click handling
                var clickable = rt ? rt.GetComponent<TimesTableClickableCell>() : null;
                if (rt && clickable == null) clickable = rt.gameObject.AddComponent<TimesTableClickableCell>();

                bool disabled = isHeader || innerMode == InnerCellsMode.Products;
                clickable?.Init(this, r, c, disabled);

                if (!isHeader && innerMode == InnerCellsMode.Blank && string.IsNullOrEmpty(txt.text))
                    blankCells.Add(rt);
            }
        }
    }

    /// Show product, stop its animation, and play reveal pop + screen shake.
    public bool RevealCell(int r, int c)
    {
        if (r <= 0 || c <= 0) return false; // headers ignored
        int idx = Index(r, c);
        var txt = cellTexts[idx];
        var rt  = cellRects[idx];
        if (txt == null || rt == null) return false;
        if (!string.IsNullOrEmpty(txt.text)) return false; // already revealed

        // set number
        txt.text = (r * c).ToString();

        // stop anim for this cell
        rt.DOKill(true);
        rt.localScale = Vector3.one;
        blankCells.Remove(rt);

        // POP reveal (small punch scale)
        rt.DOPunchScale(Vector3.one * revealPopExtraScale, revealPopDuration, revealPopVibrato)
          .SetEase(Ease.OutQuad)
          .OnKill(() => { if (rt) rt.localScale = Vector3.one; })
          .OnComplete(() => { if (rt) rt.localScale = Vector3.one; });

        // Screen/UI shake (subtle)
        if (shaker) shaker.Shake();  // you can pass a multiplier like Shake(1.2f)

        return true;
    }

    public void Highlight(int r, int c)
    {
        // reset colors
        for (int rr = 0; rr < Rows; rr++)
            for (int cc = 0; cc < Cols; cc++)
            {
                int idx = Index(rr, cc);
                bool isHeader = (rr == 0) || (cc == 0);
                if (cellGraphics[idx])
                    cellGraphics[idx].color = isHeader ? headerColor : bodyColor;
            }

        if (r <= 0 || c <= 0) return;

        for (int cc = 0; cc < Cols; cc++)
            if (cellGraphics[Index(r, cc)]) cellGraphics[Index(r, cc)].color = highlightColor;

        for (int rr = 0; rr < Rows; rr++)
            if (cellGraphics[Index(rr, c)]) cellGraphics[Index(rr, c)].color = highlightColor;
    }

    // ----------------- Jiggle -----------------

    private IEnumerator JiggleLoop()
    {
        while (true)
        {
            if (blankCells.Count > 0)
            {
                int burst = Mathf.Clamp(Random.Range(burstCountRange.x, burstCountRange.y + 1), 1, blankCells.Count);
                for (int i = 0; i < burst; i++)
                {
                    var cell = blankCells[Random.Range(0, blankCells.Count)];
                    JiggleCell(cell);
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
