using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScene : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI rankText;

    void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        scoreText.text = $"최종 점수: {finalScore}점";

        // ✅ 점수 구간에 따른 등급 판정
        if (finalScore >= 55)
            rankText.text = "당신은 고수 입니다.";
        else if (finalScore >= 40)
            rankText.text = "당신은 중수 입니다.";
        else if (finalScore >= 30)
            rankText.text = "당신은 하수 입니다.";
        else
            rankText.text = "점수가 너무 낮아요... 다시 도전!";
    }

    public void OnRetryClicked()
    {
        SceneManager.LoadScene("2_Game");
    }

    public void OnQuitClicked()
    {
        SceneManager.LoadScene("1_Menu");
    }
}
