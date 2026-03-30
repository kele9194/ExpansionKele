using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    /// <summary>
    /// 卡牌大师 - 魔法武器
    /// 每次攻击随机抽取 5 张牌，根据牌型造成不同倍率的伤害
    /// </summary>
        public class CardMaster : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Magic";
        public const int BASE_DAMAGE = 100;
        public static LocalizedText currentHandLuckText { get; private set; }
        
        // 预分配的手牌缓冲区（避免 GC）
        private static readonly CardData[] _handBuffer = new CardData[5];
        
        public override void SetStaticDefaults()
        {
            currentHandLuckText = this.GetLocalization("CurrentHandLuckText");
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(BASE_DAMAGE);
            Item.DamageType = DamageClass.Magic;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CardProjectile>();
            Item.shootSpeed = 12f;
            Item.mana = 8;
        }


public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
{
    // 获取玩家的卡牌运气系统
    CardLuckPlayer cardLuckPlayer = player.GetModPlayer<CardLuckPlayer>();
    
    // 生成新手牌（此时会使用更新后的手牌运气值）
    CardDeck.GenerateHand(_handBuffer, player.luck, cardLuckPlayer.HandLuckValue);
    
    HandType handType = CardHandEvaluator.Evaluate(_handBuffer, out float typeMultiplier);
    int rankSum = CardHandEvaluator.CalculateRankSum(_handBuffer);

    
    // 提前计算 ai 值
    int packedHash0 = 
        _handBuffer[0].GetHashCode() |
        (_handBuffer[1].GetHashCode() << 8) |
        (_handBuffer[2].GetHashCode() << 16);
    
    int packedHash1 = 
        _handBuffer[3].GetHashCode() |
        (_handBuffer[4].GetHashCode() << 8) |
        ((int)handType << 16);
    
    
    // 创建投射物并传入 ai 值
    int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, packedHash0, packedHash1);
    
    if (projIndex >= 0 && projIndex < Main.maxProjectiles)
    {
        Projectile proj = Main.projectile[projIndex];
        proj.netUpdate = true;
    }
    cardLuckPlayer.UpdateHandLuck(handType);
    
    return false;
}

// ... existing code ...
        
// ... existing code ...
        
// ... existing code ...
        
// ... existing code ...
public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        Player player = Main.LocalPlayer;
        CardLuckPlayer cardLuckPlayer = player.GetModPlayer<CardLuckPlayer>();
        
        string currentHandLuckStr = currentHandLuckText.WithFormatArgs(ValueUtils.FormatValue(cardLuckPlayer.HandLuckValue)).Value;
        tooltips.Add(new TooltipLine(Mod, "CurrentHandLuck", currentHandLuckStr));
    }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddIngredient(ItemID.BambooBlock, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
        
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20f, -2f);
        }
    }
    /// <summary>
    /// 卡牌手气管理系统 - 每个玩家独立的手牌运气值
    /// 用于解决多人游戏中所有玩家共享同一个运气值的问题
    /// </summary>
    public class CardLuckPlayer : ModPlayer
    {
        /// <summary>
        /// 手牌运气值 - 累积玩家的牌运
        /// 差牌增加运气值，好牌减少运气值
        /// </summary>
        public float HandLuckValue { get; set; } = 0f;
        
        /// <summary>
        /// 记录上一次的手牌类型，用于在下次抽牌前更新运气值
        /// </summary>
        public HandType? LastHandType { get; set; } = null;
        
        
        /// <summary>
        /// 根据牌型更新手牌运气值
        /// 应该在每次评估牌型后调用
        /// </summary>
        public void UpdateHandLuck(HandType handType)
        {
            switch (handType)
            {
                case HandType.HighCard:
                    HandLuckValue += 4f;
                    break;
                case HandType.OnePair:
                    HandLuckValue += 2f;
                    break;
                case HandType.TwoPair:
                case HandType.ThreeOfAKind:
                    HandLuckValue += 1f;
                    break;
                default:
                    // 其他牌型（顺子、同花、葫芦、四条、同花顺、皇家同花顺）不增加运气值，而是将当前运气值减半
                    if (HandLuckValue > 0)
                    {
                        HandLuckValue = (int)(HandLuckValue *0.7f);
                    }
                    break;
            }
            
        }
        
        /// <summary>
        /// 获取当前的手牌运气值
        /// </summary>
        public float GetHandLuckValue()
        {
            return HandLuckValue;
        }
        
        /// <summary>
        /// 重置手牌运气值（例如玩家死亡或退出游戏时）
        /// </summary>
         public override void OnEnterWorld()
        {
            HandLuckValue = 0f;
            LastHandType = null;
        }
        
        /// <summary>
        /// 玩家死亡时重置运气值（可选）
        /// </summary>
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {

            HandLuckValue = 0f;
            LastHandType = null;
        }
    }
}
