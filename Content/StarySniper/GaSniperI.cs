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
	public class GaSniperI : GaSniperAbs
	{
        public override string LocalizationCategory => "StarySniper";
        // 基础属性
        public override int BaseDamage => 700;
        public override float KnockBack => 8f;
        public override float ShootSpeed => 16f;
        public override int UseTime => 44;
        public override int Crit => 20;
        public override int Rarity => 9;
        public override string ItemName => "SG星元狙击步枪-I";

        public override string introduction => "该狙击步枪H型号升级版,使用火枪子弹,钨/银子弹时,会转化为高速子弹。";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperI>());
            recipe.AddIngredient(ItemID.FragmentVortex, 7);
            recipe.AddIngredient(ModContent.ItemType<GaSniperH>(), 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }

    }
}