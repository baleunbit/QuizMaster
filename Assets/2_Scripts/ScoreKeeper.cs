using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    int correctAnswer = 0;
    int questionSeen = 0;
    int totalPoints = 0;               // ✅ 누적 포인트

    public int GetCorrectAnswer()
    {
        return correctAnswer;
    }

    public void IncrementCorrectAnswer()
    {
        correctAnswer++;
    }

    public int GetQuestionSeen()
    {
        return questionSeen;
    }

    public void IncrementQuestionSeen()
    {
        questionSeen++;
    }

    /// <summary>점수 추가 (Quiz에서 속도별 점수 계산 후 호출)</summary>
    public void AddPoints(int points)
    {
        totalPoints += points;
    }

    /// <summary>현재 점수 반환</summary>
    public int CalculateScore()
    {
        return totalPoints;
    }

    /// <summary>게임 재시작 시 점수 리셋</summary>
    public void ResetScore()
    {
        correctAnswer = 0;
        questionSeen = 0;
        totalPoints = 0;
    }
}
