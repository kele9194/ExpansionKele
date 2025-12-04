using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Summon
{
    /// <summary>
    /// 望月法杖 - 召唤围绕玩家旋转的月亮
    /// </summary>
    public class FullMoonStaff : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月法杖");
            // Tooltip.SetDefault("召唤围绕玩家旋转的月亮\n月亮会造成伤害并施加满月减益");
            
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // 游戏手柄使用全屏范围
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true; // 锁定时忽略碰撞
        }

        public override void SetDefaults()
        {
            // Item.SetNameOverride("望月法杖");
            Item.damage = ExpansionKele.ATKTool(120, 150);         // 基础伤害值
            Item.DamageType = DamageClass.Summon;                // 召唤伤害类型
            Item.width = 26;                                     // 物品宽高
            Item.height = 28;
            Item.useTime = 36;                                   // 使用时间（帧数）
            Item.useAnimation = 36;                              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing;                // 使用样式为挥舞
            Item.noMelee = true;                                 // 关闭近战攻击判定
            Item.knockBack = 1;                                  // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);                       // 稀有度：粉红
            Item.UseSound = SoundID.Item44;                      // 使用音效
            Item.shoot = ModContent.ProjectileType<FullMoonMinionController>(); // 发射主控弹幕
            Item.shootSpeed = 10f;                               // 弹幕初始速度
            Item.mana = 10;                                      // 消耗法力值
            Item.autoReuse = false;                              // 不自动重用，限制召唤一次
            Item.buffType = ModContent.BuffType<FullMoonMinionBuff>(); // 对应的Buff
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 添加持续时间较长的Buff，确保召唤物能够生成
            player.AddBuff(Item.buffType, 36000); // 10分钟

            // 计算鼠标位置与玩家中心的距离
            Vector2 playerCenter = player.Center;
            Vector2 mousePosition = Main.MouseWorld;
            float distanceToMouse = Vector2.Distance(playerCenter, mousePosition);
            
            // 将距离限制在最小80和最大640之间
            int distanceLevel = (int)MathHelper.Clamp((distanceToMouse - 80) / 80, 0, 7);

            // 检查是否已经召唤过主控弹幕
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonMinionController>()] <= 0)
            {
                // 只有当没有主控弹幕存在时才召唤，并设置初始距离层级
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, distanceLevel, 0f);
            }
            else
            {
                // 如果主控弹幕已存在，更新月亮的距离
                UpdateMoonDistanceToTarget(player, distanceLevel);
            }
            
            return false; // 阻止原版弹幕发射
        }
        
        // 更新月亮与玩家的距离
        private void UpdateMoonDistanceToTarget(Player player, int targetDistanceLevel)
        {
            // 查找主控弹幕并调用其更新方法
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<FullMoonMinionController>() && proj.owner == player.whoAmI)
                {
                    // 调用主控弹幕的更新距离方法
                    var controller = proj.ModProjectile as FullMoonMinionController;
                    if (controller != null)
                    {
                        controller.UpdateMoonDistanceToTarget(targetDistanceLevel);
                    }
                    break;
                }
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"Tooltip", "[c/800000:召唤6个围绕玩家的距离可变（用鼠标）的月亮]"},
                    {"Tooltip2", "[c/800000:限召唤一次，使用2仆从栏"}
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