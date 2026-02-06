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

		// 修改UpdateAccessory方法，只设置标志位，不在这里执行治疗逻辑
		public override void UpdateAccessory(Player player, bool hideVisual) {
            player.GetModPlayer<LifeHealingWingPlayer>().hasLifeHealingWings = true;
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
        public bool hasLifeHealingWings = false;

		public override void ResetEffects() {
			hasLifeHealingWings = false;
		}
        
        public override void PostUpdate()
        {
            // 只有当装备了生命治疗之翼时才进行处理
            if (!hasLifeHealingWings)
            {
                playerHealTimer = 0;
                return;
            }

            // 只在单人游戏或服务器端执行实际的治疗逻辑，避免多人模式下重复执行
            if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
            {
                // 处理玩家自身治疗
                if (++playerHealTimer >= 250)
                {
                    playerHealTimer = 0;
                    int healAmount = (int)(Player.statLifeMax2 * 0.03f);
                    
                    // 只在玩家生命值未满时进行治疗
                    
                        Player.Heal(healAmount);
                        
                        // 在服务器模式下发送网络消息同步治疗效果
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, Player.whoAmI, healAmount);
                        }
                }
            }
        }
    }
}