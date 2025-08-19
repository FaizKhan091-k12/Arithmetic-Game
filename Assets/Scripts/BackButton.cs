using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class BackButton : MonoBehaviour
{
    public static BackButton Instance;
    [SerializeField] Button backBtn;
    [SerializeField] ProceduralImage fadeScreen;
    [SerializeField] float fadeTimer = 1f;
    [SerializeField] GameObject[] currentLevel;
    [SerializeField] GameObject add,multi,div,sub;

    public bool isAdd, isSub, isMulti, isDiv;


    void Awake()
    {
        Instance = this;
    }
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
        if (isAdd)
        {
            add.SetActive(true);
            multi.SetActive(false);
            div.SetActive(false);
            sub.SetActive(false);
        }
        else if (isMulti)
        {
            multi.SetActive(true);
            add.SetActive(false);
            div.SetActive(false);
            sub.SetActive(false);
        }
        else if (isDiv)
        {
            multi.SetActive(false);
            add.SetActive(false);
            div.SetActive(true);
            sub.SetActive(false);
        }
        else if (isSub)
        {
            multi.SetActive(false);
            add.SetActive(false);
            div.SetActive(false);
            sub.SetActive(true);
        }
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
