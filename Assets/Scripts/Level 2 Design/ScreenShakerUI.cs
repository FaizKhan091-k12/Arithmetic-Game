using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ScreenShakerUI : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("RectTransform to shake. If null, uses this component's RectTransform.")]
    [SerializeField] private RectTransform target;

    [Header("Default Shake Settings")]
    [SerializeField] private float duration = 0.20f;
    [SerializeField] private float strength = 25f;   // pixels
    [SerializeField] private int vibrato = 20;
    [SerializeField] private float randomness = 90f;
    [SerializeField] private bool fadeOut = true;

    private Vector2 originalAnchoredPos;

    void Awake()
    {
        if (!target) target = transform as RectTransform;
        if (!target) Debug.LogError("ScreenShakerUI requires a RectTransform target.");
        originalAnchoredPos = target.anchoredPosition;
    }

    void OnDisable()
    {
        if (!target) return;
        target.DOKill(true);
        target.anchoredPosition = originalAnchoredPos;
    }

    /// <summary>
    /// Shakes the UI. Pass a multiplier to scale intensity (e.g., 1.5f).
    /// </summary>
    public void Shake(float intensityMultiplier = 1f)
    {
        if (!target) return;

        // Kill current shake but DON'T complete (we want to reset to origin).
        target.DOKill(false);
        target.anchoredPosition = originalAnchoredPos;

        // Use DOShakeAnchorPos for UI
        target.DOShakeAnchorPos(
            duration,
            strength * intensityMultiplier,
            vibrato,
            randomness,
            false,
            fadeOut
        )
        .SetUpdate(false) // normal time scale
        .OnKill(() => { if (target) target.anchoredPosition = originalAnchoredPos; })
        .OnComplete(() => { if (target) target.anchoredPosition = originalAnchoredPos; });
    }
}
