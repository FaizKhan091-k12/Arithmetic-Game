using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Ricimi;
using TMPro;

public class StartLevelInit : MonoBehaviour
{


    [SerializeField] ProceduralImage fadeScreen;
    [SerializeField] int leveltoStart;
    [SerializeField] float fadeSpeed;
    [SerializeField] Transform canvas;
    [SerializeField] TextMeshProUGUI levelText;
    Animator anim;


    void OnEnable()
    {

        fadeScreen = FindFirstObjectByType<FadeScreen>().GetComponent<ProceduralImage>();
        fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, 0f);
        anim = GetComponent<Animator>();
         
    
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        levelText.text = "Level " + MultiplyLevel.Instance.levelNum;
        
    }
    public void WhichLevelToStart()
    {
        leveltoStart = MultiplyLevel.Instance.levelNum;
        fadeScreen.transform.SetParent(FindFirstObjectByType<MultiplyLevel>().transform);
        StartCoroutine(FadeIntoGame());
        anim.Play("Close");
        fadeScreen.raycastTarget = true;
    }



    IEnumerator FadeIntoGame()
    {
             fadeScreen.transform.SetParent(FindFirstObjectByType<Canvas>().transform);
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;

            Color tempColor = fadeScreen.color;

            tempColor.a = Mathf.Lerp(0, 1, t);

            fadeScreen.color = tempColor;

            yield return null;

        }
        StartCoroutine(FadeIntoGameLevel());

    }

    IEnumerator FadeIntoGameLevel()
    {
    GameObject backGround = GameObject.Find("PopupBackground");
        Destroy(backGround, 1.0f);
        if (leveltoStart == 1)
        {
            Debug.Log("Let's Start Level 1 ");
        }
        if (leveltoStart == 2)
        {

            Debug.Log("Let's Start Level 2 ");
        }
        if (leveltoStart == 3)
        {

            Debug.Log("Let's Start Level 3 ");
        }
        yield return new WaitForSeconds(1f);
        float i = 0f;

        while (i < 1)
        {
            i += Time.deltaTime * fadeSpeed;

            Color tempColor = fadeScreen.color;

            tempColor.a = Mathf.Lerp(1, 0, i);

            fadeScreen.color = tempColor;

            yield return null;

        }


        fadeScreen.raycastTarget = false;
    
        Destroy(gameObject, 1.0f);

    }
 
 
}
