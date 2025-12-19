using ExpansionKele.Commons;
using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Accessories.Wings
{
	[AutoloadEquip(EquipType.Wings)]
    
	public class LifeHealingWings : ModItem
	{
        public override string LocalizationCategory => "Items.Accessories.Wings";
        public float healAmountPlayer = 0.03f;
        public float healAmountTeammate = 0.015f;

        public int HealInterval = 250;
        public override void SetStaticDefaults() {
			// 这些翅膀使用类似于天使翅膀的数值
			// 飞行时间: 160 ticks = 2.67 秒
			// 飞行速度: 8
			// 加速度乘数: 2.0
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(160, 8f, 2.0f);
		}

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 20;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<LifeHealingWingPlayer>().hasLifeHealingWings = true;
			// 每秒恢复玩家1%的生命值 (60 ticks = 1秒)
			if (++player.GetModPlayer<LifeHealingWingPlayer>().playerHealTimer >= HealInterval) {
				player.GetModPlayer<LifeHealingWingPlayer>().playerHealTimer = 0;
				int healAmount = (int)(player.statLifeMax2 * healAmountPlayer);
				if (healAmount > 0) {
					player.Heal(healAmount);
					// 添加治疗效果的视觉反馈
					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, player.whoAmI, healAmount);
					}
				}
			}

			// 为队友恢复生命值
			foreach (Player teammate in Main.player) {
				// 检查玩家是否有效且不是当前玩家
				if (teammate.active && teammate.whoAmI != player.whoAmI && teammate.team == player.team && teammate.team != 0) {
					// 每秒恢复队友0.5%的生命值
					if (++teammate.GetModPlayer<LifeHealingWingPlayer>().teammateHealTimer >=HealInterval) {
						teammate.GetModPlayer<LifeHealingWingPlayer>().teammateHealTimer = 0;
						int healAmount = (int)(teammate.statLifeMax2 * healAmountTeammate);
						if (healAmount > 0) {
							teammate.Heal(healAmount);
							// 添加治疗效果的视觉反馈
							if (Main.netMode == NetmodeID.Server) {
								NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, teammate.whoAmI, healAmount);
							}
						}
					}
				}
			}
            player.noFallDmg = true;
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f; // 下降滑翔速度
			ascentWhenRising = 0.15f; // 上升速度
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 2f;
			constantAscend = 0.135f;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.ChlorophyteBar, 10)
				.AddIngredient(ItemID.LifeFruit, 5)
				.AddIngredient(ItemID.AngelWings, 1)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class LifeHealingWingPlayer : ModPlayer
	{
		public int playerHealTimer = 0;
		public int teammateHealTimer = 0;
        public bool hasLifeHealingWings = false;

		public override void ResetEffects() {
			hasLifeHealingWings = false;
		}
        public override void PostUpdate()
        {
            if (!hasLifeHealingWings)
            {
                playerHealTimer = 0;
                teammateHealTimer = 0;
            }
        }
    }
}