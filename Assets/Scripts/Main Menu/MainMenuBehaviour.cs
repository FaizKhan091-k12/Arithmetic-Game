using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;
using Unity.VisualScripting.FullSerializer;



public class MainMenuBehaviour : MonoBehaviour
{
    public static MainMenuBehaviour Instance;

    [Header("Debug Options")]
    [SerializeField] bool isDebugging;
    [SerializeField] ParticleSystem confettiPS;
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject gameMenuPanel;
    [SerializeField] GameObject iconSymbol;
    [SerializeField] Button playBtn;

    [SerializeField] float fadeTimer;
    [SerializeField] ProceduralImage fadeScreen;
    [SerializeField] Color fadeScreenColor;
    [SerializeField] GameObject[] ui_Stars;
    [SerializeField] Button openSettingsBtn;
    [SerializeField] Button closeSettingsBtn;
    [SerializeField] GameObject settingsPanel;


    [Header("BackGround Music Settings")]
    [SerializeField] Button backGroundMusicBtn;
    [SerializeField] Slider backGroundMusicSlider;


    [SerializeField] AudioSource backGroundMusicSource;



    [Header("Sound Effects Settings")]
    [SerializeField] Button SoundMusicBtn;
    [SerializeField] Slider SoundMusicSlider;
    [SerializeField] GameObject SoundMusicSource;

    [Header("Quit Main Menu")]
    [SerializeField] Button crossButton;

    [SerializeField] Button dontQuitButton;
    [SerializeField] Button quitButton;

    [SerializeField] GameObject quitPanel;



    [SerializeField] GameObject[] cards;

    [SerializeField] GameObject mainMenuCanvas;
    [SerializeField] public  GameObject[] levelMenuCanvas;
    [SerializeField] ProceduralImage backGroundAttachment;



    bool isBackGroundMusicOn = true;
    bool isSoundMusicOn = true;

    Animator settingsPanelAnimator;
    Animator quitPanelAnimator;
    [HideInInspector]public LevelSelectDecider.ArithmeticLevel selectLevel;
    [SerializeField] public GameObject multiplyAll, divisionAll, additionAll, subtractAll;
    [SerializeField] public GameObject[] multiplyLevels;
    [SerializeField] public GameObject[] AdditionLevels;
    [SerializeField] public GameObject[] DivisionLevels;
    [SerializeField] public GameObject[] SubtractionLevels;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (isDebugging) return;
        foreach (GameObject item in multiplyLevels)
        {
            multiplyAll.SetActive(false);
            item.SetActive(false);
        }
        confettiPS.gameObject.SetActive(false);
        mainMenuPanel.SetActive(true);
        gameMenuPanel.SetActive(false);
        quitPanel.SetActive(true);
        iconSymbol.SetActive(true);
        settingsPanel.SetActive(true);

        mainMenuCanvas.SetActive(true);
        for (int i = 0; i < levelMenuCanvas.Length; i++)
        {
            levelMenuCanvas[i].SetActive(false);
        }
        backGroundAttachment.fillAmount = 0f;
        backGroundAttachment.transform.GetChild(0).GetComponent<Image>().enabled = false;
        settingsPanelAnimator = settingsPanel.GetComponent<Animator>();
        settingsPanelAnimator.Play("Close");
        openSettingsBtn.onClick.AddListener(() => settingsPanelAnimator.Play("Open"));
        closeSettingsBtn.onClick.AddListener(() => settingsPanelAnimator.Play("Close"));
        backGroundMusicBtn.onClick.AddListener(() =>
        {
            isBackGroundMusicOn = !isBackGroundMusicOn; backGroundMusicSource.volume = isBackGroundMusicOn ? 1 : 0;
            backGroundMusicSlider.value = isBackGroundMusicOn ? 1 : 0;

        });

        SoundMusicBtn.onClick.AddListener(() =>
        {
            isSoundMusicOn = !isSoundMusicOn; SoundMusicSource.SetActive(isSoundMusicOn);
            SoundMusicSlider.value = isSoundMusicOn ? 1 : 0;
        });

        quitPanelAnimator = quitPanel.GetComponent<Animator>();
        quitPanelAnimator.Play("Close");


        crossButton.onClick.AddListener(() => quitPanelAnimator.Play("Open"));
        dontQuitButton.onClick.AddListener(() => quitPanelAnimator.Play("Close"));
        quitButton.onClick.AddListener(() => Debug.Log("Application Exited"));
        playBtn.onClick.AddListener(() => PlayButtonClicked());



