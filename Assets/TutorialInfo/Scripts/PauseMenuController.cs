using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Pause Settings")]
    public bool pauseAudio = true;

    private EventSystem sceneEventSystem;
    private bool eventSystemWasActive;
    private bool createdEventSystem;
    private bool isPaused;

    void Awake()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        RestoreRuntimeState();
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;
        EnsureEventSystem();
        Time.timeScale = 0f;

        if (pauseAudio)
        {
            AudioListener.pause = true;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PauseMenuController nema nastaveny pausePanel.");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseAudio)
        {
            AudioListener.pause = false;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        RestoreEventSystemState();
    }

    public void LoadMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseAudio)
        {
            AudioListener.pause = false;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RestoreEventSystemState();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDisable()
    {
        RestoreRuntimeState();
        RestoreEventSystemState();
    }

    void OnDestroy()
    {
        RestoreRuntimeState();
        RestoreEventSystemState();
    }

    void RestoreRuntimeState()
    {
        Time.timeScale = 1f;

        if (pauseAudio)
        {
            AudioListener.pause = false;
        }
    }

    void EnsureEventSystem()
    {
        sceneEventSystem = FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);

        if (sceneEventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("PauseEventSystem");
            sceneEventSystem = eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
            createdEventSystem = true;
            eventSystemWasActive = false;
            return;
        }

        createdEventSystem = false;
        eventSystemWasActive = sceneEventSystem.gameObject.activeSelf;

        if (!sceneEventSystem.gameObject.activeSelf)
        {
            sceneEventSystem.gameObject.SetActive(true);
        }
    }

    void RestoreEventSystemState()
    {
        if (sceneEventSystem == null)
        {
            return;
        }

        if (createdEventSystem)
        {
            Destroy(sceneEventSystem.gameObject);
            sceneEventSystem = null;
            createdEventSystem = false;
            return;
        }

        if (!eventSystemWasActive)
        {
            sceneEventSystem.gameObject.SetActive(false);
        }
    }
}
