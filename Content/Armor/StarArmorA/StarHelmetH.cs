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
	public class StarHelmetH : StarHelmetAbs
	{
		public override int Index => 7; // H 对应索引 7
		public static string setNameOverride = "星元头盔H";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateH>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsH>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetH>();
		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetH>()); // 替换为 GaSniperA 的类型  
	recipe.AddIngredient(ItemID.BeetleHusk, 4);//
    recipe.AddIngredient(ModContent.ItemType<StarHelmetG>(), 1);
    recipe.AddTile(TileID.MythrilAnvil);
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

