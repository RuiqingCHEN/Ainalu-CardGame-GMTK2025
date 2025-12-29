using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InstructionUI : MonoBehaviour
{  
    [SerializeField] private Button loadButton;
    public void LoadScene()
    {
        SceneManager.LoadScene("Level0");
    }
}
