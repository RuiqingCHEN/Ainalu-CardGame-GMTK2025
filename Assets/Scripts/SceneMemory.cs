using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneMemory
{
    private static string lastSceneName;
    private static int lastSceneBuildIndex;
    
    public static void SaveCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        lastSceneName = currentScene.name;
        lastSceneBuildIndex = currentScene.buildIndex;
    }
    
    public static void LoadLastScene()
    {
        if (!string.IsNullOrEmpty(lastSceneName))
        {
            SceneManager.LoadScene(lastSceneName);
        }
        else if (lastSceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(lastSceneBuildIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    
    public static string GetLastSceneName()
    {
        return lastSceneName;
    }
}