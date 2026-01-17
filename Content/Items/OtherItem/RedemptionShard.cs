using ExpansionKele.Content.Customs;
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
			Item.value = ItemUtils.CalculateValueFromRecipes(this,1.0f,200);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
		}
	}
}