using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    /// <summary>
    /// 卡牌数据结构 - 使用 struct 避免 GC 压力
    /// </summary>
    public struct CardData : IEquatable<CardData>
    {
        public byte Rank;  // 点数：2-14 (A=14)
        public byte Suit;  // 花色：0=黑桃，1=红桃，2=梅花，3=方块
        
        public CardData(byte rank, byte suit)
        {
            Rank = rank;
            Suit = suit;
        }
        
        public bool Equals(CardData other)
        {
            return Rank == other.Rank && Suit == other.Suit;
        }
        
        public override bool Equals(object obj)
        {
            return obj is CardData other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return (Rank << 4) | Suit;
        }
    }
    
     /// <summary>
    /// 静态牌组工具类 - 提供零 GC 的抽牌和权重随机功能
    /// </summary>
        public static class CardDeck
    {
        // 预定义的完整牌组（52 张牌）
        private static readonly CardData[] _fullDeck;
        
        // 用于权重随机的临时缓冲区（线程安全需要注意，但在主线程中安全）
        private static readonly float[] _weightBuffer = new float[52];
        
        // 牌组常量
        public const int TOTAL_CARDS = 52;
        public const int NUM_SUITS = 4;
        public const int NUM_RANKS = 13;
        public const int MIN_RANK = 2;
        public const int MAX_RANK = 14; // A=14
        
        // 测试模式标志 - 用于测试时统一使用 Club2Test.png
        public static bool TestMode = false;
        
        
        // 花色名称（用于显示）
        public static readonly string[] SuitNames = { "Spades", "Hearts", "Clubs", "Diamonds" };
        
        // 点数名称（用于显示）
        public static readonly string[] RankNames = { "", "", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
        
        /// <summary>
        /// 静态构造函数 - 初始化完整牌组
        /// </summary>
        static CardDeck()
        {
            _fullDeck = new CardData[TOTAL_CARDS];
            
            int index = 0;
            // 按花色顺序填充：黑桃 (0)、红桃 (1)、梅花 (2)、方块 (3)
            for (byte suit = 0; suit < NUM_SUITS; suit++)
            {
                // 每个花色有 13 张牌：2-14 (A=14)
                for (byte rank = MIN_RANK; rank <= MAX_RANK; rank++)
                {
                    _fullDeck[index++] = new CardData(rank, suit);
                }
            }
        }
// ... existing code ...

      
        /// <summary>
        /// 计算单张牌的权重修正（基于手牌运气值）
        /// </summary>
        private static float CalculateRankWeightModifier(byte rank, float handLuckValue)
        {
            if (handLuckValue <= 0)
                return 0f;
            
            float modifier = 0f;
            
            const float K_VALUE = 0.02f;
            const float MAX_WEIGHT_REDUCTION = 0.5f;
            
            if (rank >= 8)
            {
                // 8 以上的牌：每点幸运值增加权重 k*ln(t-7+1)
                float lnValue = (float)Math.Log(rank - 7 + 1);
                modifier = handLuckValue * K_VALUE * lnValue;
            }
            else
            {
                // 7 以下的牌：每点幸运值降低权重 0.7*k*ln(8-t+1)
                float lnValue = (float)Math.Log(8 - rank + 1);
                float reduction = handLuckValue * 0.7f * K_VALUE * lnValue;
                modifier = -Math.Min(reduction, MAX_WEIGHT_REDUCTION);
            }
            
            return modifier;
        }
        /// <summary>
        /// 生成一手牌（5 张），写入预分配的缓冲区
        /// 使用 Fisher-Yates 洗牌算法确保五张牌不重复
        /// </summary>
        public static void GenerateHand(CardData[] handBuffer, float luck, float handLuckValue = 0f)
        {
            luck = MathHelper.Clamp(luck, -0.3f, 0.3f);
            if (TestMode)
            {
                // 测试模式：全部返回梅花 2（Suit=2, Rank=2）
                for (int i = 0; i < 5; i++)
                {
                    handBuffer[i] = new CardData(2, 2);
                }
            }
            else
            {
                // 正常模式：创建临时牌组副本并应用权重
                int[] deckIndices = new int[TOTAL_CARDS];
                for (int i = 0; i < TOTAL_CARDS; i++)
                {
                    deckIndices[i] = i;
                }
                
                // 计算每张牌的权重
                for (int i = 0; i < TOTAL_CARDS; i++)
                {
                    byte rank = _fullDeck[i].Rank;
                    float weight = 1f;
                    
                    if (luck > 0)
                    {
                        if (rank >= 8)
                            weight += luck * 0.5f;
                    }
                    else if (luck < 0)
                    {
                        if (rank < 8)
                            weight -= luck * 0.5f;
                    }
                    
                    // 应用手牌运气值的权重修正
                    float handLuckModifier = CalculateRankWeightModifier(rank, handLuckValue);
                    weight += handLuckModifier;
                    
                    _weightBuffer[i] = Math.Max(0.1f, weight);
                }
                
                // 使用 Fisher-Yates 洗牌算法，带权重随机
                for (int i = 0; i < 5; i++)
                {
                    int remainingCards = TOTAL_CARDS - i;
                    
                    // 根据权重选择一张牌
                    float totalWeight = 0f;
                    for (int j = i; j < TOTAL_CARDS; j++)
                    {
                        totalWeight += _weightBuffer[deckIndices[j]];
                    }
                    
                    float randomValue = Main.rand.NextFloat() * totalWeight;
                    float cumulativeWeight = 0f;
                    int selectedIndex = i;
                    
                    for (int j = i; j < TOTAL_CARDS; j++)
                    {
                        cumulativeWeight += _weightBuffer[deckIndices[j]];
                        if (randomValue <= cumulativeWeight)
                        {
                            selectedIndex = j;
                            break;
                        }
                    }
                    
                    // 交换选中的牌到当前位置（Fisher-Yates 核心步骤）
                    int temp = deckIndices[i];
                    deckIndices[i] = deckIndices[selectedIndex];
                    deckIndices[selectedIndex] = temp;
                    
                    // 将选中的牌加入手牌
                    handBuffer[i] = _fullDeck[deckIndices[i]];
                }
            }
        }
// ... existing code ...
        /// <summary>
        /// 从打包的整数中解码卡牌数据
        /// </summary>
        public static CardData DecodeCard(int hashCode)
        {
            byte rank = (byte)((hashCode >> 4) & 0x0F);
            byte suit = (byte)(hashCode & 0x0F);
            return new CardData(rank, suit);
        }
        
        
        /// <summary>
        /// 将单张卡牌转换为带 Unicode 花色符号的字符串
        /// 花色映射：0=黑桃♠, 1=红桃♥, 2=梅花♣, 3=方块♦
        /// </summary>
        public static string CardToString(CardData card)
        {
            string rankStr = GetRankString(card.Rank);
            string suitSymbol = GetSuitSymbol(card.Suit);
            return $"{rankStr}{suitSymbol}";
        }
        
        /// <summary>
        /// 获取点数对应的字符串
        /// </summary>
        public static string GetRankString(byte rank)
        {
            switch (rank)
            {
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                case 14: return "A";
                default: return rank.ToString();
            }
        }
        
        /// <summary>
        /// 获取花色对应的 Unicode 符号
        /// 0=黑桃♠, 1=红桃♥, 2=梅花♣, 3=方块♦
        /// </summary>
        public static string GetSuitSymbol(byte suit)
        {
            switch (suit)
            {
                case 0: return "♠";
                case 1: return "♥";
                case 2: return "♣";
                case 3: return "♦";
                default: return "?";
            }
        }
        
        /// <summary>
        /// 格式化手牌显示，如 "2♠, 3♥, Q♣, K♦, A♠"
        /// </summary>
        public static string FormatHandDisplay(CardData[] hand)
        {
            if (hand == null || hand.Length == 0)
                return "";
            
            string[] cards = new string[hand.Length];
            for (int i = 0; i < hand.Length; i++)
            {
                cards[i] = CardToString(hand[i]);
            }
            return string.Join(", ", cards);
        }
        /// <summary>
        /// 获取卡牌的颜色（用于绘制）
        /// </summary>
        public static Microsoft.Xna.Framework.Color GetCardColor(byte suit)
        {
            switch (suit)
            {
                case 0: // 黑桃 - 黑色
                    return new Microsoft.Xna.Framework.Color(30, 30, 30);
                case 1: // 红桃 - 红色
                    return new Microsoft.Xna.Framework.Color(220, 30, 30);
                case 2: // 梅花 - 黑色
                    return new Microsoft.Xna.Framework.Color(30, 30, 30);
                case 3: // 方块 - 蓝色
                    return new Microsoft.Xna.Framework.Color(30, 100, 220);
                default:
                    return Microsoft.Xna.Framework.Color.White;
            }
        }
        public static Microsoft.Xna.Framework.Color GetRarityColor(HandType handType)
        {
            int rarityLevel = (int)handType;
            
            if (rarityLevel > ItemRarityID.Purple)
            {
                rarityLevel = ItemRarityID.Purple;
            }
            
            switch (rarityLevel)
            {
                case ItemRarityID.White:
                    return new Microsoft.Xna.Framework.Color(255, 255, 255);
                case ItemRarityID.Blue:
                    return new Microsoft.Xna.Framework.Color(0, 171, 255);
                case ItemRarityID.Green:
                    return new Microsoft.Xna.Framework.Color(160, 255, 0);
                case ItemRarityID.Orange:
                    return new Microsoft.Xna.Framework.Color(255, 196, 0);
                case ItemRarityID.LightRed:
                    return new Microsoft.Xna.Framework.Color(255, 100, 100);
                case ItemRarityID.Pink:
                    return new Microsoft.Xna.Framework.Color(255, 128, 200);
                case ItemRarityID.Purple:
                    return new Microsoft.Xna.Framework.Color(180, 100, 255);
                case ItemRarityID.Lime:
                    return new Microsoft.Xna.Framework.Color(128, 255, 128);
                case ItemRarityID.Yellow:
                    return new Microsoft.Xna.Framework.Color(255, 255, 0);
                case ItemRarityID.Cyan:
                    return new Microsoft.Xna.Framework.Color(0, 255, 255);
                case ItemRarityID.Red:
                    return new Microsoft.Xna.Framework.Color(255, 0, 0);
                default:
                    return new Microsoft.Xna.Framework.Color(255, 255, 255);
            }
        }
      /// <summary>
        /// 根据牌型稀有度获取光照强度
        /// </summary>
        public static float GetLightIntensityByRarity(HandType handType)
        {
            switch (handType)
            {
                case HandType.RoyalFlush:
                    return 1.5f;
                case HandType.StraightFlush:
                    return 1.3f;
                case HandType.FourOfAKind:
                    return 1.2f;
                case HandType.FullHouse:
                case HandType.Flush:
                    return 1.1f;
                case HandType.Straight:
                case HandType.ThreeOfAKind:
                    return 1.0f;
                case HandType.TwoPair:
                    return 0.9f;
                case HandType.OnePair:
                    return 0.8f;
                default:
                    return 0.7f;
            }
        }
        
        /// <summary>
        /// 根据牌型稀有度获取发光缩放比例
        /// </summary>
        public static float GetGlowScaleByRarity(HandType handType)
        {
            switch (handType)
            {
                case HandType.RoyalFlush:
                    return 1.5f;
                case HandType.StraightFlush:
                    return 1.35f;
                case HandType.FourOfAKind:
                    return 1.25f;
                case HandType.FullHouse:
                case HandType.Flush:
                    return 1.15f;
                case HandType.Straight:
                case HandType.ThreeOfAKind:
                    return 1.05f;
                case HandType.TwoPair:
                    return 0.95f;
                case HandType.OnePair:
                    return 0.85f;
                default:
                    return 0.75f;
            }
        }
        
    }
}