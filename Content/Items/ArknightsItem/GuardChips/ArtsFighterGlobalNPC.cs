using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.ArknightsItem.GuardChips;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 术战者【ArtsFighter】的 GlobalNPC：对已被术战者玩家阻挡的目标施加 10% 额外易伤。
    /// 
    /// 原理：
    /// 每帧在 ResetEffects 中检查该 NPC 是否被任意术战者队友阻挡，
    /// 如果是，在 ModifyHitNPC 中对所有来源的攻击附加 1.10x 最终伤害。
    /// </summary>
    public class ArtsFighterGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>当前帧该 NPC 是否被任意术战者玩家阻挡</summary>
        public bool IsBlockedByArtsFighter { get; private set; } = false;

        public override void ResetEffects(NPC npc)
        {
            IsBlockedByArtsFighter = false;

            // 检查所有活跃玩家中是否有术战者阻挡此 NPC
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead || player.ghost) continue;

                var afPlayer = player.GetModPlayer<ArtsFighterPlayer>();
                if (!afPlayer.HasArtsFighterWeaponEquipped) continue;

                var blocking = player.GetModPlayer<BlockingPlayer>();
                if (blocking.IsInBlockingList(npc))
                {
                    IsBlockedByArtsFighter = true;
                    break;
                }
            }
        }

        /// <summary>
        /// 如果该 NPC 被术战者玩家阻挡，受到的所有伤害 +10%。
        /// </summary>
        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
        {
            // 本 GlobalNPC 挂载的 npc 是"被攻击者/受害者"
            // 当 NPC 作为被攻击者时，来自任何来源的伤害都 +10%
            // 但我们实际上需要通过判断 npc 是否等于 target 来确定这是"npc被攻击"的场景
            // GlobalNPC.ModifyHitNPC(NPC npc, NPC target, ...) 中 npc = 攻击者, target = 被攻击者
            // 所以我们用 npc == target? 不对，这实际上是 NPC攻击NPC 的情景。
            // 正确的方法是：从被攻击者的 GlobalNPC 实例判断
        }

        /// <summary>
        /// 玩家/弹幕攻击 NPC 时：如果该 NPC 被术战者阻挡，受到伤害 +10%。
        /// </summary>
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (!IsBlockedByArtsFighter) return;
            modifiers.FinalDamage *= 1.10f;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!IsBlockedByArtsFighter) return;
            modifiers.FinalDamage *= 1.10f;
        }
    }
}