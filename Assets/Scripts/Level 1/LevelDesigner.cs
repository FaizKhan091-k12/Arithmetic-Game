using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class LevelDesigner : MonoBehaviour
{

    public static LevelDesigner Instance;
    [SerializeField] int firstQuestionNumber, secondQuestionNumber, finalAnswerNumber;
    [SerializeField] TextMeshProUGUI[] txt_AnswerGuessers;
    [SerializeField] TextMeshProUGUI txt_FirstNumber, txt_SecondNumber, txt_FinalAnswer;
    [SerializeField] int stars_Earn;
    [SerializeField] GameObject[] yellow_Starts;
    [SerializeField] public int stars;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] AudioSource scoreStarClip;
    [SerializeField] BalloonBehaviour[] balloonBehaviours;
    [SerializeField] GameObject endGameWindow;

    [Header("EndGame References")]
    [SerializeField] GameObject[] end_YellowStars;
    [SerializeField] GameObject[] end_GrayStars;

    [Header("Basic Level Stars")]
    [SerializeField] GameObject[] basic_YellowStars;
    [SerializeField] GameObject[] basic_GrayStars;

    [Header("Score Board")]
    [SerializeField] GameObject[] score_Board_Stars;

    public bool test;

    int correctIndex;
    public LevelSelectDecider.ArithmeticLevel selectLevel;
    [SerializeField] public GameObject handIconAnim;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        NextQuestion();
        test = false;
        stars = 0;
 
      
        foreach (GameObject item in score_Board_Stars)
        {
            item.SetActive(false);
        }
        endGameWindow.SetActive(false);
        foreach (BalloonBehaviour item in balloonBehaviours)
        {
            item.enabled = true;
        }
        handIconAnim.SetActive(true);
    }

    void Update()
    {
        EndGameStars();

    }

    void EndGameStars()
    {

        switch (stars)
        {
            case 0:
                foreach (GameObject item in basic_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in basic_GrayStars)
                {
                    item.SetActive(true);

                }
                break;
            case 1:
                foreach (GameObject item in end_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in end_GrayStars)
                {
                    item.SetActive(false);
                }

                end_YellowStars[0].SetActive(true);
                end_GrayStars[1].SetActive(true);
                end_GrayStars[2].SetActive(true);

                //Basic Stars

                foreach (GameObject item in basic_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in basic_GrayStars)
                {
                    item.SetActive(false);
                }

                basic_YellowStars[0].SetActive(true);
                basic_GrayStars[1].SetActive(true);
                basic_GrayStars[2].SetActive(true);
                break;
            case 2:
                foreach (GameObject item in end_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in end_GrayStars)
                {
                    item.SetActive(false);
                }

                end_YellowStars[0].SetActive(true);
                end_YellowStars[1].SetActive(true);

                end_GrayStars[2].SetActive(true);

                //Basic Stars

                foreach (GameObject item in basic_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in basic_GrayStars)
                {
                    item.SetActive(false);
                }

                basic_YellowStars[0].SetActive(true);
                basic_YellowStars[1].SetActive(true);

                basic_GrayStars[2].SetActive(true);
                break;
            case 3:

                foreach (GameObject item in end_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in end_GrayStars)
                {
                    item.SetActive(false);
                }

                end_YellowStars[0].SetActive(true);
                end_YellowStars[1].SetActive(true);
                end_YellowStars[2].SetActive(true);
                //Basic Stars


                foreach (GameObject item in basic_YellowStars)
                {
                    item.SetActive(false);

                }
                foreach (GameObject item in basic_GrayStars)
                {
                    item.SetActive(false);
                }

                basic_YellowStars[0].SetActive(true);
                basic_YellowStars[1].SetActive(true);
                basic_YellowStars[2].SetActive(true);


                break;


        }
    }

    public void LevelSwitcher()
    {
        if (stars >= 3)
        {
            test = true;
            Debug.Log("Level 1 Clear");
            endGameWindow.SetActive(true);

            particleSystem.Play();
            scoreStarClip.Play();
            foreach (BalloonBehaviour item in balloonBehaviours)
            {
                item.enabled = false;
            }
            return;
        }

        stars++;

        Invoke(nameof(NextQuestion), 2f);
        if (stars == 1)
        {
            yellow_Starts[0].SetActive(true);
            particleSystem.Play();
            scoreStarClip.Play();


        }
        else if (stars == 2)
        {
            yellow_Starts[1].SetActive(true);
            particleSystem.Play();
            scoreStarClip.Play();

        }
        else if (stars == 3)
        {
            yellow_Starts[2].SetActive(true);
            particleSystem.Play();
            scoreStarClip.Play();

        }


    }



    void ResetAllOptions()
    {
        foreach (var t in txt_AnswerGuessers)
        {
            if (!t) continue;

            // Clear previous text
            t.text = "";

            // Reset the balloon that holds this text
            var balloon = t.GetComponentInParent<BalloonBehaviour>(true);
            if (balloon)
            {
                balloon.ResetToStart(reenableAnimator: true);
            }
        }
    }

    void GenerateQuestion()
    {
        int a = 0, b = 0;

        switch (selectLevel)
        {
            case LevelSelectDecider.ArithmeticLevel.AdditionLevel:
                a = Random.Range(1, 11);   // 1..10
                b = Random.Range(1, 11);   // 1..10
                finalAnswerNumber = a + b;
                break;

            case LevelSelectDecider.ArithmeticLevel.SubtractionLevel:
                a = Random.Range(2, 11);   // 2..10
                b = Random.Range(1, a);    // ensure b < a
                finalAnswerNumber = a - b;
                break;

            case LevelSelectDecider.ArithmeticLevel.MultiplyLevel:
                a = Random.Range(1, 11);   // 1..10
                b = Random.Range(1, 11);   // 1..10
                finalAnswerNumber = a * b;
                break;

            case LevelSelectDecider.ArithmeticLevel.DivisionLevel:
                b = Random.Range(1, 11);   // divisor (1..10)
                finalAnswerNumber = Random.Range(1, 11); // quotient (1..10)
                a = finalAnswerNumber * b; // dividend ensures no remainder
                break;
        }

        firstQuestionNumber = a;
        secondQuestionNumber = b;

        // Update UI
        txt_FirstNumber.text = firstQuestionNumber.ToString();
        txt_SecondNumber.text = secondQuestionNumber.ToString();
    }

    void GenerateAnswers()
    {
        // Reset answers
        foreach (var t in txt_AnswerGuessers)
        {
            t.text = "";
            var balloon = t.GetComponentInParent<BalloonBehaviour>();
            if (balloon) balloon.isCorrectAnswer = false;
        }

        // Place correct answer
        correctIndex = Random.Range(0, txt_AnswerGuessers.Length);
        txt_AnswerGuessers[correctIndex].text = finalAnswerNumber.ToString();
        txt_AnswerGuessers[correctIndex].GetComponentInParent<BalloonBehaviour>().isCorrectAnswer = true;

        // Generate distractors
        var used = new HashSet<int> { finalAnswerNumber };

        for (int i = 0; i < txt_AnswerGuessers.Length; i++)
        {
            if (i == correctIndex) continue;

            int distractor = GenerateDistractor(finalAnswerNumber, firstQuestionNumber, secondQuestionNumber, used);
            used.Add(distractor);
            txt_AnswerGuessers[i].text = distractor.ToString();
        }
    }

    int GenerateDistractor(int correct, int a, int b, HashSet<int> used)
    {
        int pick = 0;

        for (int attempts = 0; attempts < 20; attempts++)
        {
            switch (selectLevel)
            {
                case LevelSelectDecider.ArithmeticLevel.AdditionLevel:
                    {
                        int mode = Random.Range(0, 3);
                        switch (mode)
                        {
                            case 0: pick = correct + Random.Range(-3, 4); break; // near miss
                            case 1: pick = (a + 1) + b; break;                 // off by one on a
                            case 2: pick = a + (b + 1); break;                 // off by one on b
                        }
                        break;
                    }

                case LevelSelectDecider.ArithmeticLevel.SubtractionLevel:
                    {
                        int mode = Random.Range(0, 3);
                        switch (mode)
                        {
                            case 0: pick = correct + Random.Range(-3, 4); break;
                            case 1: pick = a + b; break;                         // mistake: added instead of subtracted
                            case 2: pick = (a - (b + 1)); break;                 // off-by-one
                        }
                        break;
                    }

                case LevelSelectDecider.ArithmeticLevel.MultiplyLevel:
                    {
                        int mode = Random.Range(0, 4);
                        switch (mode)
                        {
                            case 0: pick = correct + Random.Range(-10, 11); break; // near miss
                            case 1: pick = (a + 1) * b; break;
                            case 2: pick = a * (b + 1); break;
                            case 3: pick = (a - 1) * b; break;
                        }
                        break;
                    }

                case LevelSelectDecider.ArithmeticLevel.DivisionLevel:
                    {
                        int mode = Random.Range(0, 3);
                        switch (mode)
                        {
                            case 0: pick = correct + Random.Range(-2, 3); break; // near miss
                            case 1: pick = a - b; break;                         // subtraction mistake
                            case 2: pick = b; break;                             // confuse divisor with result
                        }
                        break;
                    }
            }

            if (pick > 0 && pick != correct && !used.Contains(pick))
                return pick;
        }

        // fallback unique number
        int fallback = correct + 1;
        while (used.Contains(fallback)) fallback++;
        return fallback;
    }

    public void NextQuestion()
    {
        GenerateQuestion();
        GenerateAnswers();
    }

    public int GetCorrectIndex() => correctIndex;
}