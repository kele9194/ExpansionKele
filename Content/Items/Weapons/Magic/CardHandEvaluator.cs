using System;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    /// <summary>
    /// 牌型枚举 - 按强度排序
    /// </summary>
    public enum HandType
    {
        HighCard = 0,       // 高牌
        OnePair = 1,        // 一对
        TwoPair = 2,        // 两对
        ThreeOfAKind = 3,   // 三条
        Straight = 4,       // 顺子
        Flush = 5,          // 同花
        FullHouse = 6,      // 满堂红
        FourOfAKind = 7,    // 四条
        StraightFlush = 8,  // 同花顺
        RoyalFlush = 9      // 皇家同花顺
    }
    
    /// <summary>
    /// 手牌评估器 - 零 GC 的牌型判定工具类
    /// 使用频次统计法，O(N) 复杂度，无需排序
    /// </summary>
    public static class CardHandEvaluator
    {
        // 预分配的统计数组（避免每次分配内存）
        private static readonly int[] _rankCounts = new int[15]; // 索引 2-14
        private static readonly int[] _suitCounts = new int[4];  // 索引 0-3

        private static readonly float[] _multipliers = new float[]
        {
            1f,   // HighCard
            2f,   // OnePair
            5f,   // TwoPair
            8f,   // ThreeOfAKind
            10f,  // Straight
            10f,  // Flush
            15f,  // FullHouse
            18f,  // FourOfAKind
            20f,  // StraightFlush
            25f   // RoyalFlush
        };
        
        /// <summary>
        /// 评估手牌类型（5 张牌）
        /// 返回牌型枚举和倍率
        /// </summary>
        public static HandType Evaluate(CardData[] hand, out float multiplier)
        {
            // 清空统计数组
            Array.Clear(_rankCounts, 0, _rankCounts.Length);
            Array.Clear(_suitCounts, 0, _suitCounts.Length);
            
            // 单次遍历填表
            for (int i = 0; i < 5; i++)
            {
                _rankCounts[hand[i].Rank]++;
                _suitCounts[hand[i].Suit]++;
            }
            
            // 统计信息
            bool isFlush = false;
            bool isStraight = false;
            int maxCount = 0;
            int pairCount = 0;
            
            // 检查同花
            for (int s = 0; s < 4; s++)
            {
                if (_suitCounts[s] == 5)
                {
                    isFlush = true;
                    break;
                }
            }
            
            // 检查顺子
            // 特殊检查：10-J-Q-K-A (皇家顺子)
            if (_rankCounts[10] >= 1 && _rankCounts[11] >= 1 && _rankCounts[12] >= 1 && 
                _rankCounts[13] >= 1 && _rankCounts[14] >= 1)
            {
                isStraight = true;
            }
            else
            {
                // 检查普通顺子（连续 5 张）
                int consecutiveCount = 0;
                for (int r = 2; r <= 14; r++)
                {
                    if (_rankCounts[r] >= 1)
                    {
                        consecutiveCount++;
                        if (consecutiveCount >= 5)
                        {
                            isStraight = true;
                            break;
                        }
                    }
                    else
                    {
                        consecutiveCount = 0;
                    }
                }
            }
            
            // 统计对子和条子
            for (int r = 2; r <= 14; r++)
            {
                if (_rankCounts[r] == 2)
                    pairCount++;
                else if (_rankCounts[r] >= 3)
                    maxCount = Math.Max(maxCount, _rankCounts[r]);
            }
            
            // 确定牌型
            HandType handType;
            
            if (isStraight && isFlush)
            {
                // 检查是否为皇家同花顺 (10-J-Q-K-A)
                if (_rankCounts[10] >= 1 && _rankCounts[11] >= 1 && _rankCounts[12] >= 1 && 
                    _rankCounts[13] >= 1 && _rankCounts[14] >= 1)
                {
                    handType = HandType.RoyalFlush;
                    multiplier = 25f;
                }
                else
                {
                    handType = HandType.StraightFlush;
                    multiplier = 20f;
                }
            }
            else if (maxCount == 4)
            {
                handType = HandType.FourOfAKind;
                multiplier = 18f;
            }
            else if (maxCount == 3 && pairCount >= 1)
            {
                handType = HandType.FullHouse;
                multiplier = 15f;
            }
            else if (isFlush)
            {
                handType = HandType.Flush;
                multiplier = 10f;
            }
            else if (isStraight)
            {
                handType = HandType.Straight;
                multiplier = 10f;
            }
            else if (maxCount == 3)
            {
                handType = HandType.ThreeOfAKind;
                multiplier = 8f;
            }
            else if (pairCount >= 2)
            {
                handType = HandType.TwoPair;
                multiplier = 5f;
            }
            else if (pairCount == 1)
            {
                handType = HandType.OnePair;
                multiplier = 2f;
            }
            else
            {
                handType = HandType.HighCard;
                multiplier = 1f;
            }
            
            return handType;
        }
        
        /// <summary>
        /// 计算手牌的点数总和（用于伤害计算）
        /// </summary>
        public static int CalculateRankSum(CardData[] hand)
        {
            int sum = 0;
            for (int i = 0; i < 5; i++)
            {
                sum += hand[i].Rank;
            }
            return sum;
        }
        
        /// <summary>
        /// 获取牌型的中文名称
        /// </summary>
        public static string GetHandTypeName(HandType handType)
        {
            switch (handType)
            {
                case HandType.RoyalFlush: return "皇家同花顺";
                case HandType.StraightFlush: return "同花顺";
                case HandType.FourOfAKind: return "四条";
                case HandType.FullHouse: return "满堂红";
                case HandType.Flush: return "同花";
                case HandType.Straight: return "顺子";
                case HandType.ThreeOfAKind: return "三条";
                case HandType.TwoPair: return "两对";
                case HandType.OnePair: return "一对";
                case HandType.HighCard: return "高牌";
                default: return "未知";
            }
        }

        /// <summary>
        /// 根据 HandType 获取倍率（查表法，零 GC）
        /// </summary>
        public static float GetMultiplier(HandType handType)
        {
            return _multipliers[(int)handType];
        }
    }
}