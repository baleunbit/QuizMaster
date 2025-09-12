using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChatGPTClient;

public class ChatGPTClient : MonoBehaviour
{
    public delegate void QuizGeneratedHandler(List<QuestionSO> questions);
    public event QuizGeneratedHandler quizGenerateHandler;

    public void GenerateQuizQuestions(int questionCount, string topicToUse)
    {
        StartCoroutine(GenerateWithDelay());
    }

    private IEnumerator GenerateWithDelay()
    { 
        yield return new WaitForSeconds(3f);

        List<QuestionSO> questions = new List<QuestionSO>();
        QuestionSO so1 = CreateQuetion("GPT 생성 질문 1",
            new string[] { "답변1(정답)", "답변2", "답변3", "답변4" },
            0);
        questions.Add(so1);

        QuestionSO so2 = CreateQuetion("GPT 생성 질문 2",
            new string[] { "답변1", "답변2(정답)", "답변3", "답변4" },
            1);
        questions.Add(so2);

        QuestionSO so3 = CreateQuetion("GPT 생성 질문 3",
            new string[] { "답변1", "답변2", "답변3(정답)", "답변4" },
            2);
        questions.Add(so3);

        quizGenerateHandler?.Invoke(questions);   
        Debug.Log("Finished GenerateWithDelay............");
    }

    QuestionSO CreateQuetion(string q, string[] a, int correctIndex)
    {
        QuestionSO so = ScriptableObject.CreateInstance<QuestionSO>();
        so.SetData(q, a, correctIndex);

        return so;
    }
}