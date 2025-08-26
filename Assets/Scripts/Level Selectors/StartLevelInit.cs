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
    [SerializeField] TextMeshProUGUI objective_Txt;
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

        // levelText.text = "Level " + LevelNumberSelector.Instance.commonLevelNum;
        if (LevelNumberSelector.Instance.commonLevelNum == 1)
        {
            levelText.text = "BASICS";
            objective_Txt.text = "Let's Test Your Skills.Try to Earn Three Stars";
        }
        else if (LevelNumberSelector.Instance.commonLevelNum == 2)
        {
            levelText.text = "BEGINNER";
            objective_Txt.text = "Ohh! So You Think You Are Above Basic.Let's Check it.";
        }
        else if (LevelNumberSelector.Instance.commonLevelNum == 3)
        {
            levelText.text = "EXPERT";
            objective_Txt.text = "If You Are Not A Beginner.Then Check Your Skills Here.";
        }
        else if (LevelNumberSelector.Instance.commonLevelNum == 4)
        {
            levelText.text = "EXPERT";
            objective_Txt.text = "You Love Challenges.Try One Here";
        }


    }
    public void WhichLevelToStart()
    {

        if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.MultiplyLevel)
        {
            Debug.Log("Its Multi");
            leveltoStart = LevelNumberSelector.Instance.multiLevelNum;
            fadeScreen.transform.SetParent(FindFirstObjectByType<LevelNumberSelector>().transform);
            StartCoroutine(FadeIntoGame());
            anim.Play("Close");
            fadeScreen.raycastTarget = true;
        }
        else if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.AdditionLevel)
        {
            Debug.Log("Its Add");
            leveltoStart = LevelNumberSelector.Instance.addLevelNum;
            fadeScreen.transform.SetParent(FindFirstObjectByType<LevelNumberSelector>().transform);
            StartCoroutine(FadeIntoGame());
            anim.Play("Close");
            fadeScreen.raycastTarget = true;
        }
        else if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.SubtractionLevel)
        {
            Debug.Log("Its Sub");
            leveltoStart = LevelNumberSelector.Instance.subtractLevelNum;
            fadeScreen.transform.SetParent(FindFirstObjectByType<LevelNumberSelector>().transform);
            StartCoroutine(FadeIntoGame());
            anim.Play("Close");
            fadeScreen.raycastTarget = true;
        }
        else if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.DivisionLevel)
        {
            Debug.Log("Its Div");
            leveltoStart = LevelNumberSelector.Instance.divisionLevelNum;
            fadeScreen.transform.SetParent(FindFirstObjectByType<LevelNumberSelector>().transform);
            StartCoroutine(FadeIntoGame());
            anim.Play("Close");
            fadeScreen.raycastTarget = true;
        }

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

        if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.MultiplyLevel)
        {
            MainMenuBehaviour.Instance.levelMenuCanvas[0].SetActive(false);

            MultiLevelNumber();

        }
        else if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.AdditionLevel)
        {
            MainMenuBehaviour.Instance.levelMenuCanvas[1].SetActive(false);
            AdditionLevelNumber();

        }
        else if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.SubtractionLevel)
        {
            MainMenuBehaviour.Instance.levelMenuCanvas[2].SetActive(false);
            SubtractionLevelNumber();

        }

        else if (MainMenuBehaviour.Instance.selectLevel == LevelSelectDecider.ArithmeticLevel.DivisionLevel)
        {
            MainMenuBehaviour.Instance.levelMenuCanvas[3].SetActive(false);
            DivisionLevelNumber();
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

    public void MultiLevelNumber()
    {
        BackButton.Instance.isMulti = true;
        BackButton.Instance.isAdd = false;
        BackButton.Instance.isDiv = false;
        BackButton.Instance.isSub = false;
        switch (leveltoStart)
        {
            case 1:
                Debug.Log("Let's Start Level 1 multiply ");
                MainMenuBehaviour.Instance.multiplyAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.multiplyLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.multiplyLevels[0].SetActive(true);
                break;
            case 2:
                Debug.Log("Let's Start Level 2 multiply");
                MainMenuBehaviour.Instance.multiplyAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.multiplyLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.multiplyLevels[1].SetActive(true);
                break;
            case 3:
                   Debug.Log("Let's Start Level 3 multiply");
                MainMenuBehaviour.Instance.multiplyAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.multiplyLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.multiplyLevels[2].SetActive(true);
                break;
            case 4:
                Debug.Log("Let's Start Level 4 ");
                break;
            case 5:
                Debug.Log("Let's Start Level 5 ");
                break;
            case 6:
                Debug.Log("Let's Start Level 6 ");
                break;
            case 7:
                Debug.Log("Let's Start Level 7 ");
                break;
            case 8:
                Debug.Log("Let's Start Level 8 ");
                break;
            case 9:
                Debug.Log("Let's Start Level 9 ");
                break;
            case 10:
                Debug.Log("Let's Start Level 10 ");
                break;
        }
    }

    public void AdditionLevelNumber()
    {
        BackButton.Instance.isMulti = false;
        BackButton.Instance.isAdd = true;
        BackButton.Instance.isDiv = false;
        BackButton.Instance.isSub = false;
        switch (leveltoStart)
        {
            case 1:
                Debug.Log("Let's Start Level 1 Add");
                MainMenuBehaviour.Instance.additionAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.AdditionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.AdditionLevels[0].SetActive(true);
                break;
            case 2:

                MainMenuBehaviour.Instance.additionAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.AdditionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.AdditionLevels[1].SetActive(true);
                Debug.Log("Let's Start Level 2 Add ");
                break;
            case 3:
                   MainMenuBehaviour.Instance.additionAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.AdditionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.AdditionLevels[2].SetActive(true);
                Debug.Log("Let's Start Level 2 Add ");
                break;
            case 4:
                Debug.Log("Let's Start Level 4 ");
                break;
            case 5:
                Debug.Log("Let's Start Level 5 ");
                break;
            case 6:
                Debug.Log("Let's Start Level 6 ");
                break;
            case 7:
                Debug.Log("Let's Start Level 7 ");
                break;
            case 8:
                Debug.Log("Let's Start Level 8 ");
                break;
            case 9:
                Debug.Log("Let's Start Level 9 ");
                break;
            case 10:
                Debug.Log("Let's Start Level 10 ");
                break;
        }
    }

    public void SubtractionLevelNumber()
    {
        BackButton.Instance.isMulti = false;
        BackButton.Instance.isAdd = false;
        BackButton.Instance.isDiv = false;
        BackButton.Instance.isSub = true;
        switch (leveltoStart)
        {
            case 1:
                Debug.Log("Let's Start Level 1 Subtraction");
                MainMenuBehaviour.Instance.subtractAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.SubtractionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.SubtractionLevels[0].SetActive(true);
                break;
            case 2:
                Debug.Log("Let's Start Level 2 Subtraction ");
                MainMenuBehaviour.Instance.subtractAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.SubtractionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.SubtractionLevels[1].SetActive(true);
                break;
            case 3:
                   Debug.Log("Let's Start Level 3 Subtraction ");
                MainMenuBehaviour.Instance.subtractAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.SubtractionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.SubtractionLevels[2].SetActive(true);
                break;
            case 4:
                Debug.Log("Let's Start Level 4 ");
                break;
            case 5:
                Debug.Log("Let's Start Level 5 ");
                break;
            case 6:
                Debug.Log("Let's Start Level 6 ");
                break;
            case 7:
                Debug.Log("Let's Start Level 7 ");
                break;
            case 8:
                Debug.Log("Let's Start Level 8 ");
                break;
            case 9:
                Debug.Log("Let's Start Level 9 ");
                break;
            case 10:
                Debug.Log("Let's Start Level 10 ");
                break;
        }
    }
    public void DivisionLevelNumber()
    {
        BackButton.Instance.isMulti = false;
        BackButton.Instance.isAdd = false;
        BackButton.Instance.isDiv = true;
        BackButton.Instance.isSub = false;
        switch (leveltoStart)
        {
            case 1:
                Debug.Log("Let's Start Level 1 Division");
                MainMenuBehaviour.Instance.divisionAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.DivisionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.DivisionLevels[0].SetActive(true);
                break;
            case 2:
                Debug.Log("Let's Start Level 2 Division ");
                MainMenuBehaviour.Instance.divisionAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.DivisionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.DivisionLevels[1].SetActive(true);

                break;
            case 3:
                        Debug.Log("Let's Start Level 3 Division ");
                MainMenuBehaviour.Instance.divisionAll.SetActive(true);
                foreach (GameObject item in MainMenuBehaviour.Instance.DivisionLevels)
                {
                    item.SetActive(false);
                }
                MainMenuBehaviour.Instance.DivisionLevels[2].SetActive(true);
                break;
            case 4:
                Debug.Log("Let's Start Level 4 ");
                break;
            case 5:
                Debug.Log("Let's Start Level 5 ");
                break;
            case 6:
                Debug.Log("Let's Start Level 6 ");
                break;
            case 7:
                Debug.Log("Let's Start Level 7 ");
                break;
            case 8:
                Debug.Log("Let's Start Level 8 ");
                break;
            case 9:
                Debug.Log("Let's Start Level 9 ");
                break;
            case 10:
                Debug.Log("Let's Start Level 10 ");
                break;
        }
    }
}
