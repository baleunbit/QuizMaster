using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    [Header("Questions")]
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] List<QuestionSO> questions = new List<QuestionSO>();
    QuestionSO currentQuestion;

    [Header("Answers")]
    [SerializeField] GameObject[] answerButtons;
    [SerializeField] TextMeshProUGUI[] answerTextArr;

    [Header("Button Color")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("Timer")]
    [SerializeField] Image timerImage;
    [SerializeField] Sprite problemTimerSprite;
    [SerializeField] Sprite solutionTimerSprite;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;
    ScoreKeeper scoreKeeper;

    [Header("ProgressBar")]
    [SerializeField] Slider progressBar;

    [Header("ChatGPT Client")]
    [SerializeField] ChatGPTClient chatGPTClient;
    [SerializeField] int questionCount = 3;

    Timer timer;
    bool chooseAnswer = false;
    bool isGeneratingQuestions = false;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClient.quizGenerateHandler += OnQuizGenerated;

        if (questions.Count == 0)
        {
            GenerateQuestionsIfNeeded();
        } 
        else
        {
            InitalizeProgressBar();
        }
    }

    private void GenerateQuestionsIfNeeded()
    {
        if (isGeneratingQuestions) return;

        isGeneratingQuestions = true;
        GameManager.Instance.ShowLoadingSceen();

        string topicToUse = GetTrendingTopic();
        chatGPTClient.GenerateQuizQuestions(questionCount, topicToUse);
        Debug.Log("Generating questions on topic:" + topicToUse);
    }

    private string GetTrendingTopic()
    {
        string[] topics = new string[]
        {
            "배틀그라운드",
            "발로란트",
            "레디 오어 낫",
            "마인크래프트",
            "레드 데드 리뎀션"
        };
        int randomIndex = UnityEngine.Random.Range(0, topics.Length);
        return topics[randomIndex];
    }

    private void OnQuizGenerated (List<QuestionSO> generatedQuestions)
    {
        Debug.Log("Quiz received with " + generatedQuestions.Count + " questions.");
        isGeneratingQuestions = false;
    }
    private void InitalizeProgressBar()
    {
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
    }

    private void Update()
    {
        if (timer.isProblemTime)
            timerImage.sprite = problemTimerSprite;
        else
            timerImage.sprite = solutionTimerSprite;
        timerImage.fillAmount = timer.fillAmount;

        if(timer.loadNextQuestion)
        {
            if (questions.Count == 0)
            {
                GenerateQuestionsIfNeeded();
                //GameManager.Instance.ShowEndSceen();
            }
            else
            {
                timer.loadNextQuestion = false;
                GetNextQuestion();
            }
        }

        if (timer.isProblemTime == false && chooseAnswer == false)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (questions.Count == 0)
        {
            Debug.Log("문제가 없습니다.");
            return;
        }

        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();
        GetRandomQuestion();
        OnDisplayQuestion();
        scoreKeeper.IncrementQuestionSeen();
        progressBar.value++;
    }

    private void GetRandomQuestion()
    {
        int randomIndex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[randomIndex];
        questions.RemoveAt(randomIndex);
    }

    private void OnDisplayQuestion()
    {
        questionText.text = currentQuestion.GetQuestion();

        for (int i = 0; i < answerTextArr.Length; i++)
        {
            answerTextArr[i].text = currentQuestion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancelTimer();
        scoreText.text = $"Score: {scoreKeeper.CalculateScore()}%";
    }

    private void DisplaySolution(int index)
    {
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            questionText.text = "정답입니다!";
            answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;
            scoreKeeper.IncrementCorrectAnswer();
        }
        else
        {
            questionText.text = $"오답입니다! 정답은 {currentQuestion.GetCorrectAnswer()}입니다.";
        }
        SetButtonState(false);
    }

    private void SetDefaultButtonSprites()
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Image>().sprite = defaultAnswerSprite;
        }
    }
    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }
}
