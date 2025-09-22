using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] TextMeshProUGUI timerText;
    private Timer timer;

    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;
    public ScoreKeeper scoreKeeper;

    [Header("ProgressBar")]
    [SerializeField] Slider progressBar;

    [Header("ChatGPT Client")]
    [SerializeField] ChatGPTClient chatGPTClient;
    [SerializeField] int questionCount = 3;
    [SerializeField] TextMeshProUGUI loadingText;

    [Header("Hint")]
    [SerializeField] Button hintButton;
    [SerializeField] TextMeshProUGUI hintText;
    private bool hintShown = false;

    [Header("Cheer")]
    [SerializeField] TextMeshProUGUI cheerText;

    [Header("Speed Scoring")]
    [SerializeField, Range(0f, 1f)] float fastThreshold = 0.66f;
    [SerializeField, Range(0f, 1f)] float midThreshold = 0.33f;
    [SerializeField] int fastPoints = 7;
    [SerializeField] int midPoints = 5;
    [SerializeField] int slowPoints = 3;

    [SerializeField] int maxQuestions = 5;
    private int askedCount = 0;
    private Color baseColor;
    private bool isGeneratingQuestions = false;

    // ✅ 자동 해설 중복 방지 플래그
    private bool solutionShown = false;

    private static readonly string[] FALLBACK_CORRECT = new[]
    {
        "멋지다!\n계속 가보자!",
        "완벽해!\n이대로 가자!",
        "좋았어!\n다음도 기대할게!",
    };

    private static readonly string[] FALLBACK_WRONG = new[]
    {
        "괜찮아,\n실수 할 수 있어.",
        "한 번 삐끗!\n힘 내보자!",
        "조금만 더!\n할 수 있어!",
    };

    private string PickRandom(string[] arr) => arr[rng.Next(arr.Length)];
    private System.Random rng = new System.Random();

    void Awake()
    {
        baseColor = timerImage != null ? timerImage.color : Color.white;
    }

    void Start()
    {
        timer = FindFirstObjectByType<Timer>();
        scoreKeeper = FindFirstObjectByType<ScoreKeeper>();
        chatGPTClient.quizGenerateHandler += QuizGeneratedHandler;

        if (questions.Count == 0)
            GenerateQuestionsIfNeeded();
        else
            InitalizeProgressBar();
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
        string[] topics = new[]
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

    private int GetSpeedPoints()
    {
        float remain = Mathf.Max(0.0001f, timer.remainingTime);
        float total = Mathf.Max(0.0001f, timer.totalTime);
        float ratio = remain / total;

        if (ratio >= fastThreshold) return fastPoints;
        if (ratio >= midThreshold) return midPoints;
        return slowPoints;
    }

    void QuizGeneratedHandler(List<QuestionSO> newQuestions)
    {
        isGeneratingQuestions = false;

        if (newQuestions == null || newQuestions.Count == 0)
        {
            loadingText.text = "문제 생성 실패. 인터넷을 확인하고 다시 시도해 주세요.";
            return;
        }

        this.questions.AddRange(newQuestions);
        progressBar.maxValue = maxQuestions;
        GetNextQuestion();
    }

    private void InitalizeProgressBar()
    {
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
    }

    private void Update()
    {
        // 타이머 UI
        if (timer.isProblemTime)
        {
            timerImage.sprite = problemTimerSprite;
            timerImage.fillAmount = timer.fillAmount;
            timerImage.color = timer.remainingTime <= 3f ? Color.red : baseColor;
        }
        else
        {
            timerImage.sprite = solutionTimerSprite;
            timerImage.fillAmount = timer.fillAmount;
            timerImage.color = baseColor;
        }

        if (timerText != null)
            timerText.text = $"남은시간 : {Mathf.CeilToInt(timer.remainingTime)}초";

        if (timer.loadNextQuestion)
        {
            if (askedCount >= maxQuestions)
            {
                PlayerPrefs.SetInt("FinalScore", scoreKeeper.CalculateScore());
                PlayerPrefs.Save();
                SceneManager.LoadScene("3_End");
                return;
            }

            if (questions.Count == 0)
            {
                if (!isGeneratingQuestions) GenerateQuestionsIfNeeded();
            }
            else
            {
                GetNextQuestion();
            }
        }

        // ⛔ 한 번만 자동 해설 실행
        if (!timer.isProblemTime && !chooseAnswer && !solutionShown)
        {
            DisplaySolution(-1);
        }
    }

    private void GetNextQuestion()
    {
        if (questions.Count == 0)
        {
            if (!isGeneratingQuestions) GenerateQuestionsIfNeeded();
            return;
        }

        // 새 문제 시작 → 해설 초기화
        solutionShown = false;
        hintShown = false;
        if (hintText != null) hintText.gameObject.SetActive(false);
        if (hintButton != null) hintButton.gameObject.SetActive(true);

        timer.loadNextQuestion = false;

        GameManager.Instance.ShowQuizSceen();
        chooseAnswer = false;
        SetButtonState(true);
        SetDefaultButtonSprites();

        GetRandomQuestion();
        OnDisplayQuestion();

        askedCount++;
        scoreKeeper.IncrementQuestionSeen();
        progressBar.value++;
    }

    public void OnHintButtonClicked()
    {
        if (hintShown) return;

        hintShown = true;
        if (hintButton != null) hintButton.gameObject.SetActive(false);
        if (hintText != null) hintText.gameObject.SetActive(true);
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

        if (hintText != null)
        {
            hintText.text = "힌트: " + currentQuestion.GetHint();
            hintText.gameObject.SetActive(false);
        }

        for (int i = 0; i < answerButtons.Length; i++)
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.GetAnswers(i);
    }

    public void OnAnswerButtonClicked(int index)
    {
        if (solutionShown) return; // 중복 클릭 방지
        chooseAnswer = true;
        DisplaySolution(index);
        timer.CancelTimer();
        SetButtonState(false);
        scoreText.text = $"점수: {scoreKeeper.CalculateScore()}점";
    }

    private void DisplaySolution(int index)
    {
        if (solutionShown) return; // 자동 해설 중복 방지
        solutionShown = true;

        bool isCorrect = (index == currentQuestion.GetCorrectAnswerIndex());

        if (isCorrect)
        {
            questionText.text = "정답입니다!";
            if (index >= 0 && index < answerButtons.Length)
                answerButtons[index].GetComponent<Image>().sprite = correctAnswerSprite;

            scoreKeeper.IncrementCorrectAnswer();
            int award = timer.isProblemTime ? GetSpeedPoints() : slowPoints;
            scoreKeeper.AddPoints(award);
        }
        else
        {
            questionText.text = $"오답입니다! 정답은 {currentQuestion.GetCorrectAnswer()}입니다.";
        }

        if (cheerText != null)
        {
            cheerText.gameObject.SetActive(true);
            cheerText.text = isCorrect ? PickRandom(FALLBACK_CORRECT) : PickRandom(FALLBACK_WRONG);
        }

        scoreText.text = $"점수: {scoreKeeper.CalculateScore()}점";
        SetButtonState(false);
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject obj in answerButtons)
            obj.GetComponent<Button>().interactable = state;
    }

    private void SetDefaultButtonSprites()
    {
        foreach (GameObject obj in answerButtons)
            obj.GetComponent<Image>().sprite = defaultAnswerSprite;
    }
}