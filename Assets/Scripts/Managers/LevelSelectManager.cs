using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    public void OnBackClick()
    {
        SceneManager.LoadScene("Menu");
    }
    public void OnLevel0Click()
    {
        SceneManager.LoadScene("Level0");
    }
    public void OnLevel1Click()
    {
        SceneManager.LoadScene("Level1");
    }
    
    public void OnLevel2Click()
    {
        SceneManager.LoadScene("Level2");
    }
    public void OnLevel3Click()
    {
        SceneManager.LoadScene("Level3");
    }
}