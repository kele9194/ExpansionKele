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
	public class StarHelmet : ModItem
	{
		public override string LocalizationCategory => "Armor.StarArmorA";
		public static readonly int helmetDefense = 20;
		public static readonly float GenericDamageBonus = 0.2f;

		public static readonly int maxTurrets = 1;

		
		public float a = 1.15f;

		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults() {
			// If your head equipment should draw hair while drawn, use one of the following:
			// ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false; // Don't draw the head at all. Used by Space Creature Mask
			// ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true; // Draw hair as if a hat was covering the top. Used by Wizards Hat
			// ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true; // Draw all hair as normal. Used by Mime Mask, Sunglasses
			// ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(GenericDamageBonus);
		}

		public override void SetDefaults() {
			//Item.SetNameOverride("星元头盔");
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = helmetDefense; // The amount of defense the item will give when equipped
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
    	return head.type == ModContent.ItemType<StarHelmet>() &&
           body.type == ModContent.ItemType<StarBreastplate>() &&
           legs.type == ModContent.ItemType<StarLeggings>();
}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		 public override void UpdateArmorSet(Player player) {
            player.setBonus = "被动血量越低伤害越高，套装奖励：禁用生命再生获得高额攻击力";
            float lifePercentage = player.statLife / (float)player.statLifeMax2;
            float damageBoost = (1 / (lifePercentage + a)) - (1 / (1 + a));
            player.GetDamage<GenericDamageClass>() += damageBoost;

            if (ExpansionKele.calamity != null)
            {
                var calamityPlayerType = player.GetModPlayer(ExpansionKele.calamity.Find<ModPlayer>("CalamityPlayer"));
                if (calamityPlayerType != null)
                {
                    // 获取 rogueStealthMax 字段
                    FieldInfo rogueStealthMaxField = calamityPlayerType.GetType().GetField("rogueStealthMax", BindingFlags.Public | BindingFlags.Instance);
                    if (rogueStealthMaxField != null)
                    {
                        // 获取当前值并增加 1f
                        float currentValue = (float)rogueStealthMaxField.GetValue(calamityPlayerType);
                        rogueStealthMaxField.SetValue(calamityPlayerType, currentValue + 1f);
                    }
                    

                    // 获取 wearingRogueArmor 字段
                    FieldInfo wearingRogueArmorField = calamityPlayerType.GetType().GetField("wearingRogueArmor", BindingFlags.Public | BindingFlags.Instance);
                    if (wearingRogueArmorField != null)
                    {
                        // 设置为 true
                        wearingRogueArmorField.SetValue(calamityPlayerType, true);
                    }
				}
				}
        }

		public override void UpdateEquip(Player player)
        {
			player.maxTurrets= maxTurrets;
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
                    { "Defense", $"[c/00FF00:防御力 +{Item.defense}]" },
				    {"GenericDamageBonus", $"[c/00FF00:伤害 +{GenericDamageBonus * 100}%]"},
				    {"maxTurrets", $"[c/00FF00:最大哨兵数量 +{maxTurrets}]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
	}
}
// ... existing code ...
		

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		// public override void AddRecipes() {
		// 	CreateRecipe()
		// 		.AddIngredient<ExampleItem>()
		// 		.AddTile<Tiles.Furniture.ExampleWorkbench>()
		// 		.Register();
		// }

