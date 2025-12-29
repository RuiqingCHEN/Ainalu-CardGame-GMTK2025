using UnityEngine;

public class VideoDestroyer : MonoBehaviour
{
    public float videoLength = 2.5f;
    public GameObject uiToShow;
    
    void Start()
    {
        Invoke("DestroyVideo", videoLength);
    }
    
    void DestroyVideo()
    {
        if (uiToShow != null)
            uiToShow.SetActive(true);
        
        Destroy(gameObject);
    }
}