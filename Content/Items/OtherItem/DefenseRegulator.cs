using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class DefenseRegulator : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Defense Regulator");
            // Tooltip.SetDefault("When Calamity Mod is loaded, locks all defense prefixes and Ironskin potion effects to a fixed progression stage\n" +
            //                   "Left-click to cycle through stages: Pre-Hardmode → Hardmode → Post-Moon Lord → Post-DoG → Unlimited → Pre-Hardmode...");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.consumable = false;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 5);
        }

        // ... existing code ...
public override bool? UseItem(Player player)
{
    if (ModLoader.HasMod("CalamityMod"))
    {
        var exPlayer = player.GetModPlayer<DefenseRegulatorPlayer>();
        exPlayer.CycleStage();
        
        // Display current stage to player
        string stageName = "";
        switch (exPlayer.selectedStage)
        {
            case 0:
                stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StagePreHardmode");
                break;
            case 1:
                stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StageHardmode");
                break;
            case 2:
                stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StagePostMoonLord");
                break;
            case 3:
                stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StagePostDoG");
                break;
            case 4:
                stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StageUnlimited");
                break;
        }
        
        Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StageSet", stageName), Color.LightBlue);
        return true;
    }
    return false;
}
// ... existing code ...

        // ... existing code ...
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    // Show current stage
    Player player = Main.LocalPlayer;
    var exPlayer = player.GetModPlayer<DefenseRegulatorPlayer>();
    
    string stageName = "";
    switch (exPlayer.selectedStage)
    {
        case 0:
            stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StagePreHardmode");
            break;
        case 1:
            stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StageHardmode");
            break;
        case 2:
            stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StagePostMoonLord");
            break;
        case 3:
            stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StagePostDoG");
            break;
        case 4:
            stageName = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.StageUnlimited");
            break;
    }
    
    if (ModLoader.HasMod("CalamityMod"))
    {
        tooltips.Add(new TooltipLine(Mod, "CurrentStage", Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.CurrentStage", stageName)));
        tooltips.Add(new TooltipLine(Mod, "CalamityRequired", Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.CalamityRequired")));
    }
    else
    {
        tooltips.Add(new TooltipLine(Mod, "CalamityRequired", Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.DefenseRegulator.NeedCalamity"))
        {
            OverrideColor = Color.Gray
        });
    }
}
// ... existing code ...

        // Crafting recipe
        public override void AddRecipes()
        {

        }
    }
     public class DefenseRegulatorPlayer : ModPlayer
    {
        // 0 = Pre-hardmode, 1 = Hardmode, 2 = Post-Moon Lord, 3 = Post-DoG, 4 = Unlimited
        public int selectedStage = 0;

        public override void ResetEffects()
        {
            // Nothing to reset each frame for this item
        }

        public void CycleStage()
        {
            selectedStage = (selectedStage + 1) % 5; // Cycle through 0-4
        }

        public override void UpdateEquips()
        {
            // Apply Ironskin potion fix if Calamity is loaded and player has selected a stage (not unlimited)
            if (ModLoader.HasMod("CalamityMod") && selectedStage != 4)
            {
                // Check if player has Ironskin buff active
                if (Player.HasBuff(BuffID.Ironskin))
                {
                    ApplyIronskinFix();
                }
            }
        }

        private int GetPlayerCurrentStage()
        {
            // Determine the player's actual game stage
            int playerStage = 0; // Pre-HM
            
            if (Main.hardMode) 
                playerStage = 1; // HM
                
            if (NPC.downedMoonlord) 
                playerStage = 2; // Post-ML
                
            // Check for Post-DoG (Calamity)
            if (ModLoader.HasMod("CalamityMod"))
            {
                var calamityMod = ModLoader.GetMod("CalamityMod");
                if (calamityMod != null)
                {
                    var downedBossSystemType = calamityMod.Code.GetType("CalamityMod.Systems.DownedBossSystem");
                    if (downedBossSystemType != null)
                    {
                        var dogField = downedBossSystemType.GetField("downedDoG", BindingFlags.Public | BindingFlags.Static);
                        if (dogField != null)
                        {
                            bool downedDog = (bool)dogField.GetValue(null);
                            if (downedDog) 
                                playerStage = 3; // Post-DoG
                        }
                    }
                }
            }
            
            return playerStage;
        }

        private void ApplyIronskinFix()
        {
            // Only apply if a stage is selected and not in unlimited mode
            if (selectedStage == -1 || selectedStage == 4)
                return;

            // Get player's current stage
            int playerStage = GetPlayerCurrentStage();
            
            // Calculate stage difference
            int stageDifference = selectedStage - playerStage;
            
            // No adjustment needed if stages match
            if (stageDifference == 0)
                return;

            // Each stage difference adds/removes 2 defense from Ironskin potion
            Player.statDefense += stageDifference * 2;
        }

        // Helper method to determine what the defense value should be based on the locked stage
        public int GetFixedDefenseValue(int defaultValue, int preHM, int HM, int postML, int postDoG)
        {
            return selectedStage switch
            {
                0 => preHM,  // Pre-Hardmode
                1 => HM,     // Hardmode
                2 => postML, // Post-Moon Lord
                3 => postDoG,// Post-DoG
                _ => defaultValue // Unlimited - use default value
            };
        }
    }
}