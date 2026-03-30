using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Global;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class SummonerRunicTablet : ModItem
    {
        public const float CritChanceBonus = 0.03f; // 5% 暴击率
        public const float SummonDamageBonus = 0.03f; // 额外 5% 召唤伤害
        
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(SummonDamageBonus, true),
            ValueUtils.FormatValue(CritChanceBonus, true)
            
        );

        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetStaticDefaults()
        {
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
            if(Item.favorited){
            player.GetModPlayer<SummonerRunicTabletPlayer>().summonerRuneEquipped = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.FallenStar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"SummonerRuneCrit", $"[c/00FF00:+{CritChanceBonus * 100}%暴击率（对标记目标）]"},
                    {"SummonerRuneTag", "[c/00FF00:对所有 IsATagBuff 类型的增益生效]"},
                    {"SummonerRuneDamage", $"[c/00FF00:+{SummonDamageBonus * 100}%召唤伤害]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
    }

    public class SummonerRunicTabletPlayer : ModPlayer
    {
        public bool summonerRuneEquipped = false;

        public override void ResetEffects()
        {
            summonerRuneEquipped = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (summonerRuneEquipped)
            {
                Player.GetDamage(DamageClass.Summon) += SummonerRunicTablet.SummonDamageBonus;
            }
        }
    }
}