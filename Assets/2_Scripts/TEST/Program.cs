using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Program : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Hello, World!");
        Publisher publisher = new Publisher();
        publisher.msg += ResultProcess;
        publisher.msg += OtherProcess;

        publisher.SendMessage("안녕하세요!");

        Debug.Log("통신 성공");
    }

    void ResultProcess(string msg)
    {
        Debug.Log($"메시지 수신: {msg}");
    }

    void OtherProcess (string text)
    {
        Debug.Log($"다른 처리: {text}");
    }
}

public class Publisher
{
    public delegate void OnMessage(string msg);
    public event OnMessage msg;

    public void SendMessage(string text)
    {
        Debug.Log($"ChatGPT API와 통신을 시도 합니다... (최대 1분 이상 소요) {text}");

        msg?.Invoke(text);
    }
}