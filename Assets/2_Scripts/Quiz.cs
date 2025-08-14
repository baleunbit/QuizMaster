using TMPro;
using UnityEngine;

public class Quiz : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI questionText;
    [SerializeField] QuestionSO question;
    [SerializeField] TextMeshProUGUI[] answerTextArr = new TextMeshProUGUI[4];

    void Start()
    {
        questionText.text = question.GetQuestion();

        Debug.Log("answerTextArr.Length: " + answerTextArr.Length);

        for (int i = 0; i < answerTextArr.Length; i++)
        {
            answerTextArr[i].text = question.GetAnswers(i);
        }
    }
}
