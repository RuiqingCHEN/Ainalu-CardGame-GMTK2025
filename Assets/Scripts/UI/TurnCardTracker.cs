using UnityEngine;

public class TurnCardTracker : Singleton<TurnCardTracker>
{
    private int rightCardsPlayedThisTurn = 0; // 专门追踪右牌堆卡牌
    private int totalCardsPlayedThisTurn = 0; // 追踪所有卡牌
    private const int EXACT_RIGHT_CARDS_PER_TURN = 2; // 右牌堆卡牌精确要求：必须且仅2张
    private const int MINIMUM_TOTAL_CARDS_WHEN_DECK2_EMPTY = 2; // 右牌堆空时的最低要求
    
    private void OnEnable()
    {
        // 订阅打牌事件
        ActionSystem.SubscribeReaction<PlayCardGA>(OnCardPlayed, ReactionTiming.POST);
    }
    
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(OnCardPlayed, ReactionTiming.POST);
    }
    
    private void OnCardPlayed(PlayCardGA playCardGA)
    {
        // 检查卡牌是否确实被成功放置了
        if (playCardGA.TargetSlot != null && playCardGA.TargetSlot.IsOccupied)
        {
            // 验证卡牌类型与卡槽类型匹配
            if (playCardGA.Card.DeckType == playCardGA.TargetSlot.SlotType)
            {
                totalCardsPlayedThisTurn++;
                
                // 如果是右牌堆的卡牌，额外计数
                if (playCardGA.Card.DeckType == 2)
                {
                    rightCardsPlayedThisTurn++;
                }
                
                // 检查是否满足出牌要求，如果满足且警告正在显示则关闭警告
                if (CanEndTurn())
                {
                    if (WarningUIManager.Instance != null)
                    {
                        WarningUIManager.Instance.TryAutoCloseWarning();
                    }
                }
            }
        }
    }
    
    // 新增：检查是否可以放置右边卡牌（防止超过2张）
    public bool CanPlayRightCard()
    {
        // 检查当前场景名称，如果是Level3则没有限制
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Level3")
        {
            return true; // Level3没有出牌限制
        }
        
        // 检查右牌堆是否为空
        if (CardSystem.Instance != null && CardSystem.Instance.IsDeck2Empty())
        {
            return true; // 右牌堆空了，可以出任意卡牌
        }
        else
        {
            // 右牌堆还有牌，检查是否已经出了2张右边卡牌
            return rightCardsPlayedThisTurn < EXACT_RIGHT_CARDS_PER_TURN;
        }
    }
    
    // 供外部查询是否可以结束回合
    public bool CanEndTurn()
    {
        // 检查当前场景名称，如果是Level3则不需要限制
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Level3")
        {
            return true; // Level3没有出牌限制
        }
        
        bool canEnd = false;
        
        // 检查右牌堆是否为空
        if (CardSystem.Instance != null && CardSystem.Instance.IsDeck2Empty())
        {
            // 右牌堆空了，可以出任意数量的牌（包括0张）
            canEnd = true;
        }
        else
        {
            // 右牌堆还有牌，必须精确出2张右牌堆的牌
            canEnd = rightCardsPlayedThisTurn == EXACT_RIGHT_CARDS_PER_TURN;
        }
        
        return canEnd;
    }
    
    // 新增：获取当前状态的警告信息
    public string GetWarningMessage()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Level3")
        {
            return ""; // Level3没有限制
        }
        
        if (CardSystem.Instance != null && CardSystem.Instance.IsDeck2Empty())
        {
            return ""; // 右牌堆空了，没有限制
        }
        
        if (rightCardsPlayedThisTurn < EXACT_RIGHT_CARDS_PER_TURN)
        {
            int remaining = EXACT_RIGHT_CARDS_PER_TURN - rightCardsPlayedThisTurn;
            return $"每回合必须出2张右边卡牌！\n还需要出 {remaining} 张右边卡牌";
        }
        else if (rightCardsPlayedThisTurn >= EXACT_RIGHT_CARDS_PER_TURN)
        {
            return "本回合已经出够2张右边卡牌！\n无法再出更多右边卡牌";
        }
        
        return "";
    }
    
    // 回合真正结束时重置计数器
    public void ResetForNewTurn()
    {
        rightCardsPlayedThisTurn = 0;
        totalCardsPlayedThisTurn = 0;
    }
    
    // 供外部查询当前回合已出的右牌堆卡牌数
    public int GetRightCardsPlayedThisTurn()
    {
        return rightCardsPlayedThisTurn;
    }
    
    // 供外部查询当前回合已出的总卡牌数
    public int GetTotalCardsPlayedThisTurn()
    {
        return totalCardsPlayedThisTurn;
    }
    
    // 供外部查询还需要出多少张右牌堆卡牌
    public int GetRemainingRightCardsNeeded()
    {
        // 检查当前场景名称，如果是Level3则返回0
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Level3")
        {
            return 0; // Level3没有出牌限制
        }
        
        if (CardSystem.Instance != null && CardSystem.Instance.IsDeck2Empty())
        {
            // 右牌堆空了，可以出任意数量的牌
            return 0;
        }
        else
        {
            // 右牌堆还有牌，返回还需要的右牌堆卡牌数
            return Mathf.Max(0, EXACT_RIGHT_CARDS_PER_TURN - rightCardsPlayedThisTurn);
        }
    }
}