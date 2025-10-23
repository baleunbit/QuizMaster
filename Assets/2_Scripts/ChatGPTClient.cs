using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatGPTRequest
{
    public string model = "gpt-4.1-nano";
    public Message[] messages;
    public float temperature = 1.1f;
    public int max_completion_tokens = 4000;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class ChatGPTResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class QuizData
{
    public QuizQuestion[] questions;
}

[Serializable]
public class QuizQuestion
{
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
    public string hint;
}

public class ChatGPTClient : MonoBehaviour
{
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    private string apiKey;

    public delegate void QuizGenerateHandler(List<QuestionSO> questions);
    public event QuizGenerateHandler quizGenerateHandler;

    private void Awake()
    {
        apiKey = LoadApiKey();
        Debug.Log($"Loaded API Key (masked): {Mask(apiKey)}");

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("OpenAI API Key가 비어 있습니다. PlayerPrefs/Resources/config/환경변수를 확인하세요.");
        }
    }

    private string LoadApiKey()
    {
        // 1) PlayerPrefs
        var key = PlayerPrefs.GetString("OpenAI_API_Key", string.Empty);
        if (!string.IsNullOrEmpty(key)) return key.Trim();

        // 2) Resources/config (예: Assets/Resources/config.txt)
        try
        {
            TextAsset configFile = Resources.Load<TextAsset>("config");
            if (configFile != null)
            {
                string[] lines = configFile.text.Split('\n');
                foreach (string line in lines)
                {
                    if (line.StartsWith("OPENAI_API_KEY="))
                    {
                        return line.Substring("OPENAI_API_KEY=".Length).Trim();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Resources 설정 파일 로드 실패: {e.Message}");
        }

        // 3) 환경변수
        key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(key)) return key.Trim();

        return string.Empty;
    }

    private string Mask(string key)
    {
        if (string.IsNullOrEmpty(key)) return "(empty)";
        if (key.Length <= 8) return $"{key[0]}***{key[^1]}";
        return $"{key.Substring(0, 4)}***{key.Substring(key.Length - 4)}";
    }


    public void GenerateQuizQuestions(int count = 3, string topic = "일반상식")
    {
        StartCoroutine(RequestQuizQuestions(count, topic));
    }

    private IEnumerator RequestQuizQuestions(int count, string topic)
    {
        string prompt = $"다음 조건에 맞는 객관식 퀴즈 문제를 {count}개 생성해주세요:\n" +
                        $"주제: {topic}\n" +
                        "조건:\n" +
                        "- 문제와 보기는 20자 이내로 짧게 작성\n" +
                        "- 힌트는 15자 이내로 간결하게 작성\n" +
                        "- 문제/보기는 실제 게임 맥락에서 자연스럽게\n" +
                        "- 선택지는 재치있거나 함정 요소 포함\n" +
                        "- 정답은 0~3 인덱스\n" +
                        "- 응답은 반드시 다음 JSON 형식(설명 금지):\n" +
                        "{\n" +
                        "  \"questions\": [\n" +
                        "    {\n" +
                        "      \"question\": \"문제 내용\",\n" +
                        "      \"answers\": [\"선택지1\", \"선택지2\", \"선택지3\", \"선택지4\"],\n" +
                        "      \"correctAnswerIndex\": 0,\n" +
                        "      \"hint\": \"간단한 힌트\"\n" +
                        "    }\n" +
                        "  ]\n" +
                        "}";

        Debug.Log("Prompt to ChatGPT:\n" + prompt);

        ChatGPTRequest request = new ChatGPTRequest
        {
            messages = new Message[]
            {
                new Message { role = "user", content = prompt }
            }
        };

        string jsonRequest = JsonUtility.ToJson(request);
        Debug.Log("Request JSON:\n" + jsonRequest);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Raw response from ChatGPT:\n" + webRequest.downloadHandler.text);
                    ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(webRequest.downloadHandler.text);

                    if (response == null || response.choices == null || response.choices.Length == 0)
                    {
                        Debug.LogError("Invalid response structure from ChatGPT API");
                        yield break;
                    }

                    if (response.choices[0].message == null)
                    {
                        Debug.LogError("Message content is null in ChatGPT response");
                        yield break;
                    }

                    string jsonContent = response.choices[0].message.content;

                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        Debug.LogError("Content is empty. Finish reason: " + response.choices[0].message);
                        Debug.LogError("Consider increasing max_completion_tokens");
                        yield break;
                    }

                    Debug.Log("Response from ChatGPT:\n" + jsonContent);
                    // JSON 문자열에서 불필요한 부분 제거
                    jsonContent = jsonContent.Trim();
                    if (jsonContent.StartsWith("```json"))
                    {
                        jsonContent = jsonContent.Substring(7);
                    }
                    if (jsonContent.EndsWith("```"))
                    {
                        jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
                    }
                    jsonContent = jsonContent.Trim();

                    QuizData quizData = JsonUtility.FromJson<QuizData>(jsonContent);
                    List<QuestionSO> generatedQuestions = CreateQuestionSOs(quizData.questions);

                    quizGenerateHandler?.Invoke(generatedQuestions);
                }
                catch (Exception e)
                {
                    Debug.LogError($"응답 파싱 오류: {e.Message}");
                    Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");
                }
            }
            else
            {
                Debug.LogError($"ChatGPT API 요청 실패: {webRequest.error}");
                Debug.LogError($"응답 코드: {webRequest.responseCode}");
                Debug.LogError($"응답 내용: {webRequest.downloadHandler.text}");
            }
        }
    }
    public void GenerateCheerMessage(bool isCorrect, string topic, int score, Action<string> onDone)
    {
        StartCoroutine(RequestCheerMessage(isCorrect, topic, score, onDone));
    }

    private IEnumerator RequestCheerMessage(bool isCorrect, string topic, int score, Action<string> onDone)
    {
        // 한 줄만, 40자 이내, 한국어, 이모지 1~2개 허용
        string mood = isCorrect ? "정답" : "오답";
        string prompt =
            $"퀴즈 {mood} 결과에 맞는 한국어 한 줄 응원 메시지를 1개만 만들어줘.\n" +
            $"주제: {topic}\n" +
            $"현재 점수: {score}점\n" +
            "- 40자 이내, 간결하고 따뜻하게, 재미있게\n" +
            "- 이모지는 0~2개만\n" +
            "- 따옴표 없이 문장만 반환\n" +
            "- 코드블록, 설명, 접두사/접미사 금지";

        ChatGPTRequest req = new ChatGPTRequest
        {
            messages = new Message[]
            {
            new Message { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(req);

        using (UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return webRequest.SendWebRequest();

            string result = null;

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var resp = JsonUtility.FromJson<ChatGPTResponse>(webRequest.downloadHandler.text);
                    result = resp?.choices != null && resp.choices.Length > 0
                        ? resp.choices[0].message?.content
                        : null;

                    if (!string.IsNullOrEmpty(result))
                    {
                        // 혹시 모를 코드블록 제거
                        result = result.Trim();
                        if (result.StartsWith("```")) result = result.Trim('`').Trim();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Cheer parse fail: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Cheer request fail: {webRequest.error}");
            }

            if (string.IsNullOrEmpty(result))
            {
                // 실패 시 null 전달 (퀴즈에서 폴백 유지)
                result = null;
            }

            onDone?.Invoke(result);
        }
    }

    private List<QuestionSO> CreateQuestionSOs(QuizQuestion[] quizQuestions)
    {
        List<QuestionSO> questionSOs = new List<QuestionSO>();

        foreach (QuizQuestion quizQ in quizQuestions)
        {
            QuestionSO questionSO = ScriptableObject.CreateInstance<QuestionSO>();

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var questionField = typeof(QuestionSO).GetField("question", flags);
            var answersField = typeof(QuestionSO).GetField("answers", flags);
            var correctAnswerIndexField = typeof(QuestionSO).GetField("correctAnswerIndex", flags);
            var hintField = typeof(QuestionSO).GetField("hint", flags); // ✅ 추가

            questionField?.SetValue(questionSO, quizQ.question);
            answersField?.SetValue(questionSO, quizQ.answers);
            correctAnswerIndexField?.SetValue(questionSO, quizQ.correctAnswerIndex);

            // ✅ hint 주입 (필드가 없으면 로그)
            if (hintField != null) hintField.SetValue(questionSO, quizQ.hint);
            else Debug.LogWarning("QuestionSO에 'hint' 필드가 없습니다. GetHint()가 어느 필드를 읽는지 확인하세요.");

            questionSOs.Add(questionSO);
        }

        return questionSOs;
    }

    public void SetApiKey(string key)
    {
        apiKey = key;
        PlayerPrefs.SetString("OpenAI_API_Key", key);
        PlayerPrefs.Save();
    }
}