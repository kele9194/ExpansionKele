using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace ExpansionKele.Content.Armor.StarArmorA
{
	// 继承自抽象基类 StarHelmetAbs
	[AutoloadEquip(EquipType.Head)]
	public class StarHelmetJ : StarHelmetAbs
	{
		public override int Index => 9; // J 对应索引 9
		public static string setNameOverride = "星元头盔J";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateJ>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsJ>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetJ>();


		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetJ>()); // 替换为 GaSniperA 的类型  
	recipe.AddIngredient(ItemID.LunarBar, 6);//神圣锭*7
	recipe.AddIngredient(ItemID.FragmentStardust, 3);
    recipe.AddIngredient(ModContent.ItemType<StarHelmetI>(), 1);
    recipe.AddTile(TileID.LunarCraftingStation);
    recipe.Register(); // 注册配方  
	if(ExpansionKele.calamity!=null)
	{
	Recipe recipeI = Recipe.Create(ModContent.ItemType<StarHelmetJ>()); 
	recipeI.AddIngredient(ExpansionKele.calamity.Find<ModItem>("LifeAlloy").Type, 8);
	recipeI.AddIngredient(ExpansionKele.calamity.Find<ModItem>("GalacticaSingularity").Type, 8);
    recipeI.AddIngredient(ItemID.LunarBar, 8);
    recipeI.AddTile(TileID.LunarCraftingStation);//远古操纵机
    recipeI.Register(); // 注册配方  
	}
	}  
            }
        }
		

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		// public override void AddRecipes() {
		// 	CreateRecipe()
		// 		.AddIngredient<ExampleItem>()
		// 		.AddTile<Tiles.Furniture.ExampleWorkbench>()
		// 		.Register();
		// }

