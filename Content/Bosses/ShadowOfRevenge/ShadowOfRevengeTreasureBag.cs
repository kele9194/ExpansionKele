using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.OtherItem;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Bosses.ShadowOfRevenge
{
	public class ShadowOfRevengeTreasureBag : ModItem
	{
        public override string LocalizationCategory => "Bosses.ShadowOfRevenge";
		public override void SetStaticDefaults() {
			ItemID.Sets.BossBag[Type] = true;
		}

		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.Purple;
			Item.expert = true;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			// 添加70金币
			itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 70, 70));

			// 添加30-40个FullMoonBar
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<FullMoonBar>(), 1, 30, 40));

			// 添加10-20瓶治疗药水 (原版物品)
			itemLoot.Add(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 10, 20));
		}
	}
}