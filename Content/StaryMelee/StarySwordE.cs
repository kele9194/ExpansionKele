using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;


namespace ExpansionKele.Content.StaryMelee
{
    public class StarySwordE : StarySwordAbs
    {
        
        public override string LocalizationCategory => "StaryMelee";
        public override string setNameOverride => "星元剑E";
        private const string introduction = "星元剑D的升级版，近战可造成两次伤害，第二次拥有恐怖的二次乘伤和短时增益，右键允许突进但无法连续使用，突进损失一定蓝量但给予无敌帧";

        // 重写基础属性
        public override int BaseDamage => 24;
        public override int UseTime => 21;
        public override int Rarity => ItemRarityID.LightRed;
        public override int Crit => 7;
        public override bool UseTurn => false;

        public override void SetStaticDefaults()
        {
            //ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
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
            Recipe recipe = Recipe.Create(ModContent.ItemType<StarySwordE>()); // 替换为 GaSniperD 的类型  
            recipe.AddRecipeGroup("ExpansionKele:SecondaryBars", 6);
            recipe.AddRecipeGroup("ExpansionKele:TertiaryBars", 6);
            recipe.AddIngredient(ModContent.ItemType<StarySwordD>(), 1);
            recipe.AddTile(TileID.MythrilAnvil);
            //recipe.AddTile(OrichalcumAnvil); // 使用秘银/山铜砧  

            Recipe recipeI = Recipe.Create(ModContent.ItemType<StarySwordE>()); // 替换为 GaSniperD 的类型   
            recipeI.AddRecipeGroup("ExpansionKele:SecondaryBars", 8);
            recipeI.AddIngredient(ModContent.ItemType<StarySwordD>(), 1);
            recipeI.AddTile(TileID.MythrilAnvil);
            if (ExpansionKele.calamity != null)
            {
                recipeI.Register(); // 注册配方 
            }
            else
            {
                recipe.Register();
            }
        }
    }
}