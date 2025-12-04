using Terraria;
using Terraria.ModLoader;
using System;
using System.Reflection;
using ExpansionKele.Content.Items.OtherItem;
using Terraria.ID;

namespace ExpansionKele.Global
{
    public class DefenseRegulatorGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        // Apply prefix modifications
        public override void UpdateEquip(Item item, Player player)
        {
            if (ModLoader.HasMod("CalamityMod") && item.prefix != 0)
            {
                ApplyDefensePrefixFix(item, player);
            }
        }

        public override void UpdateInventory(Item item, Player player)
        {
            // Removed Ironskin potion handling to prevent duplicate calculations
            // Ironskin fix is now handled in DefenseRegulatorPlayer.UpdateEquips
        }

        private int GetPlayerCurrentStage(Player player)
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

        private void ApplyDefensePrefixFix(Item item, Player player)
        {
            var exPlayer = player.GetModPlayer<DefenseRegulatorPlayer>();

            // Only apply if a stage is selected and not in unlimited mode
            if (exPlayer.selectedStage == -1 || exPlayer.selectedStage == 4)
                return;

            // Get player's current stage
            int playerStage = GetPlayerCurrentStage(player);
            
            // Calculate stage difference
            int stageDifference = exPlayer.selectedStage - playerStage;
            
            // No adjustment needed if stages match
            if (stageDifference == 0)
                return;

            // Hard / Guarding / Armored / Warding prefixes
            switch (item.prefix)
            {
                case PrefixID.Hard:
                    {
                        // Each stage difference adds/removes 1 defense
                        player.statDefense += stageDifference * 1;
                        break;
                    }
                case PrefixID.Guarding:
                    {
                        // Each stage difference adds/removes 1 defense
                        player.statDefense += stageDifference * 1;
                        break;
                    }
                case PrefixID.Armored:
                    {
                        // Each stage difference adds/removes 1 defense
                        player.statDefense += stageDifference * 1;
                        break;
                    }
                case PrefixID.Warding:
                    {
                        // Each stage difference adds/removes 1 defense
                        player.statDefense += stageDifference * 1;
                        break;
                    }
            }
        }
    }
}