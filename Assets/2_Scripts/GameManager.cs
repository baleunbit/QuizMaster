using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Quiz quiz;
    [SerializeField] private EndScreen endScreen;

    void Awake()
    {
            if (Instance == null)  // null일 때 할당하도록 변경
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
    }
    void Start()
    {
        ShowQuizSceen();
    }

    private void ShowQuizSceen()
    {
        quiz.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
    }

    public void ShowEndSceen()
    {
        quiz.gameObject.SetActive(false);
        endScreen.gameObject.SetActive(true);
        endScreen.ShowFinalScore();
    }

    public void OnReplayLevel1()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
