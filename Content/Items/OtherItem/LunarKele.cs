using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Bosses;
using ExpansionKele.Content.Bosses.BossKele;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class LunarKele : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetStaticDefaults()
        {
            // 启用右键功能（可选）
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("夜明可乐罐");
            Item.width = 20; // 物品宽度
            Item.height = 20; // 物品高度
            Item.useTime = 45; // 使用时间
            Item.useAnimation = 45; // 使用动画时间
            Item.useStyle = ItemUseStyleID.HoldUp; // 使用方式
            Item.noMelee = true; // 不是近战物品
            Item.value = Item.sellPrice(0, 0, 1, 20); // 价值
            Item.rare = ItemRarityID.Purple; // 稀有度
            Item.UseSound = SoundID.Item44; // 使用音效
            Item.autoReuse = false; // 不自动重用
            Item.maxStack = 1; // 最大堆叠数量
            Item.consumable = false; // 不可消耗
        }

        public override bool CanUseItem(Player player)
        {
            // 确保BossKele未被召唤
            return !NPC.AnyNPCs(ModContent.NPCType<BossKele>());
        }

        public override bool? UseItem(Player player)
        {
            // 召唤BossKele
            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<BossKele>());
            return true;
        }
        
        public override void AddRecipes()
        {
            // 创建夜明可乐罐的合成配方
            Recipe recipe = Recipe.Create(ModContent.ItemType<LunarKele>());
            recipe.AddIngredient(ItemID.LunarBar, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}