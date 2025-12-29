using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    private bool isWarningUIActive = false;
    
    public void SetWarningUIActive(bool active)
    {
        isWarningUIActive = active;
    }
    
    public bool PlayerCanInteract()
    {
        if (isWarningUIActive) return false;
        if (!ActionSystem.Instance.IsPerforming) return true;
        else return false;
    }
    
    public bool PlayerCanHover()
    {
        if (isWarningUIActive) return false;
        if (PlayerIsDragging) return false;
        return true;
    }
}