using UnityEngine;

[CreateAssetMenu(fileName = "New Question", menuName = "Quiz/Question")]

public class QuestionSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] string question = "���⿡ ������ �Է��ϼ���.";
}