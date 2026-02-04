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
    /// 望月法杖 - 召唤围绕玩家旋转的月亮
    /// 每次使用召唤一个月亮，消耗1个召唤栏，无上限
    /// </summary>
    public class FullMoonStaff : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月法杖");
            // Tooltip.SetDefault("召唤围绕玩家旋转的月亮\n每次召唤消耗1个召唤栏，无上限\n月亮具有接触伤害和局部无敌帧");
            
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // 游戏手柄使用全屏范围
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true; // 锁定时忽略碰撞
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // 每次使用占用1个召唤栏
        }

        public override void SetDefaults()
        {
            // Item.SetNameOverride("望月法杖");
            Item.damage = ExpansionKele.ATKTool(40, 45);         // 基础伤害值
            Item.DamageType = DamageClass.Summon;                // 召唤伤害类型
            Item.width = 26;                                     // 物品宽高
            Item.height = 28;
            Item.useTime = 36;                                   // 使用时间（帧数）
            Item.useAnimation = 36;                              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing;                // 使用样式为挥舞
            Item.noMelee = true;                                 // 关闭近战攻击判定
            Item.knockBack = 3f;                                 // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);                       // 稀有度：粉红
            Item.UseSound = SoundID.Item44;                      // 使用音效
            Item.shoot = ModContent.ProjectileType<FullMoonMinion>(); // 直接发射月亮弹幕
            Item.mana = 10;                                      // 消耗法力值
            Item.autoReuse = true;                               // 自动重用，允许连续召唤
            Item.buffType = ModContent.BuffType<FullMoonMinionBuff>(); // 对应的Buff
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
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"Tooltip", "[c/800000:每次使用召唤一个月亮，消耗1个召唤栏]"},
                    {"Tooltip2", "[c/800000:可无限召唤，月亮具有接触伤害]"},
                    {"Tooltip3", "[c/800000:每个月亮拥有独立的局部无敌帧]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }

        /// <summary>
        /// 注册合成配方：需要12个【满月锭】，在秘银砧或山铜砧上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FullMoonBar>(), 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}