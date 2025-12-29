using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FailSceneManager : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    
    private void Start()
    {
        Time.timeScale = 1f;
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
    }
    
    public void RestartGame()
    {
        SceneMemory.LoadLastScene();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}