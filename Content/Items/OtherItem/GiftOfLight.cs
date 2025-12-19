using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class GiftOfLight : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("光明者的赠礼");
            // Tooltip.SetDefault("放入背包后激活：\n每秒获得洞穴探险和光芒药水效果");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

        public override void UpdateInventory(Player player)
        {
            // 当物品在背包中且被收藏时应用效果
            if (Item.favorited)
            {
                // 添加洞穴探险和光芒效果，持续2秒（每秒刷新）
                player.AddBuff(BuffID.Spelunker, 120); // 2秒 = 120 ticks
                player.AddBuff(BuffID.Shine, 120);     // 2秒 = 120 ticks
            }
        }

        public override void AddRecipes()
        {
            // 创建配方
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SpelunkerPotion, 1);
            recipe.AddIngredient(ItemID.ShinePotion, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

    }

    public class GiftOfLightPlayer : ModPlayer
    {
        // 用于跟踪玩家是否已经领取过新手礼包
        public bool receivedStarterGift = false;

        public override void OnEnterWorld()
        {
            // 检查玩家是否已经领取过新手礼包
            if (!receivedStarterGift)
            {
                // 给予玩家光明者的赠礼物品
                Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), ModContent.ItemType<GiftOfLight>());
                
                // 标记玩家已经领取过礼包
                receivedStarterGift = true;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            // 加载玩家是否已经领取过礼包的数据
            receivedStarterGift = tag.ContainsKey("receivedStarterGift") && tag.GetBool("receivedStarterGift");
        }

        public override void SaveData(TagCompound tag)
        {
            // 保存玩家是否已经领取过礼包的数据
            tag["receivedStarterGift"] = receivedStarterGift;
        }
    }
}