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
    
    public class StarStaffI : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
    protected override int damage => 284;
    protected override string setNameOverride => "星元法杖I";
        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffI>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.FragmentNebula, 2);//神圣锭*7
    recipe.AddIngredient(ModContent.ItemType<StarStaffH>(), 1);
	
    recipe.AddTile(TileID.LunarCraftingStation); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}





    
    
    
    


    





