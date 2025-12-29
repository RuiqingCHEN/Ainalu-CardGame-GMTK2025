using UnityEngine;
using UnityEngine.UI;

public class NextLevelButton : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    
    public static string lastLevelNextScene = "";
    
    private void Start()
    {
        if (nextButton == null)
        {
            nextButton = GetComponent<Button>();
        }
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(GoToNextLevel);
        }
    }
    
    public void GoToNextLevel()
    {
        if (!string.IsNullOrEmpty(lastLevelNextScene))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(lastLevelNextScene);
        }
    }
    
    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
        }
    }
}