using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Threading;
using System.IO;



namespace ExpansionKele.Content.Armor.StarArmorA
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Head value here will result in TML expecting a X_Head.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Head)]
	public class StarHelmetA : ModItem
	{
		public override string LocalizationCategory => "Armor.StarArmorA";
		public static int index = 0;
		public static  int helmetDefense = ArmorData.HelmetDefense[index];
		public static  float GenericDamageBonus = ArmorData.GenericDamageBonus[index]/100f;

		public static  int maxTurrets = ArmorData.MaxTurrets[index];

		public static float rogueStealthMax = ArmorData.StealthMax[index]/100f;

		public static int RogueCritChance = ArmorData.RogueCritChance[index];

		
		public float a = ArmorData.CalculateA(0.34f+0.3f*ArmorData.GenericDamageBonus[index]/100f);
		public static string setNameOverride="星元头盔A";
		public static LocalizedText SetBonusText { get; private set; }
		public static LocalizedText SetBonusTextWithCalamity { get; private set; }



		public override void SetStaticDefaults() {


			SetBonusText = this.GetLocalization("SetBonus");
			SetBonusTextWithCalamity = this.GetLocalization("SetBonusWithCalamity").WithFormatArgs(RogueCritChance, rogueStealthMax*100);
		}
		

		public override void SetDefaults() {
			//Item.SetNameOverride(setNameOverride);
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = helmetDefense; // The amount of defense the item will give when equipped
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
    	return head.type == ModContent.ItemType<StarHelmetA>() &&
           body.type == ModContent.ItemType<StarBreastplateA>() &&
           legs.type == ModContent.ItemType<StarLeggingsA>();
}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		 public override void UpdateArmorSet(Player player) {
            player.setBonus = SetBonusText.Value;
            if(ExpansionKele.calamity!=null)
			{
				 player.setBonus = SetBonusTextWithCalamity.Value;
				player.GetCritChance<ThrowingDamageClass>() +=RogueCritChance;
            	ReflectionHelper.ApplyRogueStealth(player, rogueStealthMax);
			}
			float lifePercentage = player.statLife / (float)player.statLifeMax2;
			if(lifePercentage > 1)
			{
				lifePercentage = 1;
			}
            float damageBoost = (1 / (lifePercentage + a)) - (1 / (1 + a));
            player.GetDamage<GenericDamageClass>() += damageBoost;
			
        }

		public override void UpdateEquip(Player player)
        {
			player.maxTurrets+= maxTurrets;
            player.GetDamage(DamageClass.Generic) += GenericDamageBonus;
            // 检查是否装备了整套星元盔甲
            
              
    }


	// ... existing code ...
	public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"GenericDamageBonus", $"[c/00FF00:伤害 +{GenericDamageBonus*100}%]"},
                    {"maxTurrets", $"[c/00FF00:最大哨兵数量 +{maxTurrets}]"},
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...

		public override void AddRecipes()  
	{  
    // 创建 GaSniperA 武器的合成配方  
    Recipe recipe = Recipe.Create(ModContent.ItemType<StarHelmetA>()); // 替换为 GaSniperA 的类型  
    recipe.AddRecipeGroup("ExpansionKele:BeforeSecondaryBars", 12); // 添加任意金锭组，要求7个  
    recipe.AddRecipeGroup("ExpansionKele:BeforeTertiaryBars", 6); // 添加任意银锭组，要求7个  
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

