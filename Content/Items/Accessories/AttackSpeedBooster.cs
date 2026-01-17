using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class AttackSpeedBooster : ModItem
    {
        public static float AttackSpeedBoostSpeed = 1.5f;// 50% 使用时间减少
        public static float AttackSpeedBoostDamage = 0.75f;// 0.75 倍基础乘算增伤
        public override string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            //Item.SetNameOverride("攻速手套");
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetModPlayer<SlowSpeedGlovePlayer>().SlowSpeedGloveEquipped)
            {
                return;
            }
            // 为装备此饰品的玩家添加修饰器
            player.GetModPlayer<AttackSpeedBoosterPlayer>().AttackSpeedBoosterEquipped = true;
            player.GetModPlayer<AttackSpeedBoosterPlayer>().attackSpeedBoosterMultiplier = AttackSpeedBoostSpeed;
            ExpansionKeleTool.MultiplyDamageBonus(player, AttackSpeedBoostDamage);
        }

        // ... existing code ...
        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"AttackSpeedBoostSpeed", $"[c/00FF00:攻击速度增加 {(AttackSpeedBoostSpeed-1)*100}%]"},
                    {"AttackSpeedBoostDamage", $"[c/00FF00:伤害增加 {(AttackSpeedBoostDamage*100):F0}%]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...
// ... existing code ...

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PowerGlove, 1);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 10);
            recipe.AddIngredient(ItemID.WarriorEmblem, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }

    public class AttackSpeedBoosterPlayer : ModPlayer
    {
        public bool AttackSpeedBoosterEquipped = false;
        public float attackSpeedBoosterMultiplier=1f;

        public override void ResetEffects()
        {
            AttackSpeedBoosterEquipped = false;
        }

        // ... existing code ...
    public override float UseSpeedMultiplier(Item item)
    {
        if (AttackSpeedBoosterEquipped)
        {
            return attackSpeedBoosterMultiplier;
        }
        return 1f;
    }
// ... existing code ...
        
    }
}