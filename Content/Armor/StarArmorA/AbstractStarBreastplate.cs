// using Terraria;
// using Terraria.ID;
// using Terraria.Localization;
// using Terraria.ModLoader;
// using ExpansionKele.Content.Buff;
// using System.Collections.Generic;

// namespace ExpansionKele.Content.Armor.StarArmorA
// {
// 	/// <summary>
// 	/// 星元系列胸甲的抽象基类
// 	/// </summary>
// 	[AutoloadEquip(EquipType.Body)]
// 	public abstract class AbstractStarBreastplate : ModItem
// 	{
// 		/// <summary>
// 		/// 胸甲索引，用于从ArmorData中获取属性值
// 		/// </summary>
// 		public abstract int Index { get; }

// 		/// <summary>
// 		/// 胸甲防御力
// 		/// </summary>
// 		public virtual int PlateDefense => ArmorData.PlateDefense[Index];

// 		/// <summary>
// 		/// 暴击率加成
// 		/// </summary>
// 		public virtual int CritChance => ArmorData.CritChance[Index];

// 		/// <summary>
// 		/// 最大召唤物数量加成
// 		/// </summary>
// 		public virtual int MaxMinions => ArmorData.MaxMinions[Index];

// 		/// <summary>
// 		/// 胸甲名称本地化键
// 		/// </summary>
// 		public abstract string NameLocalizationKey { get; }

// 		public override void SetDefaults()
// 		{
// 			Item.SetNameOverride(Language.GetTextValue(NameLocalizationKey));
// 			Item.width = 18;
// 			Item.height = 18;
// 			Item.value = Item.sellPrice(gold: 1);
// 			Item.rare = ItemRarityID.Green;
// 			Item.defense = PlateDefense;
// 		}

// 		public override void UpdateEquip(Player player)
// 		{
// 			player.buffImmune[BuffID.OnFire] = true;
// 			player.maxMinions += MaxMinions;
// 			player.noKnockback = true;
// 			player.GetCritChance(DamageClass.Generic) += CritChance;
// 		}

// 		public override void ModifyTooltips(List<TooltipLine> tooltips)
// 		{
// 			var tooltipData = new Dictionary<string, string>
// 			{
// 				{"critChance", Language.GetTextValue("Mods.ExpansionKele.Items.AbstractStarBreastplate.CritChance", CritChance)},
// 				{ "MaxMinions", Language.GetTextValue("Mods.ExpansionKele.Items.AbstractStarBreastplate.MaxMinions", MaxMinions) },
// 				{ "FireImmunity", Language.GetTextValue("Mods.ExpansionKele.Items.AbstractStarBreastplate.FireImmunity") },
// 				{"kbBuff", Language.GetTextValue("Mods.ExpansionKele.Items.AbstractStarBreastplate.KnockbackImmunity")}
// 			};

// 			foreach (var kvp in tooltipData)
// 			{
// 				tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
// 			}
// 		}
// 	}
// }