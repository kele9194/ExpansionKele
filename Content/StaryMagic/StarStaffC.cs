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

    public class StarStaffC : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
    protected override int damage => 30;
    protected override string setNameOverride => "星元法杖C";

        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffC>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.HellstoneBar, 8);
	recipe.AddIngredient(ItemID.Bone, 6);
    recipe.AddIngredient(ModContent.ItemType<StarStaffB>(), 1);
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}





    
    
    
    


    





