using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;
using NUnit.Framework.Internal;

public class BalloonBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public RectTransform answerZone;        // target area
    public CanvasGroup answerHighlight;     // attach to answerZone or a child highlight; start alpha = 0
    public RectTransform shakeRoot;         // e.g., root UI panel or Canvas RectTransform

    [SerializeField] Transform balloon_Parent;


    [Header("Config")]
    public bool isCorrectAnswer = false;
    public float fadeMin = 0.25f;
    public float fadeMax = 1f;
    public float fadeSpeed = 0.6f;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 20f;
    public int   shakeVibrato = 20;

    // internals
    RectTransform rect;
    Canvas canvas;
    RectTransform canvasRect;
    Vector2 startLocalPos;
    Transform startParent;
    Vector2 pointerOffset;
    Tween fadeTween;



    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.transform as RectTransform;
        

        startLocalPos = rect.localPosition;
        startParent   = rect.parent;

        if (answerHighlight)
        {
            answerHighlight.alpha = 0f;
            answerHighlight.gameObject.SetActive(true);
        }
    }
    void OnEnable()
    {
        //  ResetToStart()
        ResetBehavior();
        enabled = true;
    }
    public void ResetToStart(bool reenableAnimator = true)
    {
        // put it back in the original container in the original spot
        rect.SetParent(startParent, worldPositionStays: false);
        rect.localPosition = startLocalPos;

        // clear state
        isCorrectAnswer = false;
        enabled = true;

        // optional: re-enable any idle animation
        var anim = GetComponent<Animator>();
        if (anim && reenableAnimator) anim.enabled = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rect.SetAsLastSibling();

        // capture grab offset
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out var lp))
        {
            pointerOffset = (Vector2)rect.localPosition - lp;
        }

        // start pulsing the answer zone
        if (answerHighlight)
        {
            fadeTween?.Kill();
            answerHighlight.alpha = fadeMin;
            fadeTween = answerHighlight
                .DOFade(fadeMax, fadeSpeed)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out var lp))
        {
            rect.localPosition = lp + pointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // stop pulsing
        if (answerHighlight)
        {
            fadeTween?.Kill();
            answerHighlight.alpha = 0f;
        }

        bool overlaps = IsOverlapping(rect, answerZone);

        if (overlaps && isCorrectAnswer)
        {
            rect.SetParent(answerZone, false);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = new Vector3(.7f, .7f, .7f);
            var anim = GetComponent<Animator>(); if (anim) anim.enabled = false;
            enabled = false; // lock if you want
            ResetAll();
          
        }
        else
        {
            // wrong → shake the screen/root, then return
            if (shakeRoot)
            {
                shakeRoot.DOComplete(); // avoid stacking
                shakeRoot.DOShakeAnchorPos(shakeDuration, shakeStrength, shakeVibrato);
            }

            rect.SetParent(startParent, false);
            rect.localPosition = startLocalPos;
        }
    }

    public void ResetAll()
    {
        if (LevelDesigner.Instance.test == true)
        {
            return;
        }
        else
        {
            isCorrectAnswer = false;
            LevelDesigner.Instance.LevelSwitcher();
            if (LevelDesigner.Instance.test == false)
            {
                Invoke(nameof(ResetBehavior), 2f);

            }

        }
    }

    private void ResetBehavior()
    {
        enabled = true;

        var anim = GetComponent<Animator>(); if (anim) anim.enabled = true;

        rect.transform.SetParent(balloon_Parent.transform);
        rect.transform.localPosition = Vector3.zero;
        rect.transform.localScale = Vector3.one;

    }

    bool IsOverlapping(RectTransform a, RectTransform b)
    {
        var ra = GetWorldRect(a);
        var rb = GetWorldRect(b);
        return ra.Overlaps(rb, true);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] c = new Vector3[4];
        rt.GetWorldCorners(c);
        var size = new Vector2(
            Vector3.Distance(c[0], c[3]),
            Vector3.Distance(c[0], c[1]));
        return new Rect(c[0], size);
    }
}
