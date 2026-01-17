using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Armor.StarArmorA
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Body value here will result in TML expecting X_Arms.png, X_Body.png and X_FemaleBody.png sprite-sheet files to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Body)]
	public class StarBreastplateA : StarBreastplateAbs
{
    public static int index = 0;
    
    public override int PlateDefense => ArmorData.PlateDefense[index];
    public override int CritChance => ArmorData.CritChance[index];
    public override int MaxMinions => ArmorData.MaxMinions[index];
	public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

    public override void AddRecipes()  
    {  
        // 创建配方
        Recipe recipe = Recipe.Create(ModContent.ItemType<StarBreastplateA>()); 
        recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 24); 
        recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 12); 
        recipe.AddTile(TileID.Anvils); 
        recipe.Register(); 
    }  
}
}