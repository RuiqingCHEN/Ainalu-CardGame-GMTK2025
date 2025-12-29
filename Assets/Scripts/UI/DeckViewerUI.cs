using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deckViewerPanel; // 牌库查看面板
    [SerializeField] private Transform cardGridParent; // GridLayoutGroup的父对象
    [SerializeField] private GameObject cardDisplayPrefab; // 显示卡牌的预制体
    [SerializeField] private Button closeButton; // 关闭按钮
    
    [Header("Deck View Buttons")]
    [SerializeField] private Button viewLeftDeckButton; // 查看左抽牌堆按钮
    [SerializeField] private Button viewRightDeckButton; // 查看右抽牌堆按钮
    [SerializeField] private Button viewDiscardPileButton; // 查看左弃牌堆按钮
    
    private List<GameObject> currentDisplayCards = new List<GameObject>();
    
    private void Start()
    {
        // 绑定按钮事件
        if (viewLeftDeckButton != null)
            viewLeftDeckButton.onClick.AddListener(() => ShowDeck(DeckType.LeftDraw));
            
        if (viewRightDeckButton != null)
            viewRightDeckButton.onClick.AddListener(() => ShowDeck(DeckType.RightDraw));
            
        if (viewDiscardPileButton != null)
            viewDiscardPileButton.onClick.AddListener(() => ShowDeck(DeckType.DiscardPile));
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDeckViewer);
            
        // 初始隐藏面板
        if (deckViewerPanel != null)
            deckViewerPanel.SetActive(false);
    }
    
    public enum DeckType
    {
        LeftDraw,    // 左抽牌堆
        RightDraw,   // 右抽牌堆
        DiscardPile  // 左弃牌堆
    }
    
    public void ShowDeck(DeckType deckType)
    {
        if (CardSystem.Instance == null) return;
        
        List<Card> targetDeck = GetDeckByType(deckType);
        DisplayDeck(targetDeck);
    }
    
    private List<Card> GetDeckByType(DeckType deckType)
    {
        switch (deckType)
        {
            case DeckType.LeftDraw:
                return CardSystem.Instance.GetLeftDrawPile();
            case DeckType.RightDraw:
                return CardSystem.Instance.GetRightDrawPile();
            case DeckType.DiscardPile:
                return CardSystem.Instance.GetDiscardPile();
            default:
                return new List<Card>();
        }
    }
    
    private void DisplayDeck(List<Card> deck)
    {
        // 清理之前的显示
        ClearCurrentDisplay();
        
        // 使用暂停机制禁用游戏交互
        PauseController.SetPause(true);
        
        // 显示每张牌
        foreach (Card card in deck)
        {
            CreateCardDisplay(card);
        }
        
        // 显示面板
        if (deckViewerPanel != null)
            deckViewerPanel.SetActive(true);
    }
    
    private void CreateCardDisplay(Card card)
    {
        if (cardDisplayPrefab == null || cardGridParent == null) return;
        
        GameObject cardDisplay = Instantiate(cardDisplayPrefab, cardGridParent);
        currentDisplayCards.Add(cardDisplay);
        
        // 设置卡牌显示信息
        CardDisplayUI cardUI = cardDisplay.GetComponent<CardDisplayUI>();
        if (cardUI != null)
        {
            cardUI.SetupCard(card);
        }
    }
    
    private void ClearCurrentDisplay()
    {
        foreach (GameObject cardDisplay in currentDisplayCards)
        {
            if (cardDisplay != null)
                Destroy(cardDisplay);
        }
        currentDisplayCards.Clear();
    }
    
    public void CloseDeckViewer()
    {
        ClearCurrentDisplay();
        
        // 恢复游戏（取消暂停）
        PauseController.SetPause(false);
        
        if (deckViewerPanel != null)
            deckViewerPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // 清理事件绑定
        if (viewLeftDeckButton != null)
            viewLeftDeckButton.onClick.RemoveAllListeners();
        if (viewRightDeckButton != null)
            viewRightDeckButton.onClick.RemoveAllListeners();
        if (viewDiscardPileButton != null)
            viewDiscardPileButton.onClick.RemoveAllListeners();
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
            
        // 确保销毁时恢复游戏状态
        PauseController.SetPause(false);
    }
}