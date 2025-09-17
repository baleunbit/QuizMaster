using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] float problemTime = 10f;
    [SerializeField] float solutionTime = 2f; //결과
    float time = 0;

    public float remainingTime; // 현재 남은 시간
    public float totalTime;     // 총 제한시간
    [HideInInspector] public bool isProblemTime = true;
    [HideInInspector] public float fillAmount;
    [HideInInspector] public bool loadNextQuestion;

    private void Start()
    {
        time = problemTime;
        totalTime = problemTime;      // ✅ 초기 총시간
        remainingTime = problemTime;  // ✅ 초기 남은시간
        loadNextQuestion = true;
    }

    private void Update()
    {
        TimerCountDown();
        UpdateFillAmount();
    }

    private void UpdateFillAmount()
    {
        if (isProblemTime)
        {
            fillAmount = time / problemTime;
            totalTime = problemTime;          // ✅ 총시간 갱신
        }
        else
        {
            fillAmount = time / solutionTime;
            totalTime = solutionTime;         // ✅ 총시간 갱신
        }

        remainingTime = Mathf.Max(0f, time);  // ✅ 남은시간 갱신(음수 방지)
    }

    private void TimerCountDown()
    {
        time -= Time.deltaTime;
        if (time <= 0f)
        {
            if (isProblemTime)
            {
                isProblemTime = false;
                time = solutionTime;
            }
            else
            {
                isProblemTime = true;
                time = problemTime;
                loadNextQuestion = true;
            }
        }
    }

    public void CancelTimer()
    {
        time = 0f;
    }
}
