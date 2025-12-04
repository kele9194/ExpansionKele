using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Commons;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    /// <summary>
    /// 望月回响 - 魔法书。
    /// 发射三发满月回响弹幕，方向偏移 ±4° 和 0°。
    /// 弹幕会逐渐减速，并在飞行一段时间后返回玩家身边。
    /// 返回时造成 60% 伤害并忽视 20 点防御。
    /// </summary>
    public class FullMoonEcho : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            // 可用于调试或国际化支持（已禁用）
            // DisplayName.SetDefault("Full Moon Echo");
            // Tooltip.SetDefault("发射三发减速追踪箭矢，接触玩家后消失");
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月回响");
            Item.damage = ExpansionKele.ATKTool(35,default);                    // 基础魔法伤害
            Item.DamageType = DamageClass.Magic; // 魔法伤害类型
            Item.mana = 10;                     // 每次使用消耗 10 点魔力
            Item.width = 28;                    // 物品宽度
            Item.height = 30;                   // 物品高度
            Item.useTime = 30;                  // 使用时间（帧数）
            Item.useAnimation = 30;             // 动画持续时间
            Item.useStyle = ItemUseStyleID.Shoot; // 使用样式为射击
            Item.noMelee = true;                // 禁止近战攻击判定
            Item.knockBack = 2;                 // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖出价格：5 金币
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);       // 稀有度：粉红（稀有）
            Item.UseSound = SoundID.Item21;      // 射击音效
            Item.autoReuse = true;              // 自动重用
            Item.shoot = ModContent.ProjectileType<FullMoonEchoProj>(); // 发射的弹幕类型
            Item.shootSpeed = 24f;              // 弹幕初始速度
        }

        /// <summary>
        /// 自定义射击逻辑：发射三发弹幕，方向偏移分别为 +4°, 0°, -4°。
        /// </summary>
        /// <param name="player">当前玩家</param>
        /// <param name="source">发射来源</param>
        /// <param name="position">发射位置</param>
        /// <param name="velocity">初始速度</param>
        /// <param name="type">弹幕类型</param>
        /// <param name="damage">基础伤害</param>
        /// <param name="knockback">击退值</param>
        /// <returns>是否允许原版弹幕继续发射</returns>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = -1; i <= 1; i++)
            {
                // 添加偏转角度（±3.2 * i => ±4°）
                Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(3.2f * i));
                Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            }

            return false; // 阻止原版弹幕发射
        }

        /// <summary>
        /// 添加自定义提示信息到物品描述中。
        /// </summary>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // tooltips.Add(new TooltipLine(Mod, "FullMoonSwordTooltip",
            //     "发射三发带偏斜的满月，减速前行后过一段时间返回但造成伤害变为原来60%"));
            // tooltips.Add(new TooltipLine(Mod, "FullMoonSwordTooltip1",
            //     "血月和蓝月的交替，代表死亡与寂静"));
        }

        /// <summary>
        /// 注册合成配方：需要 8 个【满月锭】+ 【咒法典】，在铁砧上合成。
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FullMoonBar>(), 8) // 核心材料
                .AddIngredient(ItemID.SpellTome)                  // 辅助材料
                .AddTile(TileID.Bookcases)                          
                .Register();
        }
    }
}