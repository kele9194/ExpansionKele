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
    
    public class StarStaffD: AbsStarStaff
{
    public override string LocalizationCategory => "StaryMagic";

    protected override int damage => 68;
    protected override string setNameOverride => "星元法杖D";
    public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

        public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarStaffD>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.SoulofNight, 7); // 暗影之魂*7
    recipe.AddIngredient(ItemID.SoulofLight, 7); // 光明之魂*7
    recipe.AddIngredient(ModContent.ItemType<StarStaffC>(), 1);
	
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  


}
}





    
    
    
    


    