        foreach (GameObject star in ui_Stars)
        {
            star.SetActive(false);
        }
        foreach (GameObject card in cards)
        {
            card.gameObject.GetComponent<Button>().interactable = false;
            card.transform.localScale = Vector3.zero;
            card.SetActive(false);
        }
        StartCoroutine(StarsActivator());
    }


    public void BackButtonClicked()
    {

        //fadeScreen.gameObject.SetActive(true);
        fadeScreen.raycastTarget = true;

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(true);
        }
        StartCoroutine(FadeInForMainManu());

    }

    IEnumerator FadeInForMainManu()
    {

        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;

            fadeScreen.color = fadeScreenColor;
            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(0, 1, t);

            fadeScreen.color = tempColor;

            yield return null;
        }
        StartCoroutine(FadeOutForMainMenu());


    }

    IEnumerator FadeOutForMainMenu()
    {
        mainMenuCanvas.SetActive(true);
    for (int i = 0; i < levelMenuCanvas.Length; i++)
        {
            levelMenuCanvas[i].SetActive(false);
        }
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;

            fadeScreen.color = fadeScreenColor;
            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(1, 0, t);

            fadeScreen.color = tempColor;

            yield return null;

        }
        fadeScreen.raycastTarget = false;
          StartCoroutine(LevelPanelSelectorPanel());



    }


    IEnumerator StarsActivator()
    {

        foreach (GameObject star in ui_Stars)
        {
            star.gameObject.SetActive(true);

            float randomNumber = UnityEngine.Random.Range(.2f, .5f);
            yield return new WaitForSeconds(randomNumber);
        }


    }

    public void PlayButtonClicked()
    {
        StartCoroutine(GoToGameMenu());
    }
    IEnumerator GoToGameMenu()
    {

        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;

            fadeScreen.color = fadeScreenColor;
            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(0, 1, t);

            fadeScreen.color = tempColor;

            yield return null;

        }

        StartCoroutine(IntheGameMenu());



    }

    IEnumerator IntheGameMenu()
    {
        mainMenuPanel.SetActive(false);
        gameMenuPanel.SetActive(true);
        iconSymbol.SetActive(false);

        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;

            fadeScreen.color = fadeScreenColor;
            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(1, 0, t);

            fadeScreen.color = tempColor;

            yield return null;

        }


        StartCoroutine(LevelPanelSelectorPanel());
        confettiPS.gameObject.SetActive(true);

    }

    IEnumerator LevelPanelSelectorPanel()
    {


        for (int i = 0; i < cards.Length; i++)
        {
            yield return new WaitForSeconds(.4f);
            cards[i].SetActive(true);
            cards[i].transform.DOScale(Vector3.one, .4f).SetEase(Ease.InOutFlash);

        }
        //fadeScreen.gameObject.SetActive(false);
        fadeScreen.raycastTarget = false;
        foreach (GameObject card in cards)
        {

            card.gameObject.GetComponent<Button>().interactable = true;

        }
        BackGroundAttachment();

    }

    public void LevelSelectedButtonClicked()
    {
        StartCoroutine(StartLevelScreenFadeIn());
    }

    IEnumerator StartLevelScreenFadeIn()
    {
        //fadeScreen.gameObject.SetActive(true);
        fadeScreen.raycastTarget = true;
        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;

            fadeScreen.color = fadeScreenColor;
            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(0, 1, t);

            fadeScreen.color = tempColor;

            yield return null;

        }

        StartCoroutine(InsideLevelScreenFadeOut());



    }
    IEnumerator InsideLevelScreenFadeOut()
    {

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].gameObject.SetActive(false);
        }

        mainMenuCanvas.SetActive(false);
        if (selectLevel == LevelSelectDecider.ArithmeticLevel.MultiplyLevel)
        {
            levelMenuCanvas[0].SetActive(true);
        }
        else if (selectLevel == LevelSelectDecider.ArithmeticLevel.AdditionLevel)
        {
            levelMenuCanvas[1].SetActive(true);
        }
        else if (selectLevel == LevelSelectDecider.ArithmeticLevel.SubtractionLevel)
        {
            levelMenuCanvas[2].SetActive(true);
        }
        else if (selectLevel == LevelSelectDecider.ArithmeticLevel.DivisionLevel)
        {
            levelMenuCanvas[3].SetActive(true);
        }


        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;

            fadeScreen.color = fadeScreenColor;
            Color tempColor = fadeScreen.color;
            tempColor.a = Mathf.Lerp(1, 0, t);

            fadeScreen.color = tempColor;

            yield return null;

        }

        // fadeScreen.gameObject.SetActive(false);
        fadeScreen.raycastTarget = false;

        confettiPS.gameObject.SetActive(false);



    }



    public void BackGroundAttachment()
    {
        StartCoroutine(BackGroundAttachmentRadius());
    }

    IEnumerator BackGroundAttachmentRadius()
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime * fadeTimer;
            float tempFillAmount = backGroundAttachment.fillAmount;

            tempFillAmount = Mathf.Lerp(0, 1, t);

            backGroundAttachment.fillAmount = tempFillAmount;

            yield return null;

        }

        backGroundAttachment.enabled = false;
        backGroundAttachment.transform.GetChild(0).GetComponent<Image>().enabled = true;
    }

    public void OnLevelSelector(LevelSelectDecider.ArithmeticLevel level)
    {

        selectLevel = level;


        Debug.Log(selectLevel);

    }

}
