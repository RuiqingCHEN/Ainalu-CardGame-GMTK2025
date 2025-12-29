using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    
    public static bool IsGamePaused { get; private set; } = false;
    
    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    public void TogglePause()
    {
        if (IsGamePaused)
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
        IsGamePaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    public void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void OnResumeClick()
    {
        ResumeGame();
    }

    public void OnMainMenuClick()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        SceneManager.LoadScene("Menu");
    }
    
    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    static PauseController()
    {
        SceneManager.sceneLoaded += (scene, mode) => {
            IsGamePaused = false;
            Time.timeScale = 1f;
        };
    }
}