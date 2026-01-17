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
	public class StarHelmetC : StarHelmetAbs
	{
		public override int Index => 2; // C 对应索引 2
		public static string setNameOverride = "星元头盔C";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateC>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsC>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetC>();

		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetC>()); // 替换为 GaSniperA 的类型  
    recipe.AddIngredient(ItemID.HellstoneBar, 6);
	recipe.AddIngredient(ItemID.Bone, 3);
	recipe.AddIngredient(ModContent.ItemType<StarHelmetB>(), 1); 
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

