using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class BackButton : MonoBehaviour
{
    [SerializeField] Button backBtn;
    [SerializeField] ProceduralImage fadeScreen;
    [SerializeField] float fadeTimer = 1f;
    [SerializeField] GameObject[] currentLevel;
    [SerializeField] GameObject levelSelectorPanel;


    public void OnBackBtnClicked()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadeScreen.raycastTarget = true;
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;


            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(0, 1, t);

            fadeScreen.color = tempColor;

            yield return null;

        }
        StartCoroutine(FadeOut());

    }

    IEnumerator FadeOut()
    {
        foreach (GameObject item in currentLevel)
        {
            item.SetActive(false);  
        }
        levelSelectorPanel.SetActive(true);
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;


            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(1, 0, t);

            fadeScreen.color = tempColor;

            yield return null;

        }
        fadeScreen.raycastTarget = false;
    }
}
