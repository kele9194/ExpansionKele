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
	public class StarHelmetE : StarHelmetAbs
	{
		public override int Index => 4; // E 对应索引 4
		public static string setNameOverride = "星元头盔E";

		protected override int GetBodyType()
		{
			return ModContent.ItemType<StarBreastplateE>();
		}

		protected override int GetLegsType()
		{
			return ModContent.ItemType<StarLeggingsE>();
		}
		protected override int ItemType => ModContent.ItemType<StarHelmetE>();

		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetE>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:SecondaryBars", 6); // 添加任意金锭组，要求7个 
	//recipe.AddRecipeGroup("ExpansionKele:AnySoul", 1); 
    recipe.AddIngredient(ModContent.ItemType<StarHelmetD>(), 1);
    recipe.AddTile(TileID.MythrilAnvil);
    recipe.Register(); // 注册配方  
	}  
            }
        }