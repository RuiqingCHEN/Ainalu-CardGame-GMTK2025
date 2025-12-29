using UnityEngine;

public class PlayCardGA : GameAction
{
    public Card Card { get; set; }
    public CardSlot TargetSlot { get; } // 新添加

    public PlayCardGA(Card card , CardSlot targetSlot)
    {
        Card = card;
        TargetSlot = targetSlot;
    }
}
