using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class GameOverSystem : Singleton<GameOverSystem>
{
    private bool gameEnded = false;
        
    public void CheckGameOver(int currentScore)
    {
        if (gameEnded) return; // 避免重复检查
                
        if (currentScore < 0)
        {
            TriggerGameOver(false); // 失败
        }
    }
        
    public void CheckVictory()
    {
        if (gameEnded) return; // 避免重复检查
                
        // 胜利条件：右边牌堆抽光 AND 手牌中没有右边牌堆的卡 AND 右边卡槽都空了
        if (CardSystem.Instance.IsDeck2Empty() && 
            CardSystem.Instance.HasNoDeck2CardsInHand() && 
            HasNoCardsInRightSlots())
        {
            TriggerGameOver(true); // 胜利
        }
    }
        
    private bool HasNoCardsInRightSlots()
    {
        // 检查所有右边卡槽（SlotType == 2）是否都为空
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);
                
        foreach (var slot in allSlots)
        {
            if (slot.SlotType == 2 && slot.IsOccupied)
            {
                return false; // 还有卡在右边卡槽中
            }
        }
                
        return true; // 所有右边卡槽都空了
    }
        
    private void TriggerGameOver(bool isVictory)
    {
        gameEnded = true;
        
        // 游戏结束时立即清理所有动画和订阅
        CleanupOnGameEnd();
        
        // 立即禁用所有CardView组件，防止继续交互
        DisableAllCardInteractions();
                
        if (isVictory)
        {
            // 胜利时，先保存下一关信息，然后跳转到胜利场景
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                NextLevelButton.lastLevelNextScene = levelManager.GetNextLevelName();
                Debug.Log($"保存下一关信息: {NextLevelButton.lastLevelNextScene}");
            }
            
            StartCoroutine(LoadSceneAfterFrame("WinScene"));
        }
        else
        {
            // 失败时先保存当前场景，再跳转到失败场景
            SceneMemory.SaveCurrentScene();
            StartCoroutine(LoadSceneAfterFrame("FailScene"));
        }
    }
    
    private void CleanupOnGameEnd()
    {
        // 停止所有DOTween动画
        DOTween.KillAll();
        
        // 清理ActionSystem订阅
        ActionSystem.ClearAllSubscriptions();
        
        Debug.Log("Game end cleanup completed - stopped all animations and cleared subscriptions");
    }
    
    private void DisableAllCardInteractions()
    {
        // 禁用所有CardView组件
        CardView[] allCards = FindObjectsByType<CardView>(FindObjectsSortMode.None);
        foreach (var card in allCards)
        {
            if (card != null)
            {
                card.enabled = false;
            }
        }
        
        // 也可以禁用所有Collider来阻止鼠标事件
        Collider[] allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        foreach (var collider in allColliders)
        {
            if (collider != null && collider.gameObject.GetComponent<CardView>() != null)
            {
                collider.enabled = false;
            }
        }
    }
        
    private IEnumerator LoadSceneAfterFrame(string sceneName)
    {
        yield return null; // 等待一帧
        SceneManager.LoadScene(sceneName);
    }
        
    public bool IsGameEnded()
    {
        return gameEnded;
    }
}