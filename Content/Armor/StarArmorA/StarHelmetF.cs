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
	public class StarHelmetF : StarHelmetAbs
	{
		public override int Index => 5; // F 对应索引 5
		public static string setNameOverride = "星元头盔F";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateF>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsF>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetF>();

		public override void AddRecipes()  
	{  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetF>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:TertiaryBars", 6); // 添加任意金锭组，要求7个  
	recipe.AddIngredient(ItemID.SoulofSight,1);
    recipe.AddIngredient(ModContent.ItemType<StarHelmetE>(), 1);
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

