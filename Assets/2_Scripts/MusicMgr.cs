using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("BGM Clips (assign in Inspector)")]
    [SerializeField] AudioClip menuBGM;   // 003
    [SerializeField] AudioClip gameBGM;   // sayonara mata itsuka
    [SerializeField] AudioClip endBGM;    // sayoranabus

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)] float volume = 0.7f;
    [SerializeField] bool loop = true;
    [SerializeField] bool useFade = true;
    [SerializeField, Min(0f)] float fadeOutDuration = 0.4f;
    [SerializeField, Min(0f)] float fadeInDuration = 0.4f;

    AudioSource source;

    void Awake()
    {
        // Singleton
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = volume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name); // ✅ 정확한 이름 (대소문자 구분)
    }

    public void PlayForScene(string sceneName)
    {
        AudioClip target = null;

        // 씬 이름을 프로젝트에 맞게 수정하세요.
        if (sceneName == "1_Menu") target = menuBGM;
        else if (sceneName == "2_Game") target = gameBGM;
        else if (sceneName == "3_End") target = endBGM;

        if (target == null || source.clip == target) return;

        if (useFade)
            StartCoroutine(FadeSwap(target, fadeOutDuration, fadeInDuration));
        else
        {
            source.Stop();
            source.clip = target;
            source.volume = volume;
            source.Play();
        }
    }

    IEnumerator FadeSwap(AudioClip next, float fadeOut, float fadeIn)
    {
        float startVol = source.volume;

        // Fade out
        float t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, fadeOut > 0f ? t / fadeOut : 1f);
            yield return null;
        }

        source.Stop();
        source.clip = next;
        source.Play();

        // Fade in
        t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, volume, fadeIn > 0f ? t / fadeIn : 1f);
            yield return null;
        }

        source.volume = volume;
    }

    // 외부에서 볼륨 바꾸고 싶을 때 호출
    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (source) source.volume = volume;
    }
}
