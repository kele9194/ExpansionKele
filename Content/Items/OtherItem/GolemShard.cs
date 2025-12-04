using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.OtherItem
{
	public class GolemShard : ModItem
	{
		public override string LocalizationCategory => "Items.OtherItem";

		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Golem Shard");
			// Tooltip.SetDefault("'The essence of ancient stone magic'");
		}

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 26;
			Item.maxStack = 9999;
			Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
		}
	}

	public class GolemShardDrop : GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			if (npc.type == NPCID.Golem) {
				// 添加石巨人碎片掉落，数量为100-150个，概率100%
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GolemShard>(), 1, 100, 150));
			}
		}
	}
}