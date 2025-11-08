using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
	public class RedemptionShard : ModItem
	{
		public override string LocalizationCategory => "Items.OtherItem";

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 25;
		}

		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 9999;
			Item.value = 1000; // 设置价值
			Item.rare = Terraria.ID.ItemRarityID.Blue;
		}
	}
}