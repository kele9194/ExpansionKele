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
    
    public class StarStaffH : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
    protected override int damage => 168;
    protected override string setNameOverride => "星元法杖H";
        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffH>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.Razorpine, 1);//神圣锭*7
    recipe.AddIngredient(ModContent.ItemType<StarStaffG>(), 1);
	
    recipe.AddTile(TileID.MythrilAnvil); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}





    
    
    
    


    





