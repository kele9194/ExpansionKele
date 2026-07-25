using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    public class ArtsFighterPlayer : ModPlayer
    {
        public bool HasArtsFighterWeaponEquipped { get; private set; } = false;

        public override void ResetEffects()
        {
            HasArtsFighterWeaponEquipped = false;
        }

        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasArtsFighterWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasArtsFighterWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.ArtsFighter)
            {
                HasArtsFighterWeaponEquipped = false;
                return;
            }

            HasArtsFighterWeaponEquipped = true;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.ArtsFighter);
            blocking.CurrentGuardSubclass = GuardSubclass.ArtsFighter;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasArtsFighterWeaponEquipped) return;

            // ★ 0.7x 破甲 — 同时作用于近战挥砍和任何抛射体
            modifiers.DefenseEffectiveness *= 0.7f;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (!blocking.IsInTeamBlockingSet(target))
            {
                modifiers.FinalDamage *= 1.12f;
            }
        }
    }
}