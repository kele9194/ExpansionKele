using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;


namespace ExpansionKele.Content.StaryMelee
{
    public class StarySwordF : StarySwordAbs
    {
        public override string LocalizationCategory => "StaryMelee";
        public override string setNameOverride =>"星元剑F";
        private const string introduction ="星元剑E的升级版";

        // 重写基础属性
        public override int BaseDamage => 30;
        public override int UseTime => 20;
        public override int Rarity => ItemRarityID.Pink;
        public override int Crit => 7;
        public override bool UseTurn => false;
        public override int ShootSpeed => 10;


        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 添加自定义的 tooltip  
            // TooltipLine line = new TooltipLine(Mod, setNameOverride, introduction);
            // tooltips.Add(line);
        }

        public override void AddRecipes()
        {
            // 创建 GaSniperA 武器的合成配方  
            Recipe recipe = Recipe.Create(ModContent.ItemType<StarySwordF>()); // 替换为 GaSniperD 的类型  
            recipe.AddIngredient(ItemID.HallowedBar, 6);//神圣锭*7
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);//叶绿锭*7
            recipe.AddIngredient(ModContent.ItemType<StarySwordE>(), 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}