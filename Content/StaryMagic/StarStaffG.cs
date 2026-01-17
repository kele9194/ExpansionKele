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
    
    public class StarStaffG : AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";
    protected override int damage => 158;
    protected override string setNameOverride => "星元法杖G";
    public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }
    
        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffG>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.SpectreBar, 4);//神圣锭*7
    recipe.AddIngredient(ModContent.ItemType<StarStaffF>(), 1);
	
    recipe.AddTile(TileID.MythrilAnvil); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}
