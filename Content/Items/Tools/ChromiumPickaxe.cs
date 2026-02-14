using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Tools
{
    /// <summary>
    /// 铬钢镐 - 强大的采矿工具。
    /// 具有高镐力和稳定的性能，攻击时产生金属碎片特效。
    /// </summary>
    public class ChromiumPickaxe : ModItem
    {
        public override string LocalizationCategory => "Items.Tools";
        
        public override void SetStaticDefaults()
        {
            // 可选：国际化支持
            // DisplayName.SetDefault("Chromium Pickaxe");
            // Tooltip.SetDefault("强大的采矿工具，具有高镐力");
        }

        public override void SetDefaults()
        {
            // Item.SetNameOverride("铬钢镐");
            Item.damage = 20;                    // 基础伤害值
            Item.DamageType = DamageClass.Melee;  // 近战伤害类型
            Item.width = 32;                     // 物品宽度
            Item.height = 32;                    // 物品高度
            Item.useTime = 11;                   // 使用时间（tick），对应攻速15
            Item.useAnimation = 19;              // 动画持续时间
            Item.useStyle = ItemUseStyleID.Swing; // 挥舞动作
            Item.knockBack = 4;                  // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);         // 稀有度
            Item.UseSound = SoundID.Item1;       // 使用音效
            Item.autoReuse = true;               // 自动重复使用
            Item.attackSpeedOnlyAffectsWeaponAnimation = true; // 攻速只影响动画速度

            Item.pick = 125;    // 镐力125
        }

        /// <summary>
        /// 攻击时有一定概率生成金属碎片特效。
        /// </summary>
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3)) // 每帧有约33%概率生成特效
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
        /// 需要 12 个【铬钢锭】，在地狱熔炉上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ChromiumBar>(), 12)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}