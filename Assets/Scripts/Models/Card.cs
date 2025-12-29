// 运行时的卡牌
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CardColor
{
    Red,
    Green, 
    Black,
    White
}

public class Card
{
    public string Title => data.name;
    public Sprite Image => data.Image;
    public List<Effect> Effects => data.Effects;
    public int Score { get; private set; }
    public int DeckType { get; set; } // 1=牌堆1，2=牌堆2
    public CardColor Color { get; set; } // 卡牌颜色

    private readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
        Score = cardData.Mana;
        Color = cardData.CardColor;
    }
}