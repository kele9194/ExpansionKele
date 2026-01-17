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
	public class StarBreastplateD : StarBreastplateAbs
	{
		public static int index = 3;
		
		public override int PlateDefense => ArmorData.PlateDefense[index];
		public override int CritChance => ArmorData.CritChance[index];
		public override int MaxMinions => ArmorData.MaxMinions[index];
		public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

		//Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarBreastplateD>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:PrimaryBars", 12); // 添加任意金锭组，要求7个  
    recipe.AddIngredient(ItemID.SoulofNight, 6); // 暗影之魂*7
    recipe.AddIngredient(ItemID.SoulofLight, 6); // 光明之魂*7
	recipe.AddIngredient(ModContent.ItemType<StarBreastplateC>(), 1); 
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
    recipe.Register(); // 注册配方  
	}  
	}
}
