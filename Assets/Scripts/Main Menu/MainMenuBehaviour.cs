using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;


public class MainMenuBehaviour : MonoBehaviour
{

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


    bool isBackGroundMusicOn = true;
    bool isSoundMusicOn = true;

    Animator settingsPanelAnimator;
    Animator quitPanelAnimator;
    void Start()
    {
        mainMenuPanel.SetActive(true);
        gameMenuPanel.SetActive(false);
        quitPanel.SetActive(true);
        iconSymbol.SetActive(true);
        settingsPanel.SetActive(true);
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
            card.SetActive(false);
            card.transform.localScale = Vector3.zero;
        }
        StartCoroutine(StarsActivator());
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
        Debug.Log("Fading in Completed");


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
        Debug.Log("Fading out Completed");
        fadeScreen.gameObject.SetActive(false);
        StartCoroutine(LevelPanelSelectorPanel());

    }

    IEnumerator LevelPanelSelectorPanel()
    {

        for (int i = 0; i < cards.Length; i++)
        {
            yield return new WaitForSeconds(.4f);
            cards[i].SetActive(true);
            cards[i].transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutFlash);
        }
    }


}
