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
    
    public class StarStaffJ : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
    protected override int damage => 360;
    protected override string setNameOverride => "星元法杖J";
        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffJ>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.LunarBar, 2);//神圣锭*7
    recipe.AddIngredient(ModContent.ItemType<StarStaffI>(), 1);
	
    recipe.AddTile(TileID.LunarCraftingStation); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}





    
    
    
    


    





