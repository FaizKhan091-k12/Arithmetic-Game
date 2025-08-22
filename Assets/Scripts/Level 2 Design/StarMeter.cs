using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StarMeter : MonoBehaviour
{
    [Header("Stars (UI Images must be Type=Filled)")]
    [SerializeField] private Image[] stars; // length = 3

    [Header("Milestones (cumulative correct answers)")]
    [SerializeField] private int[] thresholds = { 5, 15, 25 }; // 1st, 2nd, 3rd star

    [Header("Intermediate Icons")]
    [SerializeField] private GameObject[] inter_YellowStar; // length = 3 (0..2)
    [SerializeField] private GameObject[] inter_GrayStar;   // length = 3 (0..2)

    [Header("End Game UI")]
    [SerializeField] private GameObject[] endgame_Stars; // icons to show at the end
    [SerializeField] private GameObject endGame;         // panel to show at the end

    [Header("Tweens")]
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private float popScale = 1.2f;

    private int correctCount = 0;

    void Awake()
    {
        // Defensive: ensure arrays exist
        if (stars != null)
        {
            foreach (var img in stars)
            {
                if (!img) continue;
                img.type = Image.Type.Filled;
                img.fillAmount = 0f;
            }
        }
        ResetUIState();
    }

    public void ResetMeter()
    {
        correctCount = 0;

        if (stars != null)
        {
            foreach (var img in stars)
            {
                if (!img) continue;
                img.DOKill(true);
                img.fillAmount = 0f;
                if (img.transform is RectTransform rt) rt.localScale = Vector3.one;
            }
        }

        ResetUIState();
    }

    public void AddCorrect()
    {
        correctCount++;
        CheckStarMilestones();
    }

    public int CurrentCorrectCount() => correctCount;

    // ---------------- helpers ----------------

    private void ResetUIState()
    {
        // Hide endgame UI
        if (endgame_Stars != null)
            foreach (var g in endgame_Stars) if (g) g.SetActive(false);
        if (endGame) endGame.SetActive(false);

        // Intermediate: show 0 yellow, 3 gray
        SetInterStars(yellowCount: 0);
    }

    private void SetInterStars(int yellowCount)
    {
        // Clamp yellowCount to [0,3]
        yellowCount = Mathf.Clamp(yellowCount, 0, 3);

        for (int i = 0; i < 3; i++)
        {
            bool isYellow = i < yellowCount;

            if (inter_YellowStar != null && i < inter_YellowStar.Length && inter_YellowStar[i])
                inter_YellowStar[i].SetActive(isYellow);

            if (inter_GrayStar != null && i < inter_GrayStar.Length && inter_GrayStar[i])
                inter_GrayStar[i].SetActive(!isYellow);
        }
    }

    private void CheckStarMilestones()
    {
        // Guard thresholds length
        if (thresholds == null || thresholds.Length < 3) return;

        // 1st star
        if (correctCount == thresholds[0])
        {
            FillStar(0);
            SetInterStars(1);
        }

        // 2nd star
        if (correctCount == thresholds[1])
        {
            FillStar(1);
            SetInterStars(2);
        }

        // 3rd star + end game
        if (correctCount == thresholds[2])
        {
            FillStar(2);
            SetInterStars(3);

            if (endgame_Stars != null)
                foreach (var g in endgame_Stars) if (g) g.SetActive(true);

            if (endGame) endGame.SetActive(true);
        }
    }

    private void FillStar(int index)
    {
        if (stars == null || index < 0 || index >= stars.Length) return;
        var img = stars[index];
        if (!img) return;

        img.DOKill();
        img.DOFillAmount(1f, tweenDuration).SetEase(Ease.OutQuad);

        if (img.transform is RectTransform rt)
        {
            rt.DOKill();
            rt.localScale = Vector3.one;
            rt.DOPunchScale(Vector3.one * (popScale - 1f), tweenDuration, 10)
              .OnKill(() => rt.localScale = Vector3.one)
              .OnComplete(() => rt.localScale = Vector3.one);
        }
    }
}
