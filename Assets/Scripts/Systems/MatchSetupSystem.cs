using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private List<CardData> deck1Data; // 牌堆1的卡牌数据
    [SerializeField] private List<CardData> deck2Data; // 牌堆2的卡牌数据
    
    private void Start()
    {
        // 设置两个牌堆
        CardSystem.Instance.Setup(deck1Data, deck2Data);
    }
}