using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;


public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    private readonly List<CardView> cards = new();
    
    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        yield return UpdateCardPositions(0.15f);
    }
    
    public CardView RemoveCard(Card card)
    {
        CardView cardView = GetCardView(card);
        if (cardView == null) return null;
        cards.Remove(cardView);
        StartCoroutine(UpdateCardPositions(0.15f));
        return cardView;
    }
    
    private CardView GetCardView(Card card)
    {
        return cards.Where(cardView => cardView?.Card == card).FirstOrDefault();
    }
    
    private IEnumerator UpdateCardPositions(float duration)
    {
        if(cards.Count == 0) yield break;
        float cardSpacing = 1f / 10f; //最多10张牌
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2 ;
        Spline spline = splineContainer.Spline;
        for (int i = 0; i < cards.Count; i++)
        {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration);
            cards[i].transform.DORotate(rotation.eulerAngles, duration);
        }
        yield return new WaitForSeconds(duration);
    }

    // 修改后的HandView.cs - 需要添加的方法
    // 在HandView.cs中添加这个方法：

    public IEnumerator UpdateCardPositionsAfterRemoval()
    {
        yield return UpdateCardPositions(0.15f);
    }
    
    // 新增：立即停止所有手牌动画的方法（只在游戏胜利时调用）
    public void StopAllCardAnimations()
    {
        foreach (var card in cards)
        {
            if (card != null && card.transform != null)
            {
                card.transform.DOKill();
            }
        }
    }
}