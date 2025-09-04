using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] ScoreKeeper scoreKeeper;

    public void ShowFinalScore()
    {
        int score = scoreKeeper.CalculateScore();

        finalScoreText.text = "당신은 프로게이머 입니다.\r\n" +
            $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        if (score >= 100)
        {
            finalScoreText.text = "당신은 프로게이머 입니다.\r\n" +
                $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        }
        else if (score >= 90)
        {
            finalScoreText.text = "당신은 썩은물 입니다.\r\n" +
                $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        }
        else if (score >= 80)
        {
            finalScoreText.text = "당신은 고인물 입니다.\r\n" +
                $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        }
        else if (score >= 70)
        {
            finalScoreText.text = "당신은 킹반인 입니다.\r\n" +
                $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        }
        else if (score >= 60)
        {
            finalScoreText.text = "당신은 일반인 입니다.\r\n" +
                $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        }
        else
        {
            finalScoreText.text = "당신은 배린이 입니다.\r\n" +
                $"당신의 점수 {scoreKeeper.CalculateScore()}%";
        }
    }
}
