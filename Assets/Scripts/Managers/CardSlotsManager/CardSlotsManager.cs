using UnityEngine;

public class CardSlotsManager : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private Sprite leftSideSprite;   // 左边用的贴图
    [SerializeField] private Sprite rightSideSprite;  // 右边用的贴图
    [SerializeField] private int slotType = 1;
    
    [Header("Grid Layout")]
    [SerializeField] private int rows = 2;
    [SerializeField] private int columns = 2;
    [SerializeField] private int maxSlots = 4;
    [SerializeField] private float spacingX = 2.5f;
    [SerializeField] private float spacingY = 3.25f;
    [SerializeField] private bool isLeftSide = true;
    
    void Start()
    {
        GenerateCardSlots();
    }
    
    void GenerateCardSlots()
    {
        Vector2Int[] positions = GeneratePositionOrder();
        Sprite spriteToUse = isLeftSide ? leftSideSprite : rightSideSprite;
        
        for (int i = 0; i < Mathf.Min(positions.Length, maxSlots); i++)
        {
            Vector2Int gridPos = positions[i];
            Vector3 worldPos = new Vector3(gridPos.x * spacingX, gridPos.y * spacingY, 0);
            
            GameObject slot = Instantiate(cardSlotPrefab, transform);
            slot.transform.localPosition = worldPos;
            slot.name = $"CardSlot_{gridPos.y}_{gridPos.x}";
            
            // 更换SpriteRenderer的贴图
            SpriteRenderer spriteRenderer = slot.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spriteToUse;
            }
            
            // 设置卡槽类型
            CardSlot cardSlotComponent = slot.GetComponent<CardSlot>();
            if (cardSlotComponent != null)
            {
                var field = typeof(CardSlot).GetField("slotType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(cardSlotComponent, slotType);
            }
        }
    }
    
    Vector2Int[] GeneratePositionOrder()
    {
        // ... 保持原来的代码不变
        var positions = new System.Collections.Generic.List<Vector2Int>();
        
        if (isLeftSide)
        {
            // 左边：逆时针生成（从右上开始往左）
            for (int col = columns - 1; col >= 0; col--)
            {
                positions.Add(new Vector2Int(col, rows - 1));
            }
            
            for (int row = rows - 2; row >= 0; row--)
            {
                positions.Add(new Vector2Int(0, row));
            }
            
            for (int col = 1; col < columns; col++)
            {
                positions.Add(new Vector2Int(col, 0));
            }
            
            for (int row = 1; row < rows - 1; row++)
            {
                positions.Add(new Vector2Int(columns - 1, row));
            }
        }
        else
        {
            // 右边：逆时针生成（从左下开始）
            for (int col = 0; col < columns; col++)
            {
                positions.Add(new Vector2Int(col, 0));
            }
            
            for (int row = 1; row < rows; row++)
            {
                positions.Add(new Vector2Int(columns - 1, row));
            }
            
            for (int col = columns - 2; col >= 0; col--)
            {
                positions.Add(new Vector2Int(col, rows - 1));
            }
            
            for (int row = rows - 2; row > 0; row--)
            {
                positions.Add(new Vector2Int(0, row));
            }
        }
        
        return positions.ToArray();
    }
}