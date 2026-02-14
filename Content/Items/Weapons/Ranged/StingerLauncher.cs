using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    /// <summary>
    /// 毒针发射器 - 一种远程武器
    /// 散射发射多根钢针，具有盔甲穿透和毒性效果
    /// </summary>
    public class StingerLauncher : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        // 弹幕数量
        const int numProjectiles = 27;
        // 散射角度
        const float spreadAngle = 10f;
        // 盔甲穿透值
        public static readonly int ArmorPenetration = 20;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }
        
        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("毒针发射器");
            Item.width = 40;
            Item.height = 22;
            Item.damage = ExpansionKele.ATKTool(5,6);
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<StingerProjectile>();
            Item.shootSpeed = 10f;
        }

        /// <summary>
        /// 发射弹幕
        /// 散射发射多根钢针
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <param name="source">物品使用源信息</param>
        /// <param name="position">发射位置</param>
        /// <param name="velocity">发射速度</param>
        /// <param name="type">弹幕类型</param>
        /// <param name="damage">伤害值</param>
        /// <param name="knockback">击退值</param>
        /// <returns>是否使用默认的发射行为</returns>
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 发射27根钢针，在±7.5度范围内散射

            for (int i = 0; i < numProjectiles; i++)
            {
                // 计算随机角度偏移
                float angleOffset = MathHelper.ToRadians(spreadAngle) * (Main.rand.NextFloat() - 0.5f) * 2f;
                
                // 基于原始速度创建新的速度向量，并应用角度偏移
                Vector2 newVelocity = velocity.RotatedBy(angleOffset);
                
                // 随机速度变化在90%-110%之间
                float speedMultiplier = Main.rand.NextFloat(0.9f, 1.1f);
                newVelocity *= speedMultiplier;
                
                // 创建钢针弹药
                Projectile.NewProjectile(source, position, newVelocity, ModContent.ProjectileType<StingerProjectile>(), damage, knockback, player.whoAmI);
            }

            return false; // 阻止默认弹药生成
        }
        
        /// <summary>
        /// 修改物品的提示信息
        /// 添加关于武器特殊功能的说明
        /// </summary>
        /// <param name="tooltips">提示信息列表</param>
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // var tooltipData = new Dictionary<string, string>
            // {
            //     {"GenericDamageBonus", Language.GetText("Mods.ExpansionKele.Items.StingerLauncher.GenericDamageBonus").Format(numProjectiles, ArmorPenetration)},
            //     {"S", Language.GetText("Mods.ExpansionKele.Items.StingerLauncher.S").Value}
            // };

            // foreach (var kvp in tooltipData)
            // {
            //     tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            // }
        }

        /// <summary>
        /// 添加物品合成配方
        /// 需要毒刺和骨头在铁砧上合成
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Stinger, 5)
                .AddIngredient(ItemID.Bone, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}