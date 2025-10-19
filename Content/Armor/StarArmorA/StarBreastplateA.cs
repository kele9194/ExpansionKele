using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;

namespace ExpansionKele.Content.Armor.StarArmorA
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Body value here will result in TML expecting X_Arms.png, X_Body.png and X_FemaleBody.png sprite-sheet files to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Body)]
	public class StarBreastplateA : ModItem
	{
		public override string LocalizationCategory => "Armor.StarArmorA";
		public static int index = 0;
		// 防御值
		public static int plateDefense = ArmorData.PlateDefense[index];
		// 暴击率加成
		public static int critChance = ArmorData.CritChance[index];
		// 最大召唤物数量加成
		public static int MaxMinions = ArmorData.MaxMinions[index];

		// 使用本地化系统处理物品名称和工具提示
		public override LocalizedText DisplayName => base.DisplayName.WithFormatArgs();
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(critChance, MaxMinions);

		// 设置物品默认属性
		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = plateDefense; // The amount of defense the item will give when equipped
		}
		

		// 装备时的效果
		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.OnFire] = true; // Make the player immune to Fire
			player.maxMinions += MaxMinions; // Increase how many minions the player can have by one
			player.noKnockback = true; // Increase knockback resistance
			player.GetCritChance(DamageClass.Generic) += critChance;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"critChance", $"[c/00FF00:暴击率 +{critChance}%]"},
                    { "MaxMinions", $"[c/00FF00:最大召唤物数量 +{MaxMinions}]"},
                    { "FireImmunity", $"[c/00FF00:免疫火焰伤害,免疫击退]"},
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }

		//Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes()  
	    {  
            // 创建配方
            Recipe recipe = Recipe.Create(ModContent.ItemType<StarBreastplateA>()); 
            recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 24); 
            recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 12); 
            recipe.AddTile(TileID.Anvils); 
            recipe.Register(); 
	    }  
	}
}