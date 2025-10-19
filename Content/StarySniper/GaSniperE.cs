using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using System.Configuration;

namespace ExpansionKele.Content.StarySniper
{
	public class GaSniperE : GaSniperAbs
	{
        public override string LocalizationCategory => "StarySniper";
        // 基础属性
        public override int BaseDamage => 210;
        public override float KnockBack => 8f;
        public override float ShootSpeed => 16f;
        public override int UseTime => 77;
        public override int Crit => 10;
        public override int Rarity => 5;
        public override string ItemName => "SG星元狙击步枪-E";

        public override string introduction => "该狙击步枪D型号升级版,使用火枪子弹,钨/银子弹时,会转化为高速子弹。";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperE>());
            recipe.AddRecipeGroup("ExpansionKele:SecondaryBars", 7);
            recipe.AddRecipeGroup("ExpansionKele:TertiaryBars", 7);
            recipe.AddIngredient(ModContent.ItemType<GaSniperD>(), 1);
            recipe.AddTile(TileID.MythrilAnvil);
            
            Recipe recipeI = Recipe.Create(ModContent.ItemType<GaSniperE>());
            recipeI.AddRecipeGroup("ExpansionKele:SecondaryBars", 7);
            recipeI.AddIngredient(ModContent.ItemType<GaSniperD>(), 1);
            recipeI.AddTile(TileID.MythrilAnvil);
            
            if(ExpansionKele.calamity!=null){
                recipeI.Register();
            }
            else{
                recipe.Register();
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = new TooltipLine(Mod, "GaSniperETooltip", introduction);
            tooltips.Add(line);
        }
    }
}