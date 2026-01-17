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
	public class StarBreastplateG : StarBreastplateAbs
	{
		public static int index = 6;
		
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
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarBreastplateG>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.HallowedBar, 12);//神圣锭*7
	recipe.AddIngredient(ItemID.ChlorophyteBar, 12);//叶绿锭*7
    recipe.AddIngredient(ModContent.ItemType<StarBreastplateF>(), 1);
	recipe.AddTile(TileID.MythrilAnvil);
    recipe.Register(); // 注册配方  
	}  
	}
}