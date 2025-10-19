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
	public class GaSniperG : GaSniperAbs
	{
        public override string LocalizationCategory => "StarySniper";
        // 基础属性
        public override int BaseDamage => 463;
        public override float KnockBack => 8f;
        public override float ShootSpeed => 16f;
        public override int UseTime => 70;
        public override int Crit => 15;
        public override int Rarity => 7;
        public override string ItemName => "SG星元狙击步枪-G";

        public override string introduction => "该狙击步枪F型号升级版,使用火枪子弹,钨/银子弹时,会转化为高速子弹。";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperG>());
            recipe.AddIngredient(ItemID.ShroomiteBar, 3);
            recipe.AddIngredient(ModContent.ItemType<GaSniperF>(), 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

    }
}