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
    private QuestionSO currentQuestion;

    [Header("Answers")]
    [SerializeField] GameObject[] answerButtons;
    private bool chooseAnswer = false;

    [Header("Button Color")]
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite correctAnswerSprite;

    [Header("Timer")]
    [SerializeField] Image timerImage;
    [SerializeField] Sprite problemTimerSprite;
    [SerializeField] Sprite solutionTimerSprite;
    [SerializeField] TextMeshProUGUI timerText;   // 남은 시간 표시 텍스트
    private Timer timer;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;
    private ScoreKeeper scoreKeeper;

    [Header("ProgressBar")]
    [SerializeField] Slider progressBar;

    [Header("ChatGPT Client")]
    [SerializeField] ChatGPTClient chatGPTClient;
    [SerializeField] int questionCount = 3;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] TextMeshProUGUI hintText;

    private bool isGeneratingQuestions = false;

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClient.quizGenerateHandler += QuizGeneratedHandler;

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
        Debug.Log("Generating questions on topic: " + topicToUse);
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

    void QuizGeneratedHandler(List<QuestionSO> newQuestions)
    {
        Debug.Log($"QuizGeneratedHandler : {newQuestions?.Count ?? 0} questions received.");
        isGeneratingQuestions = false;

        if (newQuestions == null || newQuestions.Count == 0)
        {
            Debug.LogError("문제 생성에 실패 했습니다.");
            loadingText.text = "문제 생성에 실패 했습니다.\n인터넷 연결을 확인하고 다시 시도해 주세요.";
            return;
        }

        this.questions.AddRange(newQuestions);
        progressBar.maxValue += newQuestions.Count;

        GetNextQuestion();
    }

    private void InitalizeProgressBar()
    {
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
    }

    private void Update()
    {
        // 타이머 비주얼(아이콘/게이지)
        if (timer.isProblemTime)
            timerImage.sprite = problemTimerSprite;
        else
            timerImage.sprite = solutionTimerSprite;

        timerImage.fillAmount = timer.fillAmount;

        // 남은 시간 숫자(초) 표시
        if (timerText != null)
            timerText.text = $"남은시간 : {Mathf.CeilToInt(timer.remainingTime)}초";

        // 다음 문제 로딩
        if (timer.loadNextQuestion)
        {
            if (questions.Count == 0)
            {
                GenerateQuestionsIfNeeded();
            }
            else
            {
                GetNextQuestion();
            }
        }

        // 시간 끝났는데 선택 안했으면 자동 해설
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

        timer.loadNextQuestion = false;

        GameManager.Instance.ShowQuizSceen();
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
        hintText.text = "힌트: " + currentQuestion.GetHint();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.GetAnswers(i);
        }
    }

    public void OnAnswerButtonClicked(int index)
    {
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancelTimer();
        SetButtonState(false);

        // 점수: 문제당 5점으로 계산하도록 ScoreKeeper.CalculateScore()를 변경한 상태라고 가정
        scoreText.text = $"점수: {scoreKeeper.CalculateScore()}점";
    }

    private void DisplaySolution(int index)
    {
        if (index == currentQuestion.GetCorrectAnswerIndex())
        {
            questionText.text = "정답입니다!";
            if (index >= 0 && index < answerButtons.Length)
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;

            scoreKeeper.IncrementCorrectAnswer();
        }
        else
        {
            questionText.text = $"오답입니다! 정답은 {currentQuestion.GetCorrectAnswer()}입니다.";
        }

        SetButtonState(false);
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Button>().interactable = state;
        }
    }

    private void SetDefaultButtonSprites()
    {
        foreach (GameObject obj in answerButtons)
        {
            obj.GetComponent<Image>().sprite = defaultAnswerSprite;
        }
    }
}
