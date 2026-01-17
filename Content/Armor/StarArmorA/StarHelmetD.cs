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
	public class StarHelmetD : StarHelmetAbs
	{
		public override int Index => 3; // D 对应索引 3
		public static string setNameOverride = "星元头盔D";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateD>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsD>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetD>();

		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetD>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:PrimaryBars", 6); // 添加任意金锭组，要求7个  
    recipe.AddIngredient(ItemID.SoulofNight, 3); // 暗影之魂*7
    recipe.AddIngredient(ItemID.SoulofLight, 3); // 光明之魂*7
	recipe.AddIngredient(ModContent.ItemType<StarHelmetC>(), 1); 
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

