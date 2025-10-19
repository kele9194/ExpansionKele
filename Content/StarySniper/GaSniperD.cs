using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using System.Collections.Generic;

namespace ExpansionKele.Content.StarySniper
{
    public class GaSniperD : GaSniperAbs
    {
        public override string LocalizationCategory => "StarySniper";
       
        // 基础属性
        public override int BaseDamage => 177;
        public override float KnockBack => 8f;
        public override float ShootSpeed => 16f;
        public override int UseTime => 77;
        public override int Crit => 10;
        public override int Rarity => 4;
        public override string ItemName => "SG星元狙击步枪-D";

        public override string introduction => "该狙击步枪C型号升级版,使用火枪子弹,钨/银子弹时,会转化为高速子弹。";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
        }

        public override void AddRecipes()
        {
            // 创建 GaSniperD 武器的合成配方  
            Recipe recipe = Recipe.Create(ModContent.ItemType<GaSniperD>()); // 替换为 GaSniperD 的类型  
            recipe.AddIngredient(ModContent.ItemType<GaSniperC>(), 1);
            recipe.AddIngredient(ItemID.SoulofNight, 7); // 暗影之魂*7
            recipe.AddIngredient(ItemID.SoulofLight, 7); // 光明之魂*7
            recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
            recipe.Register(); // 注册配方  
        }

        
    }
}