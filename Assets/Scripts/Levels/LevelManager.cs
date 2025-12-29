using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string currentLevelName;
    [SerializeField] private string nextLevelName;    

    public string winSceneName = "WinScene"; 
    public string finalSceneName = "FinalScene"; 
    
    private void Start()
    {
        currentLevelName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(nextLevelName))
        {
            AutoSetNextLevel();
        }
    }
    
    private void AutoSetNextLevel()
    {
        switch (currentLevelName)
        {
            case "Level0":
                nextLevelName = "Level1";
                break;
            case "Level1":
                nextLevelName = "Level2";
                break;
            case "Level2":
                nextLevelName = "Level3";
                break;
            case "Level3":
                nextLevelName = finalSceneName;
                break;
            default:break;
        }
    }
    
    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }
    
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(currentLevelName);
    }
    
    public void LoadWinScene()
    {
        SceneManager.LoadScene(winSceneName);
    }
    
    public string GetCurrentLevelName()
    {
        return currentLevelName;
    }
    
    public string GetNextLevelName()
    {
        return nextLevelName;
    }
    
    public void SetNextLevel(string levelName)
    {
        nextLevelName = levelName;
    }
}