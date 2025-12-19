using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class CrossFire : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetDefaults()
        {
            Item.SetNameOverride("交叉火力");
            Item.width = 60;
            Item.height = 60;
            // 删除accessory属性，因为不再作为饰品使用
            // Item.accessory = true;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
        }

        // 使用UpdateInventory替代UpdateAccessory
        public override void UpdateInventory(Player player)
        {
            // 当物品在背包中且被收藏时激活CrossFirePlayer功能
            if (Item.favorited)
            {
                player.GetModPlayer<CrossFirePlayer>().crossFireEquipped = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ExplosivePowder, 50) // 爆炸粉
                .AddIngredient<SigwutBar>(3) // sigwut锭
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

public class CrossFirePlayer : ModPlayer
    {
        public bool crossFireEquipped = false;
        public int crossFireStacks = 0;

        public override void ResetEffects()
        {
            crossFireEquipped = false;
        }
        
        public override void PostUpdateBuffs()
        {
            // 检查玩家是否拥有CrossFireBuff，如果没有则将层数清零
            if (!Player.HasBuff(ModContent.BuffType<Buff.CrossFireBuff>()))
            {
                crossFireStacks = 0;
            }
        }
    }
}