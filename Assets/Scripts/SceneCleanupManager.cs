using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneCleanupManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        DOTween.KillAll();
        ActionSystem.ClearAllSubscriptions();
    }
}