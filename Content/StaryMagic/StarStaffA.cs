using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.StaryMagic
{
    
    public class StarStaffA : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
        protected override int damage => 16;
        protected override string setNameOverride => "星元法杖A";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }


        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffA>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 4); // 添加任意金锭组，要求7个  
    recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 3); // 添加任意银锭组，要求7个  
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}
