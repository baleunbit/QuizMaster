using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMgr : MonoBehaviour
{
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("2_Game");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}