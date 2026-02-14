using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Localization;
using ExpansionKele.Content.Items.Placeables;
using ReLogic.Content;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 赛格光矛 - 一种近战武器
    /// 使用时会向前冲刺，冲刺时造成2倍伤害，冲刺时无敌，冲刺敌人回复生命值
    /// </summary>
    public class SigLightSpear : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        /// <summary>
        /// 设置物品的基础属性
        /// 包括伤害、使用时间、稀有度等
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("赛格光矛");
            Item.width = 40;
            Item.height = 40;
            Item.damage = ExpansionKele.ATKTool(100,125);
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LightSpearProjectile>();
            Item.shootSpeed = 5f;
        }

        /// <summary>
        /// 允许使用物品的副功能（右键）
        /// </summary>
        /// <param name="player">使用物品的玩家</param>
        /// <returns>是否允许使用副功能</returns>
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        /// <summary>
        /// 添加物品合成配方
        /// 可以使用金锭或铂金锭合成
        /// </summary>
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 5)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddIngredient(ItemID.LifeCrystal, 2)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.PlatinumBar, 5)
                .AddIngredient(ModContent.ItemType<SigwutBar>(), 3)
                .AddIngredient(ItemID.LifeCrystal, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
        }
    }

    
}