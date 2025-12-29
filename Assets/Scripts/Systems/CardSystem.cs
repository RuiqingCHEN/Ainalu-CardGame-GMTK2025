using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPile1Point;
    [SerializeField] private Transform drawPile2Point;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private bool autoPlaceRightCards = false;

    private readonly List<Card> drawPile1 = new();
    private readonly List<Card> drawPile2 = new();
    private readonly List<Card> hand = new();
    private readonly List<Card> discardPile = new();
    
    private int currentTurn = 0; // 追踪当前回合数

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        DOTween.Kill(this);
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public void SetAutoPlaceRightCards(bool autoPlace)
    {
        autoPlaceRightCards = autoPlace;
    }

    public void Setup(List<CardData> deck1Data, List<CardData> deck2Data)
    {
        foreach (var cardData in deck1Data)
        {
            Card card = new(cardData);
            card.DeckType = 1;
            drawPile1.Add(card);
        }

        foreach (var cardData in deck2Data)
        {
            Card card = new(cardData);
            card.DeckType = 2;
            drawPile2.Add(card);
        }

        ScoreSystem.Instance.SetTotalRightDeckCards(deck2Data.Count);
        currentTurn = 0; // 初始化回合数
        StartCoroutine(DrawInitialCards());
    }

    private IEnumerator DrawInitialCards()
    {
        currentTurn = 1; // 设置为第一回合
        
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (currentScene == "Level3")
        {
            // Level3: 第一回合只抽4张左牌堆的牌
            for (int i = 0; i < 4; i++)
            {
                yield return DrawCard(1);
            }
            
            // Level3: 在右边卡槽随机放置2张右牌堆的牌
            yield return AutoPlaceRightCardsInLevel3(2);
        }
        else
        {
            // 其他关卡: 抽8张牌（左4张，右4张）
            for (int i = 0; i < 4; i++)
            {
                yield return DrawCard(1);
            }
            for (int i = 0; i < 4; i++)
            {
                yield return DrawCard(2);
            }
        }
        
        GameOverSystem.Instance.CheckVictory();
        
        // 注意：这里不调用UpdateScoreOnNextTurn，因为这不是回合结束
        // 只在初始化时显示初始总分，不进行累加计算
        Debug.Log("初始抽牌完成，不更新总分");
    }

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        CardSlot targetSlot = playCardGA.TargetSlot;
        if (targetSlot == null || targetSlot.IsOccupied)
        {
            yield break;
        }

        // 验证卡牌类型与卡槽类型是否匹配
        if (!IsValidSlotForCard(playCardGA.Card, targetSlot))
        {
            Debug.Log("Card and slot type mismatch!");
            // 卡牌类型不匹配时，不执行任何操作，直接退出
            yield break;
        }

        Debug.Log($"PlayCardPerformer: 放置卡牌 {playCardGA.Card.Title} 到 {targetSlot.name}");

        // 只有在验证通过后才从手牌中移除卡牌并放置
        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        yield return PlaceCardInSlot(cardView, playCardGA.Card, targetSlot);

        // 玩家放置卡牌时才重新计算分数，自动放置不会走这里
        ScoreSystem.Instance.RecalculateScore();

        foreach (var effect in playCardGA.Card.Effects)
        {
            PerformEffectGA performEffectGA = new(effect);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        currentTurn++; // 增加回合数
        
        if (TurnCardTracker.Instance != null)
        {
            TurnCardTracker.Instance.ResetForNewTurn();
        }

        ScoreSystem.Instance.UpdateScoreOnNextTurn();
        StartCoroutine(ProcessEndOfTurn());
    }

    private IEnumerator ProcessEndOfTurn()
    {
        UpdateAllSlotsTurnsRemaining();
        yield return null;
        yield return WaitForAllRecycleAnimations();
        yield return DrawCardsNextTurn();
    }

    private IEnumerator WaitForAllRecycleAnimations()
    {
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);
        List<CardSlot> slotsToRecycle = new();

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == 1 && slot.IsOccupied && slot.TurnsRemaining <= 0)
            {
                slotsToRecycle.Add(slot);
            }
        }
        foreach (var slot in slotsToRecycle)
        {
            yield return RecycleCardFromSlot(slot);
        }
    }

    private void UpdateAllSlotsTurnsRemaining()
    {
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);

        foreach (var slot in allSlots)
        {
            if (slot.IsOccupied && slot.SlotType == 1)
            {
                slot.DecreaseTurnsRemaining();
            }
        }
    }

    private IEnumerator RecycleExpiredLeftSlotCards()
    {
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);
        List<CardSlot> leftSlotsToRecycle = new();

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == 1 && slot.IsOccupied && slot.TurnsRemaining <= 0)
            {
                leftSlotsToRecycle.Add(slot);
            }
        }

        foreach (var slot in leftSlotsToRecycle)
        {
            yield return RecycleCardFromSlot(slot);
        }
    }

    private IEnumerator RecycleCardFromSlot(CardSlot slot)
    {
        if (!slot.IsOccupied) yield break;

        CardView cardView = slot.GetCardView();
        Card card = cardView?.Card;

        if (cardView == null || card == null) yield break;

        if (card.DeckType != 1)
        {
            yield break;
        }

        slot.ClearSlot();
        
        // 播放左侧卡牌回收音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardRecycle();
        }
        
        yield return AnimateCardToDrawPile(cardView, discardPilePoint);
        AddToDiscardPile(card);

        if (cardView != null && cardView.gameObject != null)
        {
            Destroy(cardView.gameObject);
        }
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.RecalculateScore();
        }

        if (GameOverSystem.Instance != null)
        {
            GameOverSystem.Instance.CheckVictory();
        }
    }

    public IEnumerator RecycleCardToLeftDeckWithAnimation(CardView cardView, Card card)
    {
        if (cardView == null || cardView.gameObject == null || card == null) yield break;

        Debug.Log($"回收卡牌 {card.Title} 到弃牌堆");
        
        // 播放左侧卡牌回收音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardRecycle();
        }
        
        yield return AnimateCardToDrawPile(cardView, discardPilePoint);
        AddToDiscardPile(card);

        if (cardView != null && cardView.gameObject != null)
        {
            Destroy(cardView.gameObject);
        }
    }

    private IEnumerator AnimateCardToDrawPile(CardView cardView, Transform target)
    {
        if (cardView == null || cardView.gameObject == null || target == null || cardView.transform == null)
        {
            yield break;
        }

        var collider = cardView.GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        cardView.transform.SetParent(null);
        cardView.transform.DOKill();

        Tween rotateTween = null;

        try
        {
            var moveTween = cardView.transform.DOMove(target.position, 0.5f)
                .SetTarget(cardView.transform);
            var scaleTween = cardView.transform.DOScale(Vector3.one, 0.5f)
                .SetTarget(cardView.transform);
            rotateTween = cardView.transform.DORotate(target.rotation.eulerAngles, 0.5f)
                .SetTarget(cardView.transform);

            rotateTween.OnComplete(() =>
            {
                if (cardView == null || cardView.gameObject == null)
                {
                    DOTween.Kill(cardView?.transform);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Animation error in AnimateCardToDrawPile: {e.Message}");
            if (cardView?.transform != null)
            {
                cardView.transform.DOKill();
            }
            yield break;
        }

        if (rotateTween != null)
        {
            yield return rotateTween.WaitForCompletion();
        }
    }

    private IEnumerator DrawCardsNextTurn()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (currentScene == "Level3")
        {
            // Level3: 第二回合开始，每回合只从左牌堆抽2张牌
            if (currentTurn >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    yield return DrawCard(1);
                }
            }
            
            // Level3: 每回合在右边卡槽随机放置2张右牌堆的牌
            yield return AutoPlaceRightCardsInLevel3(2);
        }
        else
        {
            // 其他关卡: 检查右牌堆是否为空
            if (IsDeck2Empty())
            {
                // 右牌堆已空，不再抽任何牌
                Debug.Log("右牌堆已空，本回合不抽牌");
            }
            else
            {
                // 右牌堆还有牌，正常抽牌逻辑
                for (int i = 0; i < 2; i++)
                {
                    yield return DrawCard(1);
                }
                for (int i = 0; i < 2; i++)
                {
                    yield return DrawCard(2);
                }
            }
        }
        
        GameOverSystem.Instance.CheckVictory();
        
        // 注意：这里也不调用UpdateScoreOnNextTurn
        // UpdateScoreOnNextTurn只在玩家点击回合结束按钮时调用
        Debug.Log("抽牌完成，不更新总分");
    }

    // 新增：Level3专用的自动放置右牌堆卡牌方法
    private IEnumerator AutoPlaceRightCardsInLevel3(int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            if (drawPile2.Count > 0)
            {
                Card card = drawPile2.Draw();
                yield return AutoPlaceRightCard(card, drawPile2Point);
            }
        }
    }

    public bool IsDeck2Empty()
    {
        return drawPile2.Count == 0;
    }

    public int GetRightDeckCount()
    {
        return drawPile2.Count;
    }

    public bool HasNoDeck2CardsInHand()
    {
        foreach (var card in hand)
        {
            if (card.DeckType == 2)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator DrawCard(int deckNumber)
    {
        List<Card> targetDeck = deckNumber == 1 ? drawPile1 : drawPile2;
        Transform targetPoint = deckNumber == 1 ? drawPile1Point : drawPile2Point;

        if (deckNumber == 1 && targetDeck.Count == 0)
        {
            RecycleDiscardPileToDrawPile1();
        }

        if (targetDeck.Count == 0)
        {
            Debug.Log($"Deck {deckNumber} is empty!");
            yield break;
        }

        Card card = targetDeck.Draw();

        // 播放抽牌音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardDraw();
        }

        if (deckNumber == 2 && autoPlaceRightCards)
        {
            Debug.Log($"自动放置右边卡牌: {card.Title}");
            yield return AutoPlaceRightCard(card, targetPoint);
        }
        else
        {
            hand.Add(card);

            if (CardViewCreator.Instance != null && targetPoint != null)
            {
                CardView cardView = CardViewCreator.Instance.CreateCardView(card, targetPoint.position, targetPoint.rotation);
                if (handView != null)
                {
                    yield return handView.AddCard(cardView);
                }
            }
        }
    }

    private bool IsValidSlotForCard(Card card, CardSlot slot)
    {
        return card.DeckType == slot.SlotType;
    }

    private IEnumerator PlaceCardInSlot(CardView cardView, Card card, CardSlot slot)
    {
        if (cardView == null || cardView.gameObject == null || slot == null || cardView.transform == null)
        {
            yield break;
        }

        // 根据卡牌类型设置不同的回合数
        int turnsToSet = 2; // 默认2回合
        if (card.DeckType == 2) // 右侧卡牌
        {
            turnsToSet = 2; // 右侧卡牌也是2回合
        }
        
        slot.SetOccupied(cardView, turnsToSet);
        cardView.SetSlot(slot);
        cardView.transform.DOKill();

        Tween rotateTween = null;

        try
        {
            var moveTween = cardView.transform.DOMove(slot.transform.position, 0.3f)
                .SetTarget(cardView.transform);
            var scaleTween = cardView.transform.DOScale(Vector3.one * 0.6f, 0.3f)
                .SetTarget(cardView.transform);
            rotateTween = cardView.transform.DORotate(Vector3.zero, 0.3f)
                .SetTarget(cardView.transform);

            rotateTween.OnComplete(() =>
            {
                if (cardView == null || cardView.gameObject == null)
                {
                    DOTween.Kill(cardView?.transform);
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Animation error in PlaceCardInSlot: {e.Message}");
            if (cardView?.transform != null)
            {
                cardView.transform.DOKill();
            }
            yield break;
        }

        if (rotateTween != null)
        {
            yield return rotateTween.WaitForCompletion();
        }

        if (cardView != null && cardView.gameObject != null)
        {
            var collider = cardView.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            if (cardView.transform != null && slot != null)
            {
                cardView.transform.SetParent(slot.transform);
            }
        }
        
        Debug.Log($"卡牌放置完成: {card.Title}, DeckType={card.DeckType}, SlotType={slot.SlotType}, TurnsRemaining={slot.TurnsRemaining}");
    }

    private void RecycleDiscardPileToDrawPile1()
    {
        if (discardPile.Count == 0) return;

        Debug.Log($"弃牌堆回收: {discardPile.Count}张牌回到左边牌堆");
        drawPile1.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile1();
    }

    private void ShuffleDrawPile1()
    {
        for (int i = 0; i < drawPile1.Count; i++)
        {
            Card temp = drawPile1[i];
            int randomIndex = Random.Range(i, drawPile1.Count);
            drawPile1[i] = drawPile1[randomIndex];
            drawPile1[randomIndex] = temp;
        }
    }

    public void AddToDiscardPile(Card card)
    {
        discardPile.Add(card);
    }

    public List<Card> GetLeftDrawPile()
    {
        return new List<Card>(drawPile1);
    }

    public List<Card> GetRightDrawPile()
    {
        return new List<Card>(drawPile2);
    }

    public List<Card> GetDiscardPile()
    {
        return new List<Card>(discardPile);
    }

    private IEnumerator AutoPlaceRightCard(Card card, Transform sourcePoint)
    {
        CardView cardView = null;
        if (CardViewCreator.Instance != null && sourcePoint != null)
        {
            cardView = CardViewCreator.Instance.CreateCardView(card, sourcePoint.position, sourcePoint.rotation);
        }

        CardSlot availableSlot = FindAvailableRightSlot();

        if (availableSlot == null)
        {
            Debug.Log("没有可用的右边卡槽，卡牌无法自动放置");
            if (cardView != null && cardView.gameObject != null)
            {
                Destroy(cardView.gameObject);
            }
            yield break;
        }

        // 直接放置卡牌，不通过ActionSystem
        yield return PlaceCardInSlot(cardView, card, availableSlot);

        // 只更新右边分数显示，不触发完整的分数重新计算
        ScoreSystem.Instance.UpdateRightTotalScoreDisplayImmediate();

        // 执行卡牌效果（如果有的话）
        foreach (var effect in card.Effects)
        {
            PerformEffectGA performEffectGA = new(effect);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }

        Debug.Log($"右边卡牌 {card.Title} 已自动放置到卡槽");
    }

    private CardSlot FindAvailableRightSlot()
    {
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);
        List<CardSlot> availableRightSlots = new();

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == 2 && !slot.IsOccupied)
            {
                availableRightSlots.Add(slot);
            }
        }
        if (availableRightSlots.Count > 0)
        {
            int randomIndex = Random.Range(0, availableRightSlots.Count);
            return availableRightSlots[randomIndex];
        }

        return null;
    }
    
    // 新增：获取当前回合数
    public int GetCurrentTurn()
    {
        return currentTurn;
    }
}