using UnityEngine;
using UnityEngine.UI;

public class CardDisplayUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    public void SetupCard(Card card)
    {
        if (card == null) return;
        
        if (cardImage != null && card.Image != null)
        {
            cardImage.sprite = card.Image;
        }
    }
}