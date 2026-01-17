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
	// Providing the EquipType.Legs value here will result in TML expecting a X_Legs.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Legs)]
	public class StarLeggingsJ : StarLeggingsAbs
	{
		public int Index => 9;
		public override int LeggingsDefense => ArmorData.LeggingsDefense[Index];
		public override float MoveSpeedBonus => ArmorData.MoveSpeedBonus[Index]/100f;
		public override float SummonDamage => ArmorData.SummonDamage[Index]/100f;
		public override int MeleeCritChance => ArmorData.MeleeCritChance[Index];
		public override int RangedCritChance => ArmorData.RangedCritChance[Index];
		public override float MeleeSpeed => ArmorData.MeleeSpeed[Index]/100f;
		public override int MaxMana => ArmorData.MaxMana[Index];
		public override float ManaCostReduction => ArmorData.ManaCostReduction[Index]/100f;
		public override float AmmoCostReduction => ArmorData.AmmoCostReduction[Index];
		public override void SetDefaults()
        {
            base.SetDefaults();
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarLeggingsJ>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.LunarBar, 9);//神圣锭*7
	recipe.AddIngredient(ItemID.FragmentStardust, 3);
    recipe.AddIngredient(ModContent.ItemType<StarLeggingsI>(), 1);
    recipe.AddTile(TileID.LunarCraftingStation);
    recipe.Register(); // 注册配方  
	if(ExpansionKele.calamity!=null)
	{
	Recipe recipeI = Recipe.Create(ModContent.ItemType<StarLeggingsJ>()); 
	recipeI.AddIngredient(ExpansionKele.calamity.Find<ModItem>("LifeAlloy").Type, 8);
	recipeI.AddIngredient(ExpansionKele.calamity.Find<ModItem>("GalacticaSingularity").Type, 8);
    recipeI.AddIngredient(ItemID.LunarBar, 8);
    recipeI.AddTile(TileID.LunarCraftingStation);//远古操纵机
    recipeI.Register(); // 注册配方  
	}
	}  
	
	}
}
