using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ExpansionKele.Content.Items.Weapons.Melee;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class BossKeleNewBag : ModItem
    {
        public override string LocalizationCategory => "Bosses.BossKeleNew";
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TangDynastySaber>(), 4, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarryBar>(), 1, 30, 40));
            itemLoot.Add(ItemDropRule.Common(ItemID.PlatinumCoin, 1, 2, 2));
            itemLoot.Add(ItemDropRule.Common(ItemID.SuperHealingPotion, 1, 15, 20));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<CodeChaos>(), 4, 1, 1));
        }
    }
}