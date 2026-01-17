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
	public class StarHelmetB : StarHelmetAbs
	{
		public override int Index => 1; // B 对应索引 1
		public static string setNameOverride = "星元头盔B";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateB>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsB>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetB>();
		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetB>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:AnyDemoniteBar", 6); // 添加任意魔锭组，要求7个
	recipe.AddRecipeGroup("ExpansionKele:AnyShadowScale", 3);//添加任意暗影文化组，要求7个
    recipe.AddIngredient(ModContent.ItemType<StarHelmetA>(), 1); 
    recipe.AddTile(TileID.Anvils); // 使用铁铅砧  
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

