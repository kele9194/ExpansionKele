using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Tools
{
    
    /// <summary>
    /// 铬钢锤斧 - 结合锤子和斧头功能的强大工具。
    /// 具有高伤害和优秀的工具性能，攻击时产生金属火花特效。
    /// </summary>
    public class ChromiumHamaxe : ModItem
    {
        public override string LocalizationCategory => "Items.Tools";
        public override void SetStaticDefaults()
        {
            // 可选：国际化支持
            // DisplayName.SetDefault("Chromium Hamaxe");
            // Tooltip.SetDefault("结合锤子和斧头功能的强大工具");
        }

        public override void SetDefaults()
        {
            // Item.SetNameOverride("铬钢锤斧");
            Item.damage = 35;                    // 基础伤害值
            Item.DamageType = DamageClass.Melee;  // 近战伤害类型
            Item.width = 40;                     // 物品宽度
            Item.height = 40;                    // 物品高度
            Item.useTime = 12;                   // 使用时间（tick）
            Item.useAnimation = 22;              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing; // 挥舞动作
            Item.knockBack = 7;                  // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);         // 稀有度
            Item.UseSound = SoundID.Item1;       // 使用音效
            Item.autoReuse = true;               // 自动重复使用
            Item.attackSpeedOnlyAffectsWeaponAnimation = true; // 攻速只影响动画速度

            Item.hammer = 75;    // 锤力
            Item.axe = 35;       // 斧力
        }

        /// <summary>
        /// 攻击时有一定概率生成金属火花特效。
        /// </summary>dd
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(4)) // 每帧有 25% 概率生成特效
            {
                Dust.NewDust(
                    new Vector2(hitbox.X, hitbox.Y),
                    hitbox.Width,
                    hitbox.Height,
                    DustID.Ash,
                    player.velocity.X * 0.2f,
                    player.velocity.Y * 0.2f,
                    Scale: 1.0f
                );
            }
        }

        /// <summary>
        /// 注册合成配方：
        /// 需要 15 个【铬钢锭】，在地狱熔炉上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ChromiumBar>(), 15)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}