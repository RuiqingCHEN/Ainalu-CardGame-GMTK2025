using UnityEngine;
using DG.Tweening;

public class EndTurnButtonUI : MonoBehaviour
{
    public void OnClick()
    {
        // 检查当前场景是否为Level3
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Level3跳过出牌检查
        if (currentScene != "Level3")
        {
            if (TurnCardTracker.Instance != null && !TurnCardTracker.Instance.CanEndTurn())
            {
                if (WarningUIManager.Instance != null)
                {
                    WarningUIManager.Instance.ShowWarning();
                }
                
                transform.DOShakePosition(0.3f, new Vector3(5, 0, 0), 10, 90, false, true);
                return; 
            }
        }
        
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}