using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text title; // 现在用来显示剩余回合数
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;

    public Card Card { get; private set; }
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private CardSlot currentSlot; // 记录当前所在的卡槽

    public void Setup(Card card)
    {
        Card = card;
        // 在手牌中时不显示回合数
        title.text = "2"; 
        scoreText.text = card.Score.ToString();
        imageSR.sprite = card.Image;
    }

    // 设置所在的卡槽 - 这是关键方法
    public void SetSlot(CardSlot slot)
    {
        currentSlot = slot;
        // 直接从卡槽获取并显示回合数，不依赖任何事件
        if (slot != null)
        {
            UpdateTurnsDisplay(slot.TurnsRemaining);
        }
    }

    // 更新回合数显示 - 可以传入特定的回合数
    public void UpdateTurnsDisplay(int? specificTurns = null)
    {
        int turns = specificTurns ?? (currentSlot?.TurnsRemaining ?? 0);
        
        title.text = turns.ToString();
        
    }

    // 公开方法，让CardSlot可以直接调用更新显示
    public void OnTurnsRemainingChanged()
    {
        if (currentSlot != null)
        {
            UpdateTurnsDisplay();
        }
    }

    void OnMouseEnter()
    {
        if (!CanInteract() ||!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (!CanInteract() || !Interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (!CanInteract() || !Interactions.Instance.PlayerCanInteract()) return;
        // 如果已经在卡槽中，不能再拖动
        if (currentSlot != null) return;
        
        // 播放选中卡牌音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardSelect();
        }
        
        Interactions.Instance.PlayerIsDragging = true;
        wrapper.SetActive(true);
        CardViewHoverSystem.Instance.Hide();
        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    void OnMouseDrag()
    {
        if (!CanInteract() || !Interactions.Instance.PlayerCanInteract()) return;
        if (currentSlot != null) return;
        
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    void OnMouseUp()
    {
        if (!CanInteract() || !Interactions.Instance.PlayerCanInteract()) return;
        if (currentSlot != null) return;

        bool cardPlaced = false;

        if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 20f, dropLayer))
        {
            CardSlot targetSlot = hit.collider.GetComponent<CardSlot>();
            if (targetSlot != null && !targetSlot.IsOccupied)
            {
                // 检查卡牌类型是否匹配卡槽类型
                if (Card.DeckType == targetSlot.SlotType)
                {
                    // 如果是右边卡牌，需要额外检查数量限制
                    if (Card.DeckType == 2)
                    {
                        if (TurnCardTracker.Instance != null && !TurnCardTracker.Instance.CanPlayRightCard())
                        {
                            // 右边卡牌已经出够2张，不能再出更多
                            if (AudioManager.Instance != null)
                            {
                                AudioManager.Instance.PlayCardError();
                            }
                            
                            // 显示警告
                            if (WarningUIManager.Instance != null)
                            {
                                WarningUIManager.Instance.ShowWarning();
                            }
                            
                            // 卡牌回到原位
                            transform.position = dragStartPosition;
                            transform.rotation = dragStartRotation;
                            Interactions.Instance.PlayerIsDragging = false;
                            return;
                        }
                    }
                    
                    // 匹配成功且数量允许，执行放置
                    PlayCardGA playCardGA = new(Card, targetSlot);
                    ActionSystem.Instance.Perform(playCardGA);
                    cardPlaced = true;
                    
                    // 播放放置成功音效
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayCardPlace();
                    }
                }
                else
                {
                    // 类型不匹配，播放错误音效
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayCardError();
                    }
                }
            }
        }

        // 如果没有成功放置，卡牌回到原位
        if (!cardPlaced)
        {
            transform.position = dragStartPosition;
            transform.rotation = dragStartRotation;
        }
        
        Interactions.Instance.PlayerIsDragging = false;
    }
    
    private bool CanInteract()
    {
        if (PauseController.IsGamePaused)
        {
            return false;
        }
        if (GameOverSystem.Instance != null && GameOverSystem.Instance.IsGameEnded())
        {
            return false;
        }
        if (this == null || gameObject == null)
        {
            return false;
        }
        
        return true;
    }
}