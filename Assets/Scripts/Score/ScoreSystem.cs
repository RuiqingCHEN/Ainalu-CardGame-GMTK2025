using UnityEngine;
using TMPro;
using System;

public class ScoreSystem : Singleton<ScoreSystem>
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text leftTotalScoreText;
    [SerializeField] private TMP_Text rightTotalScoreText;
    [SerializeField] private TMP_Text rightCardsProgressText;
    
    [SerializeField] private int baseScore = 1;
    
    private int currentScore; // 累积总分
    private int currentLeftScore = 0; // 当前回合左侧分数
    private int currentRightScore = 0; // 当前回合右侧分数
    private int totalRightDeckCards = 0;
    private int completedRightCards = 0;

    private void Start()
    {
        currentScore = baseScore;
        // 在游戏开始时显示初始总分
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
        // 更新其他显示
        UpdateLeftTotalScoreDisplay(0);
        UpdateRightTotalScoreDisplay(0);
        UpdateRightCardsProgressDisplay();
    }

    public void SetTotalRightDeckCards(int totalCount)
    {
        totalRightDeckCards = totalCount;
        UpdateRightCardsProgressDisplay();
    }

    public void OnRightCardCompleted()
    {
        completedRightCards++;
        UpdateRightCardsProgressDisplay();
    }

    public void RecalculateScore()
    {
        int bonusScore = 0;
        int colorPenalty = 0;
        int adjacentSameBonus = 0;

        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);

        // 计算左右两侧的不同花色奖励
        bonusScore += CalculateColorVarietyBonus(allSlots, 1); // 左侧
        bonusScore += CalculateColorVarietyBonus(allSlots, 2); // 右侧
        
        colorPenalty = CalculateColorPenalty(allSlots);
        adjacentSameBonus = CalculateAdjacentSameBonus(allSlots);

        var leftSlotScoreResult = CalculateLeftSlotScoreWithMultiplier(allSlots);
        int leftSlotScore = leftSlotScoreResult.totalScore;
        int colorMatchMultiplier = leftSlotScoreResult.multiplier;

        var rightSlotScoreResult = CalculateRightSlotScoreWithMultiplier(allSlots);
        int rightSlotScore = rightSlotScoreResult.totalScore;

        int leftAdjacentBonus = CalculateAdjacentSameBonusInSlotType(allSlots, 1);
        int rightAdjacentPenalty = CalculateAdjacentSameBonusInSlotType(allSlots, 2);

        // 计算当前回合的左右分数（不包含累积分数）
        currentLeftScore = leftSlotScore + bonusScore + leftAdjacentBonus - colorPenalty;
        currentRightScore = rightSlotScore + rightAdjacentPenalty;

        // 更新显示（但不更新总分，总分在回合结束时更新）
        UpdateLeftTotalScoreDisplay(currentLeftScore);
        UpdateRightTotalScoreDisplay(currentRightScore);
        UpdateRightCardsProgressDisplay();
        
        // 注意：这里不再检测游戏结束，只在回合结束时检测
    }

    // 修改：立即更新右边总分显示（完全隔离，绝不更新总分UI）
    public void UpdateRightTotalScoreDisplayImmediate()
    {
        Debug.Log("UpdateRightTotalScoreDisplayImmediate 被调用");
        
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);
        var rightSlotScoreResult = CalculateRightSlotScoreWithMultiplier(allSlots);
        int rightSlotScore = rightSlotScoreResult.totalScore;
        int rightAdjacentPenalty = CalculateAdjacentSameBonusInSlotType(allSlots, 2);
        int rightTotalScore = rightSlotScore + rightAdjacentPenalty;
        
        Debug.Log($"计算的右侧分数: {rightTotalScore}");
        
        // 只更新右侧分数显示，绝不调用任何可能更新总分的方法
        if (rightTotalScoreText != null)
        {
            rightTotalScoreText.text = rightTotalScore.ToString();
            Debug.Log($"右侧分数UI更新为: {rightTotalScore}");
        }
        
        // 同时更新currentRightScore用于后续计算，但不更新总分UI
        currentRightScore = rightTotalScore;
        Debug.Log($"currentRightScore 设置为: {currentRightScore}");
        
        // 明确说明：这里绝不调用 UpdateScoreDisplay()
    }

    private int CalculateAdjacentSameBonus(CardSlot[] allSlots)
    {
        int bonus = 0;
        bonus += CalculateAdjacentSameBonusInSlotType(allSlots, 1);
        bonus += CalculateAdjacentSameBonusInSlotType(allSlots, 2);
        return bonus;
    }

    private int CalculateAdjacentSameBonusInSlotType(CardSlot[] allSlots, int slotType)
    {
        var slotGrid = new System.Collections.Generic.Dictionary<Vector2Int, CardSlot>();
        var colorGroups = new System.Collections.Generic.Dictionary<CardColor, System.Collections.Generic.List<Vector2Int>>();

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == slotType && slot.IsOccupied && slot.OccupiedCard != null)
            {
                Vector2Int gridPos = ParseSlotPosition(slot.name);
                if (gridPos != Vector2Int.one * -1)
                {
                    slotGrid[gridPos] = slot;
                    CardColor cardColor = slot.OccupiedCard.Card.Color;

                    if (!colorGroups.ContainsKey(cardColor))
                        colorGroups[cardColor] = new System.Collections.Generic.List<Vector2Int>();
                    colorGroups[cardColor].Add(gridPos);
                }
            }
        }

        int totalBonus = 0;

        foreach (var colorGroup in colorGroups)
        {
            CardColor cardColor = colorGroup.Key;
            var positions = colorGroup.Value;

            if (positions.Count < 2) continue;

            // 计算相邻连接的组数
            var connectedGroups = FindConnectedGroups(positions);
            
            foreach (var group in connectedGroups)
            {
                int groupSize = group.Count;
                int bonusPoints = 0;
                
                if (slotType == 1)
                {
                    // 左侧牌：2个+2分，3个及以上+3分
                    if (groupSize == 2)
                    {
                        bonusPoints = 2;
                    }
                    else if (groupSize >= 3)
                    {
                        bonusPoints = 3;
                    }
                    totalBonus += bonusPoints;
                }
                else
                {
                    // 右侧牌：2个+1分，3个+2分，4个及以上+3分
                    if (groupSize == 2)
                    {
                        bonusPoints = 1;
                    }
                    else if (groupSize == 3)
                    {
                        bonusPoints = 2;
                    }
                    else if (groupSize >= 4)
                    {
                        bonusPoints = 3;
                    }
                    totalBonus -= bonusPoints; // 右侧是负分
                }
            }
        }

        return totalBonus;
    }

    // 新增方法：找到所有相邻连接的组
    private System.Collections.Generic.List<System.Collections.Generic.List<Vector2Int>> FindConnectedGroups(System.Collections.Generic.List<Vector2Int> positions)
    {
        var groups = new System.Collections.Generic.List<System.Collections.Generic.List<Vector2Int>>();
        var visited = new System.Collections.Generic.HashSet<Vector2Int>();
        var positionSet = new System.Collections.Generic.HashSet<Vector2Int>(positions);

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var pos in positions)
        {
            if (visited.Contains(pos)) continue;

            var currentGroup = new System.Collections.Generic.List<Vector2Int>();
            var stack = new System.Collections.Generic.Stack<Vector2Int>();
            stack.Push(pos);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited.Contains(current)) continue;

                visited.Add(current);
                currentGroup.Add(current);

                foreach (var dir in directions)
                {
                    var neighbor = current + dir;
                    if (positionSet.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }

            if (currentGroup.Count > 0)
            {
                groups.Add(currentGroup);
            }
        }

        return groups;
    }

    private int CountAdjacentConnections(System.Collections.Generic.List<Vector2Int> positions)
    {
        int connections = 0;
        var positionSet = new System.Collections.Generic.HashSet<Vector2Int>(positions);

        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var pos in positions)
        {
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = pos + dir;
                if (positionSet.Contains(neighborPos))
                {
                    connections++;
                }
            }
        }

        return connections / 2;
    }

    private int CalculateColorPenalty(CardSlot[] allSlots)
    {
        int penalty = 0;
        penalty += CheckAdjacentColorConflictInSlotType(allSlots, 1);
        penalty += CheckAdjacentColorConflictInSlotType(allSlots, 2);
        return penalty;
    }

    private int CheckAdjacentColorConflictInSlotType(CardSlot[] allSlots, int slotType)
    {
        int penalty = 0;
        var slotGrid = new System.Collections.Generic.Dictionary<Vector2Int, CardSlot>();

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == slotType && slot.IsOccupied && slot.OccupiedCard != null)
            {
                Vector2Int gridPos = ParseSlotPosition(slot.name);
                if (gridPos != Vector2Int.one * -1)
                {
                    slotGrid[gridPos] = slot;
                }
            }
        }

        foreach (var kvp in slotGrid)
        {
            Vector2Int pos = kvp.Key;
            CardSlot slot = kvp.Value;

            Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var dir in directions)
            {
                Vector2Int neighborPos = pos + dir;
                if (slotGrid.ContainsKey(neighborPos))
                {
                    CardSlot neighborSlot = slotGrid[neighborPos];
                    if (HasColorConflict(slot.OccupiedCard.Card, neighborSlot.OccupiedCard.Card))
                    {
                        penalty++;
                    }
                }
            }
        }

        return penalty / 2;
    }

    private Vector2Int ParseSlotPosition(string slotName)
    {
        string[] parts = slotName.Split('_');
        if (parts.Length == 3 && parts[0] == "CardSlot")
        {
            if (int.TryParse(parts[1], out int row) && int.TryParse(parts[2], out int col))
            {
                return new Vector2Int(col, row);
            }
        }
        return Vector2Int.one * -1;
    }

    private bool HasColorConflict(Card card1, Card card2)
    {
        return (card1.Color == CardColor.Red && card2.Color == CardColor.Green) ||
               (card1.Color == CardColor.Green && card2.Color == CardColor.Red) ||
               (card1.Color == CardColor.Black && card2.Color == CardColor.White) ||
               (card1.Color == CardColor.White && card2.Color == CardColor.Black);
    }

    // 修改后的花色多样性奖励计算
    private int CalculateColorVarietyBonus(CardSlot[] allSlots, int slotType)
    {
        var colors = new System.Collections.Generic.HashSet<CardColor>();

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == slotType && slot.IsOccupied && slot.OccupiedCard != null)
            {
                colors.Add(slot.OccupiedCard.Card.Color);
            }
        }

        // 新的奖励规则：3个不同花色+2分，4个不同花色+3分
        if (colors.Count == 3)
        {
            return 2;
        }
        else if (colors.Count >= 4)
        {
            return 3;
        }

        return 0;
    }

    // 保留原来的方法名但使用新的逻辑
    private int CalculateLeftColorVarietyBonus(CardSlot[] allSlots)
    {
        return CalculateColorVarietyBonus(allSlots, 1);
    }

    private (int totalScore, int multiplier) CalculateLeftSlotScoreWithMultiplier(CardSlot[] allSlots)
    {
        var leftSlots = new System.Collections.Generic.Dictionary<Vector2Int, CardSlot>();
        var rightSlots = new System.Collections.Generic.Dictionary<Vector2Int, CardSlot>();

        foreach (var slot in allSlots)
        {
            if (slot.IsOccupied && slot.OccupiedCard != null)
            {
                Vector2Int gridPos = ParseSlotPosition(slot.name);
                if (gridPos != Vector2Int.one * -1)
                {
                    if (slot.SlotType == 1)
                    {
                        leftSlots[gridPos] = slot;
                    }
                    else if (slot.SlotType == 2)
                    {
                        rightSlots[gridPos] = slot;
                    }
                }
            }
        }

        var correspondenceMap = GetCorrespondenceMap(leftSlots.Count, rightSlots.Count);

        int matchCount = 0;
        int matchedLeftScore = 0;
        int unmatchedLeftScore = 0;

        foreach (var leftSlot in leftSlots)
        {
            Vector2Int leftPos = leftSlot.Key;
            CardSlot leftCardSlot = leftSlot.Value;
            int leftScore = leftCardSlot.OccupiedCard.Card.Score;

            if (correspondenceMap.ContainsKey(leftPos))
            {
                Vector2Int rightPos = correspondenceMap[leftPos];

                if (rightSlots.ContainsKey(rightPos))
                {
                    CardSlot rightCardSlot = rightSlots[rightPos];
                    CardColor leftColor = leftCardSlot.OccupiedCard.Card.Color;
                    CardColor rightColor = rightCardSlot.OccupiedCard.Card.Color;


                    if (leftColor == rightColor)
                    {
                        matchCount++;
                        matchedLeftScore += leftScore;
                    }
                    else
                    {
                        unmatchedLeftScore += leftScore;
                    }
                }
                else
                {
                    unmatchedLeftScore += leftScore;
                }
            }
            else
            {
                unmatchedLeftScore += leftScore;
            }
        }

        int multiplier = matchCount;
        int totalScore;

        if (matchCount >= 1)
        {
            totalScore = (matchedLeftScore * multiplier) + unmatchedLeftScore;
        }
        else
        {
            totalScore = matchedLeftScore + unmatchedLeftScore;
        }

        return (totalScore, multiplier);
    }

    private (int totalScore, int multiplier) CalculateRightSlotScoreWithMultiplier(CardSlot[] allSlots)
    {
        var leftSlots = new System.Collections.Generic.Dictionary<Vector2Int, CardSlot>();
        var rightSlots = new System.Collections.Generic.Dictionary<Vector2Int, CardSlot>();

        foreach (var slot in allSlots)
        {
            if (slot.IsOccupied && slot.OccupiedCard != null)
            {
                Vector2Int gridPos = ParseSlotPosition(slot.name);
                if (gridPos != Vector2Int.one * -1)
                {
                    if (slot.SlotType == 1)
                    {
                        leftSlots[gridPos] = slot;
                    }
                    else if (slot.SlotType == 2)
                    {
                        rightSlots[gridPos] = slot;
                    }
                }
            }
        }

        var correspondenceMap = GetCorrespondenceMap(leftSlots.Count, rightSlots.Count);

        int matchCount = 0;
        int matchedRightScore = 0;
        int unmatchedRightScore = 0;

        foreach (var rightSlot in rightSlots)
        {
            Vector2Int rightPos = rightSlot.Key;
            CardSlot rightCardSlot = rightSlot.Value;
            int rightScore = rightCardSlot.OccupiedCard.Card.Score;

            Vector2Int? correspondingLeftPos = null;
            foreach (var mapping in correspondenceMap)
            {
                if (mapping.Value == rightPos)
                {
                    correspondingLeftPos = mapping.Key;
                    break;
                }
            }

            if (correspondingLeftPos.HasValue)
            {
                Vector2Int leftPos = correspondingLeftPos.Value;

                if (leftSlots.ContainsKey(leftPos))
                {
                    CardSlot leftCardSlot = leftSlots[leftPos];
                    CardColor leftColor = leftCardSlot.OccupiedCard.Card.Color;
                    CardColor rightColor = rightCardSlot.OccupiedCard.Card.Color;

                    if (leftColor == rightColor)
                    {
                        matchCount++;
                        matchedRightScore += rightScore;
                    }
                    else
                    {
                        unmatchedRightScore += rightScore;
                    }
                }
                else
                {
                    unmatchedRightScore += rightScore;
                }
            }
            else
            {
                unmatchedRightScore += rightScore;
            }
        }

        int multiplier = matchCount;
        int totalScore;

        if (matchCount >= 1)
        {
            totalScore = -((matchedRightScore * multiplier) + unmatchedRightScore);
        }
        else
        {
            totalScore = -(matchedRightScore + unmatchedRightScore);
        }

        return (totalScore, multiplier);
    }

    private System.Collections.Generic.Dictionary<Vector2Int, Vector2Int> GetCorrespondenceMap(int leftCount, int rightCount)
    {
        var map = new System.Collections.Generic.Dictionary<Vector2Int, Vector2Int>();
        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);
        int totalLeftSlots = 0;
        int totalRightSlots = 0;

        foreach (var slot in allSlots)
        {
            if (slot.SlotType == 1) totalLeftSlots++;
            else if (slot.SlotType == 2) totalRightSlots++;
        }


        if (totalLeftSlots == 4 && totalRightSlots == 4)
        {
            map[new Vector2Int(0, 0)] = new Vector2Int(0, 0);
            map[new Vector2Int(0, 1)] = new Vector2Int(0, 1);
            map[new Vector2Int(1, 0)] = new Vector2Int(1, 0);
            map[new Vector2Int(1, 1)] = new Vector2Int(1, 1);
        }
        else if (totalLeftSlots == 3 && totalRightSlots == 5)
        {
            map[new Vector2Int(0, 0)] = new Vector2Int(1, 0);
            map[new Vector2Int(0, 1)] = new Vector2Int(1, 1);
            map[new Vector2Int(1, 1)] = new Vector2Int(2, 1);
        }

        return map;
    }

    private void UpdateScoreDisplay()
    {
        // 添加调试信息来追踪谁在调用这个方法
        Debug.Log($"UpdateScoreDisplay被调用! 当前总分: {currentScore}");
        Debug.Log($"调用堆栈: {System.Environment.StackTrace}");
        
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
        GameOverSystem.Instance.CheckGameOver(currentScore);
    }

    // 修改：回合结束时累加分数并检测游戏结束
    public void UpdateScoreOnNextTurn()
    {
        // 先重新计算当前回合的分数（确保是最新的）
        RecalculateCurrentTurnScore();
        
        // 累加当前回合的左右分数到总分
        currentScore += currentLeftScore + currentRightScore;
        
        // 检测游戏结束（在显示更新前先检测）
        GameOverSystem.Instance.CheckGameOver(currentScore);
        
        // 更新总分显示（只在这里更新）
        UpdateScoreDisplay();
        
        // 重置当前回合分数（为下回合准备）
        currentLeftScore = 0;
        currentRightScore = 0;
    }
    
    // 新增：重新计算当前回合分数（不更新显示）
    private void RecalculateCurrentTurnScore()
    {
        int bonusScore = 0;
        int colorPenalty = 0;

        CardSlot[] allSlots = FindObjectsByType<CardSlot>(FindObjectsSortMode.None);

        // 计算左右两侧的不同花色奖励
        bonusScore += CalculateColorVarietyBonus(allSlots, 1); // 左侧
        bonusScore += CalculateColorVarietyBonus(allSlots, 2); // 右侧
        
        colorPenalty = CalculateColorPenalty(allSlots);

        var leftSlotScoreResult = CalculateLeftSlotScoreWithMultiplier(allSlots);
        int leftSlotScore = leftSlotScoreResult.totalScore;

        var rightSlotScoreResult = CalculateRightSlotScoreWithMultiplier(allSlots);
        int rightSlotScore = rightSlotScoreResult.totalScore;

        int leftAdjacentBonus = CalculateAdjacentSameBonusInSlotType(allSlots, 1);
        int rightAdjacentPenalty = CalculateAdjacentSameBonusInSlotType(allSlots, 2);

        // 计算当前回合的左右分数（不包含累积分数）
        currentLeftScore = leftSlotScore + bonusScore + leftAdjacentBonus - colorPenalty;
        currentRightScore = rightSlotScore + rightAdjacentPenalty;
    }

    private void UpdateLeftTotalScoreDisplay(int leftTotalScore)
    {
        if (leftTotalScoreText != null)
        {
            leftTotalScoreText.text = leftTotalScore.ToString();
        }
    }

    private void UpdateRightTotalScoreDisplay(int rightTotalScore)
    {
        if (rightTotalScoreText != null)
        {
            rightTotalScoreText.text = rightTotalScore.ToString();
        }
    }

    private void UpdateRightCardsProgressDisplay()
    {
        if (rightCardsProgressText != null)
        {
            int remainingCards = totalRightDeckCards - completedRightCards;
            rightCardsProgressText.text = $"{remainingCards}/{totalRightDeckCards}";
        }
    }

    private void UpdateAllDisplays()
    {
        // 开始时不自动更新总分显示，只更新其他显示
        // UpdateScoreDisplay(); // 注释掉，总分只在按按钮时更新
        UpdateLeftTotalScoreDisplay(0);
        UpdateRightTotalScoreDisplay(0);
        UpdateRightCardsProgressDisplay();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}