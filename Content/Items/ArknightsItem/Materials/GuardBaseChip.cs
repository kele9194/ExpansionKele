using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.Materials
{
    /// <summary>
    /// 近卫基础芯片：近卫系分支芯片的合成基底，本身没有战斗效果。
    /// <para/>
    /// 与 <see cref="SniperBaseChip"/> 同构 —— 效果全部落在分支芯片上，
    /// 基底只作为前置材料，避免与分支饰品功能重叠。
    /// <para/>
    /// 获取途径：可乐的商店出售（见 <c>Content/NPCs/TownNPC/Cola.cs</c> 的 <c>AddShops</c>）。
    /// </summary>
    public class GuardBaseChip : ModItem
    {
        public override string LocalizationCategory => "Items.ArknightsItem.Materials";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
            // 售价就是商店的买价（NPCShop 默认用 Item.value），也是玩家卖回时的基准。
            Item.value = Item.buyPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}
