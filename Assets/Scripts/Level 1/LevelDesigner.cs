using System.Collections.Generic;
using Ricimi;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class LevelDesigner : MonoBehaviour
{

    public static LevelDesigner Instance;
    [SerializeField] int firstQuestionNumber, secondQuestionNumber, finalAnswerNumber;
    [SerializeField] TextMeshProUGUI[] txt_AnswerGuessers;
    [SerializeField] TextMeshProUGUI txt_FirstNumber, txt_SecondNumber, txt_FinalAnswer;
    [SerializeField] int stars_Earn;
    [SerializeField] GameObject[] yellow_Starts;
    [SerializeField] public  int stars;
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

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        NextQuestion();
        test = false;
        stars = 0;
        foreach (GameObject item in end_YellowStars)
        {
            item.SetActive(false);
        }
        foreach (GameObject item in basic_YellowStars)
        {
            item.SetActive(false);
        }
        foreach (GameObject item in score_Board_Stars)
        {
            item.SetActive(false);
        }
        endGameWindow.SetActive(false);
         foreach (BalloonBehaviour item in balloonBehaviours)
            {
                item.enabled = true;
            }
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
        firstQuestionNumber = Random.Range(1, 10);
        secondQuestionNumber = Random.Range(1, 11);

        txt_FirstNumber.text = firstQuestionNumber.ToString();
        txt_SecondNumber.text = secondQuestionNumber.ToString();

        if (selectLevel == LevelSelectDecider.ArithmeticLevel.MultiplyLevel)
        {
            finalAnswerNumber = firstQuestionNumber * secondQuestionNumber;
            Debug.Log("This Is Multiply Level");
        }
        else if (selectLevel == LevelSelectDecider.ArithmeticLevel.AdditionLevel)
        {
            Debug.Log("This is Addition Level");
            finalAnswerNumber = firstQuestionNumber + secondQuestionNumber;
        }
        else if (selectLevel == LevelSelectDecider.ArithmeticLevel.DivisionLevel)
        {
            Debug.Log("This is Division Level");
            finalAnswerNumber = firstQuestionNumber / secondQuestionNumber;
        }
        else if (selectLevel == LevelSelectDecider.ArithmeticLevel.SubtractionLevel)
        {
            Debug.Log("This is Subtraction Level");
            finalAnswerNumber = firstQuestionNumber - secondQuestionNumber;

        }

    }

    void GenerateAnswers()
    {
        // Clear first (optional)
        foreach (var t in txt_AnswerGuessers) t.text = "";

        // Pick where the correct answer will go
        correctIndex = Random.Range(0, txt_AnswerGuessers.Length);
        txt_AnswerGuessers[correctIndex].text = finalAnswerNumber.ToString();
        txt_AnswerGuessers[correctIndex].gameObject.transform.GetComponentInParent<BalloonBehaviour>().isCorrectAnswer = true;

        // Fill other slots with unique distractors
        var used = new HashSet<int> { finalAnswerNumber };

        for (int i = 0; i < txt_AnswerGuessers.Length; i++)
        {
            if (i == correctIndex) continue;

            int distractor = GenerateDistractor(finalAnswerNumber, firstQuestionNumber, secondQuestionNumber, used);
            used.Add(distractor);
            txt_AnswerGuessers[i].text = distractor.ToString();
        }

     
    }

    // Generates a plausible wrong answer that isn't already used
    int GenerateDistractor(int correct, int a, int b, HashSet<int> used)
    {
        // Try a handful of heuristics for believable mistakes
        for (int attempts = 0; attempts < 20; attempts++)
        {
            int pick = 0;
            int mode = Random.Range(0, 6);

            switch (mode)
            {
                case 0: pick = correct + Random.Range(-10, 11); break;          // near miss
                case 1: pick = (a + 1) * b; break;                               // off-by-one factor
                case 2: pick = a * (b + 1); break;
                case 3: pick = (a - 1) * b; break;
                case 4: pick = a * (b - 1); break;
                case 5: pick = a + b; break;                                     // common mistake (add)
            }

            // Clamp to sensible range and validate
            if (pick < 1) continue;
            if (pick == correct) continue;
            if (used.Contains(pick)) continue;

            return pick;
        }

        // Fallback: find the next unused integer near the correct answer
        int fallback = correct + 1;
        while (fallback < 1000 && used.Contains(fallback)) fallback++;
        return fallback;
    }

    // Optional: call this to roll a new question+answers
    public void NextQuestion()
    {
        GenerateQuestion();
        GenerateAnswers();
    }

    // Optional: getter if you need to check which index is correct
    public int GetCorrectIndex() => correctIndex;
}
