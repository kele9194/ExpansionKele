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
	public class StarHelmetG : StarHelmetAbs
	{
		public override int Index => 6; // G 对应索引 6
		public static string setNameOverride = "星元头盔G";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateG>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsG>();
		}

		protected override int ItemType => ModContent.ItemType<StarHelmetG>();
		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetG>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.HallowedBar, 6);//神圣锭*7
	recipe.AddIngredient(ItemID.ChlorophyteBar, 6);//叶绿锭*7
    recipe.AddIngredient(ModContent.ItemType<StarHelmetF>(), 1);
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

