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
    
    public class StarStaffE : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
    protected override int damage => 90;
    protected override string setNameOverride => "星元法杖E";
    public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }
    
        public override void AddRecipes()  
	{  
        Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffE>()); // 替换为 GaSniperD 的类型  
	recipe.AddRecipeGroup("ExpansionKele:SecondaryBars", 3); 
	recipe.AddRecipeGroup("ExpansionKele:TertiaryBars", 3);
	recipe.AddIngredient(ModContent.ItemType<StarStaffD>(), 1);
    recipe.AddTile(TileID.MythrilAnvil);
	//recipe.AddTile(OrichalcumAnvil); // 使用秘银/山铜砧  
     

    Recipe recipeI = Recipe.Create(ModContent.ItemType<StarStaffE>()); // 替换为 GaSniperD 的类型   
	recipeI.AddRecipeGroup("ExpansionKele:SecondaryBars", 4);
	recipeI.AddIngredient(ModContent.ItemType<StarStaffD>(), 1);
    recipeI.AddTile(TileID.MythrilAnvil);
    if(ExpansionKele.calamity!=null){
        recipeI.Register(); // 注册配方 
    }
    else{
        recipe.Register();
    }
	}  


}
}





    
    
    
    


    





