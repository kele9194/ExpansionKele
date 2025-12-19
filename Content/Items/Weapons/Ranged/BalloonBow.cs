using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class BalloonBow : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(14, 16);         // 基础伤害值
            Item.DamageType = DamageClass.Ranged;   // 远程伤害类型
            Item.width = 20;                        // 物品宽高
            Item.height = 40;
            Item.useTime = 14;                      // 使用时间（帧数）
            Item.useAnimation = 14;                 // 动画持续时间
            Item.useStyle = ItemUseStyleID.Shoot;   // 使用样式为射击
            Item.noMelee = true;                    // 关闭近战攻击判定
            Item.knockBack = 1;                     // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); // 稀有度
            Item.UseSound = SoundID.Item5;          // 射击音效
            Item.autoReuse = true;                  // 自动重用
            Item.shoot = ModContent.ProjectileType<BalloonArrowProjectile>(); // 默认发射物
            Item.shootSpeed = 8f;                   // 初始弹速
            Item.useAmmo = AmmoID.Arrow;            // 弹药类型为箭类
        }

        /// <summary>
        /// 修改射击参数，将所有箭类弹药替换为气球箭
        /// </summary>
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 将所有类型的箭统一替换为气球箭
            type = ModContent.ProjectileType<BalloonArrowProjectile>();
        }

        /// <summary>
        /// 合成配方：需要1个闪亮红气球和1个金弓或铂金弓，在铁砧上合成
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShinyRedBalloon, 1)
                .AddIngredient(ItemID.GoldBow, 1)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.ShinyRedBalloon, 1)
                .AddIngredient(ItemID.PlatinumBow, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}