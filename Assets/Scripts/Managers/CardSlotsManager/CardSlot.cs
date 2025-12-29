using UnityEngine;
using DG.Tweening;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private int slotType = 1; // 1=卡槽1类型，2=卡槽2类型
    public int SlotType => slotType;
    public bool IsOccupied { get; private set; } = false;
    public CardView OccupiedCard { get; private set; } = null;
    public int TurnsRemaining { get; private set; } = 0;
    
    private void Start()
    {
        // 只有右边卡槽需要订阅事件（因为它们需要独立处理）
        if (slotType == 2)
        {
            ActionSystem.SubscribeReaction<EnemyTurnGA>(OnTurnEnd, ReactionTiming.POST);
        }
        
        // 确保有Collider用于检测拖拽
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.5f, 2f, 0.1f); // 调整大小适合卡牌
        }
    }
    
    private void OnDestroy()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 如果是右边卡槽，取消订阅
        if (slotType == 2)
        {
            try
            {
                ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnTurnEnd, ReactionTiming.POST);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"CardSlot销毁时取消订阅失败: {e.Message}");
            }
        }
    }
    
    public void SetOccupied(CardView cardView, int turns)
    {
        IsOccupied = true;
        OccupiedCard = cardView;
        TurnsRemaining = turns;
        
        Debug.Log($"CardSlot设置占用: 卡牌={cardView?.Card?.Title}, 回合数={turns}, SlotType={slotType}");
    }
    
    // 获取CardView的方法（为回收系统提供支持）
    public CardView GetCardView()
    {
        return OccupiedCard;
    }
    
    // 清空卡槽的方法（为回收系统提供支持）
    public void ClearSlot()
    {
        IsOccupied = false;
        OccupiedCard = null;
        TurnsRemaining = 0;
    }
    
    // 公开方法：由 CardSystem 调用来减少回合数
    public void DecreaseTurnsRemaining()
    {
        if (IsOccupied && OccupiedCard != null)
        {
            TurnsRemaining--;
            
            // 通知CardView更新显示
            if (OccupiedCard != null)
            {
                OccupiedCard.OnTurnsRemainingChanged();
            }
        }
    }
    
    // 只处理右边卡槽的回合结束逻辑
    private void OnTurnEnd(EnemyTurnGA enemyTurnGA)
    {
        // 只处理右边卡槽
        if (slotType == 2 && IsOccupied && OccupiedCard != null)
        {
            TurnsRemaining--;
            
            Debug.Log($"右边卡槽回合结束: 卡牌={OccupiedCard.Card?.Title}, 剩余回合={TurnsRemaining}");
            
            // 通知CardView更新显示
            OccupiedCard.OnTurnsRemainingChanged();
            
            if (TurnsRemaining <= 0)
            {
                Debug.Log($"右边卡牌即将销毁: {OccupiedCard.Card?.Title}");
                
                // 播放右侧卡牌销毁音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCardDestroy();
                }
                
                // 右边卡槽：直接销毁
                Destroy(OccupiedCard.gameObject);
                OccupiedCard = null;
                IsOccupied = false;
                
                // 记录右边卡牌完成流程
                ScoreSystem.Instance.OnRightCardCompleted();
                
                // 立即检查胜利条件，如果胜利就停止所有动画
                CheckVictoryAndStopAnimations();
                
                // 卡牌消失后重新计算分数 - 但确保不更新总分UI
                ScoreSystem.Instance.RecalculateScore();
                
                // 卡牌消失后检查胜利条件
                GameOverSystem.Instance.CheckVictory();
            }
        }
    }
    
    // 检查胜利条件并停止动画
    private void CheckVictoryAndStopAnimations()
    {
        // 检查是否胜利（右边牌堆抽光 AND 手牌中没有右边牌堆的卡 AND 右边卡槽都空了）
        if (CardSystem.Instance.IsDeck2Empty() && 
            CardSystem.Instance.HasNoDeck2CardsInHand() && 
            HasNoCardsInRightSlots())
        {
            DOTween.KillAll();
            
            // 停止所有CardSystem的协程
            if (CardSystem.Instance != null)
            {
                CardSystem.Instance.StopAllCoroutines();
            }
            
            // 停止HandView的动画
            HandView handView = FindFirstObjectByType<HandView>();
            if (handView != null)
            {
                handView.StopAllCardAnimations();
            }
            
            // 停止所有CardView的动画
            CardView[] allCards = FindObjectsByType<CardView>(FindObjectsSortMode.None);
            foreach (var card in allCards)
            {
                if (card != null && card.transform != null)
                {
                    card.transform.DOKill();
                }
            }
        }
    }
    
    // 检查所有右边卡槽是否都为空
    private bool HasNoCardsInRightSlots()
    {
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
}