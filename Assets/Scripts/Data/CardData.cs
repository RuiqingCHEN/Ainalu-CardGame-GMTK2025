// 静态资产
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]

public class CardData : ScriptableObject
{
    [field: SerializeField] public int Mana { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public CardColor CardColor { get; private set; } = CardColor.Red;
    [field: SerializeReference, SR] public List<Effect> Effects { get; private set; }
}
