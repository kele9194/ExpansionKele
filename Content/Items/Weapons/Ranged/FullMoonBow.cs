using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    /// <summary>
    /// 望月弓 - 发射两支满月箭矢。
    /// 箭矢在穿透敌人、碰到地形或达到生命周期后返回玩家身边。
    /// 返回时伤害变为50%，并具有更强追踪性与特效。
    /// </summary>
    public class FullMoonBow : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            // 可选：设置显示名称和描述（用于调试或国际化）
            // DisplayName.SetDefault("Full Moon Bow");
            // Tooltip.SetDefault("Fires two arrows that transform into full moon arrows");
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月弓");         // 设置物品中文名
            Item.damage = ExpansionKele.ATKTool(48,default);                       // 基础伤害值
            Item.DamageType = DamageClass.Ranged;   // 远程伤害类型
            Item.width = 20;                        // 物品宽高
            Item.height = 40;
            Item.useTime = 24;                      // 使用时间（帧数）
            Item.useAnimation = 24;                 // 动画持续时间
            Item.useStyle = ItemUseStyleID.Shoot;   // 使用样式为射击
            Item.noMelee = true;                    // 关闭近战攻击判定
            Item.knockBack = 2;                     // 击退值
            Item.value = Item.sellPrice(gold: 10);  // 卖出价格
            Item.rare = ItemRarityID.Red;           // 稀有度：红色
            Item.UseSound = SoundID.Item5;          // 射击音效
            Item.autoReuse = true;                  // 自动重用
            Item.shoot = ProjectileID.WoodenArrowFriendly; // 默认发射物（会被替换）
            Item.shootSpeed = 10f;                  // 初始弹速
            Item.useAmmo = AmmoID.Arrow;            // 弹药类型为箭类
        }

        /// <summary>
        /// 自定义射击行为：发射两发箭矢，并增加随机偏转和速度倍率。
        /// </summary>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 2; i++)
            {
                // 添加随机偏转角度（±4°）
                Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(4));
                perturbedSpeed *= 1.5f; // 提高初始速度

                // 发射满月箭矢
                Projectile.NewProjectile(source, position, perturbedSpeed,
                    ModContent.ProjectileType<FullMoonArrowProj>(), damage, knockback, player.whoAmI);
            }

            return false; // 防止原版箭矢也被发射
        }

        /// <summary>
        /// 所有远程箭类武器攻击时都会被替换成满月箭。
        /// </summary>
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 将所有类型的箭统一替换为 FullMoonArrowProj
            type = ModContent.ProjectileType<FullMoonArrowProj>();
        }

        /// <summary>
        /// 添加自定义提示信息
        /// </summary>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // tooltips.Add(new TooltipLine(Mod, "FullMoonSwordTooltip", 
            //     Language.GetText("Mods.ExpansionKele.Items.FullMoonBow.Tooltip1").Value));
            // tooltips.Add(new TooltipLine(Mod, "FullMoonSwordTooltip1",
            //     Language.GetText("Mods.ExpansionKele.Items.FullMoonBow.Tooltip2").Value));
        }

        /// <summary>
        /// 合成配方：需要8个【满月锭】，在铁砧上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FullMoonBar>(), 8) // 材料修正
                .AddTile(TileID.Anvils) // 合成台使用铁砧
                .Register();
        }
    }
}