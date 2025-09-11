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
        yield return new WaitForSeconds(2f);
        quizGenerateHandler?.Invoke(new List<QuestionSO>());
        Debug.Log("Finished generating questions after delay.");
    }
}
