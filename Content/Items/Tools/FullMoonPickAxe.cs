using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Tools
{
    
    /// <summary>
    /// 望月镐斧 - 兼具强大斧力与镐力的近战工具。
    /// 可用于砍伐和挖掘，攻击时释放红色星光特效。
    /// </summary>
    public class FullMoonPickAxe : ModItem
    {
        public override string LocalizationCategory => "Items.Tools";
        public override void SetStaticDefaults()
        {
            // 可选：国际化支持
            // DisplayName.SetDefault("Full Moon PickAxe");
            // Tooltip.SetDefault("兼具强大斧力与镐力的魔法武器");
        }

        public override void SetDefaults()
        {
            Item.SetNameOverride("望月镐斧");
            Item.damage = 32;                    // 基础伤害值
            Item.DamageType = DamageClass.Melee;  // 近战伤害类型
            Item.width = 40;                     // 物品宽度
            Item.height = 40;                    // 物品高度
            Item.useTime = 13;                   // 使用时间（tick）
            Item.useAnimation = 13;              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing; // 挥舞动作
            Item.knockBack = 6;                 // 击退值
            Item.value = Item.sellPrice(gold: 8); // 卖出价格：8金币
            Item.rare = ItemRarityID.Pink;       // 粉色稀有度
            Item.UseSound = SoundID.Item1;       // 使用音效
            Item.autoReuse = true;               // 自动重复使用
            Item.attackSpeedOnlyAffectsWeaponAnimation = true; // 攻速只影响动画速度

            Item.axe = 22;      // 斧力
            Item.pick = 180;   // 镐力（显示为 180）
        }

        /// <summary>
        /// 攻击时有一定概率生成红色星尘特效。
        /// </summary>
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(5)) // 每帧有 20% 概率生成特效
            {
                Dust.NewDust(
                    new Vector2(hitbox.X, hitbox.Y),
                    hitbox.Width,
                    hitbox.Height,
                    DustID.RedTorch,
                    player.velocity.X * 0.2f,
                    player.velocity.Y * 0.2f,
                    Scale: 1.2f
                );
            }
        }

        /// <summary>
        /// 注册合成配方：
        /// 需要 15 个【满月锭】，在铁砧上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FullMoonBar>(), 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}