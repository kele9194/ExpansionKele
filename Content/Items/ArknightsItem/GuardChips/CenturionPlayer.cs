using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    public class CenturionPlayer : ModPlayer
    {
        public bool HasCenturionWeaponEquipped { get; private set; } = false;
        private bool _isProcessingSplash = false;

        // ======================== 生命周期 ========================


        public override void ResetEffects()
        {
            HasCenturionWeaponEquipped = false;
        }
        // public override void PostItemCheck()
        // {
        //     // itemAnimation <= 0 说明这次挥砍已经播完了
        //     // 此时 OnHitNPC 已经在这一帧处理完毕，安全重置
        //     if (Player.itemAnimation <= 0)
        //     {
        //         _isProcessingSplash = false;
        //     }
        // }

        public override bool CanUseItem(Item item)
        {
            // ★ 任何武器使用时都重置溅射标记
            // （CanUseItem 会在每次新攻击开始时调用）
            _isProcessingSplash = false;
            return true;
        }

        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasCenturionWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasCenturionWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Centurion)
            {
                HasCenturionWeaponEquipped = false;
                return;
            }

            HasCenturionWeaponEquipped = true;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Centurion);
            blocking.CurrentGuardSubclass = GuardSubclass.Centurion;
        }

        // ======================== 专属 1.12x 阻挡增伤 ========================

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasCenturionWeaponEquipped) return;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (!blocking.IsInTeamBlockingSet(target)) return;

            modifiers.FinalDamage *= 1.12f;
        }

        // ======================== 半血以上 20% 减伤 ========================

        public override void ModifyHitByNPC(NPC attacker, ref Player.HurtModifiers modifiers)
        {
            if (!HasCenturionWeaponEquipped) return;
            if (Player.statLife <= Player.statLifeMax2 * 0.5f) return;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (!blocking.IsInTeamBlockingSet(attacker)) return;

            modifiers.FinalDamage *= 0.80f;
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!HasCenturionWeaponEquipped) return;
            if (Player.statLife <= Player.statLifeMax2 * 0.5f) return;

            if (!NPCProjectileSourceTracker.TryGetSourceNPC(proj, out NPC sourceNPC)) return;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (!blocking.IsInTeamBlockingSet(sourceNPC)) return;

            modifiers.FinalDamage *= 0.80f;
        }

        // ======================== 溅射传导伤害 ========================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasCenturionWeaponEquipped) return;
            if (_isProcessingSplash) return; // 本次攻击已触发过溅射

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (!blocking.IsInTeamBlockingSet(target)) return; // ← 改为团队版

            _isProcessingSplash = true;

            var blockingList = blocking.CurrentBlockingList;
            int splashDamage = (int)(damageDone * 0.20f);
            if (splashDamage <= 0) return;

            int projType = ModContent.ProjectileType<CenturionSlashProj>();

            for (int i = 0; i < blockingList.Count; i++)
            {
                NPC otherNPC = blockingList[i];
                if (otherNPC == null || !otherNPC.active) continue;
                if (otherNPC.whoAmI == target.whoAmI) continue;

                Projectile.NewProjectile(
                    Player.GetSource_OnHit(target),
                    otherNPC.Center,
                    Vector2.Zero,
                    projType,
                    splashDamage,
                    0f,
                    Player.whoAmI
                );
            }
            // 不必在 finally 重置 _isProcessingSplash，
            // ResetEffects 中 itemTime==0 时自动重置
        }
    }
}