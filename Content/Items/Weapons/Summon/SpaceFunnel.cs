using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.SummonProj;

namespace ExpansionKele.Content.Items.Weapons.Summon
{
    /// <summary>
    /// SpaceFunnel - 召唤浮游炮武器
    /// 召唤飞行的浮游炮，每个占用1个召唤栏位，定期发射绿色光束
    /// </summary>
    public class SpaceFunnel : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("空间漏斗");
            // Tooltip.SetDefault("召唤飞行的浮游炮\n每个浮游炮占用1个召唤栏位\n浮游炮会定期发射可无限穿透的绿色光束");
            
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // 游戏手柄使用全屏范围
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true; // 锁定时忽略碰撞
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // 每次使用占用1个召唤栏
        }

        public override void SetDefaults()
        {
            // Item.SetNameOverride("空间漏斗");
            Item.damage = ExpansionKele.ATKTool(40, 52);         // 基础伤害值
            Item.DamageType = DamageClass.Summon;                // 召唤伤害类型
            Item.width = 28;                                     // 物品宽高
            Item.height = 30;
            Item.useTime = 30;                                   // 使用时间（帧数）
            Item.useAnimation = 30;                              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing;                // 使用样式为挥舞
            Item.noMelee = true;                                 // 关闭近战攻击判定
            Item.knockBack = 2f;                                 // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);                       // 稀有度
            Item.UseSound = SoundID.Item44;                      // 使用音效
            Item.shoot = ModContent.ProjectileType<SpaceFunnelMinion>(); // 发射浮游炮弹幕
            Item.mana = 8;                                       // 消耗法力值
            Item.autoReuse = true;                               // 自动重用，允许连续召唤
            Item.buffType = ModContent.BuffType<SpaceFunnelMinionBuff>(); // 对应的Buff
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 在鼠标位置生成，但限制在玩家可达范围内
            position = Main.MouseWorld;
            player.LimitPointToPlayerReachableArea(ref position);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 添加持续时间较短的Buff，确保召唤物能够生成
            player.AddBuff(Item.buffType, 2);

            return true; // 让游戏自动处理弹幕生成
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        /// <summary>
        /// 注册合成配方：需要8个星辰碎片和5个光明之魂，在秘银砧上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpaceGun, 8)
                .AddIngredient(ItemID.SoulofFlight, 5)
                .AddIngredient(ItemID.CursedFlame,5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}