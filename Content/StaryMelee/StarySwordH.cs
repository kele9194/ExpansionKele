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
    public class StarySwordH : StarySwordAbs
    {
        public override string LocalizationCategory => "StaryMelee";
        public override string setNameOverride =>"星元剑H";
        private const string introduction ="星元剑G的升级版";

        // 重写基础属性
        public override int BaseDamage => 66;
        public override int UseTime => 20;
        public override int Rarity => ItemRarityID.Lime;
        public override int Crit => 11;
        public override bool UseTurn => false;
        public override int ShootSpeed => 10;
        public override float KnockBack => 8f;

        // 重写增益属性
        protected override int BoostTime => 25;
        protected override int DefenseBoost => 20;
        protected override int LifeRegenBoost => 6 * 20;
        protected override float SpeedBoost => 0.18f;
        protected override float EnduranceBoost => 0.11f;
        public override int WingTimeBoost => 3;
        
        // 重写冲刺属性
        protected override float DashSpeed => 30f;
        protected override int DashDuration => 10;
        protected override int DashIFrames => 35;
        protected override float ManaCostFactor => 0.33f;
        protected override float DefaultGravity => 0.4f;

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
            Recipe recipe = Recipe.Create(ModContent.ItemType<StarySwordH>()); 
            recipe.AddIngredient(ItemID.BeetleHusk, 6);
            recipe.AddIngredient(ModContent.ItemType<StarySwordG>(), 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}