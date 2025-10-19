using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Buff;

namespace ExpansionKele.Content.StaryMagic
{
    
    public class StarStaffB : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
        protected override int damage => 18;
        protected override string setNameOverride => "星元法杖B";

        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffB>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:AnyDemoniteBar", 3); // 添加任意魔锭组，要求7个
	recipe.AddRecipeGroup("ExpansionKele:AnyShadowScale", 3);//添加任意暗影文化组，要求7个
	recipe.AddIngredient(ModContent.ItemType<StarStaffA>(), 1);
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}





    
    
    
    


    





