using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;

namespace ExpansionKele.Content.StarySniper
{
	public class GaSniperJ : GaSniperAbs
	{
        public override string LocalizationCategory => "StarySniper";
        // 基础属性
        public override int BaseDamage => 850;
        public override float KnockBack => 8f;
        public override float ShootSpeed => 16f;
        public override int UseTime => 38;
        public override int Crit => 20;
        public override int Rarity => 11;
        public override string ItemName => "SG星元狙击步枪-J";

        public override string introduction => "该狙击步枪I型号升级版,是月球领主后最终版本，使用火枪子弹,钨/银子弹时,会转化为高速子弹。";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperJ>());
            recipe.AddIngredient(ItemID.LunarBar, 7);
            recipe.AddIngredient(ModContent.ItemType<GaSniperI>(), 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }

    }
}