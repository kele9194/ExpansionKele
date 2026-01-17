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
	public class StarHelmetI : StarHelmetAbs
	{
		public override int Index => 8; // I 对应索引 8
		public static string setNameOverride = "星元头盔I";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateI>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsI>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetI>();

		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetI>()); // 替换为 GaSniperA 的类型  
	recipe.AddIngredient(ItemID.FragmentSolar, 3);//叶绿锭*7
	recipe.AddIngredient(ItemID.FragmentVortex, 3);
	recipe.AddIngredient(ItemID.FragmentNebula, 3);
	//recipe.AddIngredient(ItemID.FragmentStardust, 8);
    recipe.AddIngredient(ModContent.ItemType<StarHelmetH>(), 1);
    recipe.AddTile(TileID.LunarCraftingStation);
    recipe.Register(); // 注册配方  
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

