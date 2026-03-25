using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string gameplaySceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
