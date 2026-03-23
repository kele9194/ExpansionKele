using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class TestLocatorCopy : ModItem
    {
        public static LocalizedText MultiplicativeDamageText { get; private set; }
        public static LocalizedText PreDefenseCombinedText { get; private set; }
        public static LocalizedText PostDefenseText { get; private set; }
        public override string LocalizationCategory => "Items.OtherItem";

        // ... existing code ...
public override void SetStaticDefaults()
{
    MultiplicativeDamageText = ModContent.GetInstance<TestLocatorCopy>().GetLocalization("MultiplicativeDamageText");
    PreDefenseCombinedText = ModContent.GetInstance<TestLocatorCopy>().GetLocalization("PreDefenseCombinedText");
    PostDefenseText = ModContent.GetInstance<TestLocatorCopy>().GetLocalization("PostDefenseText");
}
// ... existing code ...
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    Player player = Main.LocalPlayer;
    
    var damageMultiPlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
    float multiplicativeDamage = damageMultiPlayer.MultiplicativeDamageBonus;
    
    var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
    float preDefenseDamageReductionMulti = reductionPlayer.preDefenseDamageReductionMulti;
    float preDefenseDamageReduction = reductionPlayer.preDefenseDamageReduction;
    float customDamageReductionMulti = reductionPlayer.customDamageReductionMulti;
    float customDamageReduction = reductionPlayer.customDamageReduction;

    float preDefenseCombined = preDefenseDamageReductionMulti * (1f - preDefenseDamageReduction);
    float postDefenseCombined = customDamageReductionMulti * (1f - customDamageReduction);

    // ... existing code ...
    string multiplicativeDamageStr = MultiplicativeDamageText.WithFormatArgs((multiplicativeDamage * 100).ToString("F2")).Value;
    string preDefenseCombinedStr = PreDefenseCombinedText.WithFormatArgs((preDefenseCombined * 100).ToString("F2")).Value;
    string postDefenseCombinedStr = PostDefenseText.WithFormatArgs((postDefenseCombined * 100).ToString("F2")).Value;
    
    tooltips.Add(new TooltipLine(Mod, "MultiplicativeDamage", multiplicativeDamageStr));
    tooltips.Add(new TooltipLine(Mod, "PreDefenseCombined", preDefenseCombinedStr));
    tooltips.Add(new TooltipLine(Mod, "PostDefenseCombined", postDefenseCombinedStr));
}
// ... existing code ...
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("ExpansionKele:AnyIronBars", 6)
                .AddTile(TileID.Anvils)
                .Register();
        }

    }
}