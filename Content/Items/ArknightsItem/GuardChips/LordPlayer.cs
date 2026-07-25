using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 领主玩家状态管理器。
    /// 
    /// 领主特性：
    /// 1. 挡2
    /// 2. 300px内存在至少2个敌人时近战攻速+12%
    /// </summary>
    public class LordPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持领主刻印武器</summary>
        public bool HasLordWeaponEquipped { get; private set; } = false;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasLordWeaponEquipped = false;
        }

        /// <summary>
        /// 装备更新后执行：检查手持武器是否为领主刻印，设置 BlockingCap = 2。
        /// </summary>
        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasLordWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasLordWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Lord)
            {
                HasLordWeaponEquipped = false;
                return;
            }

            HasLordWeaponEquipped = true;

            // ===== 同步阻挡（挡2） =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Lord);
            blocking.CurrentGuardSubclass = GuardSubclass.Lord;
        }

        // ======================== 群战攻速加成 ========================

        /// <summary>
        /// 300px内存在至少2个敌人时，近战攻速+12%。
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            if (!HasLordWeaponEquipped) return 1f;
            if (!item.DamageType.CountsAsClass(DamageClass.Melee)) return 1f;

            int nearbyCount = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy()) continue;
                if (npc.friendly) continue;
                if (npc.Distance(Player.Center) > 300f) continue;
                nearbyCount++;
                if (nearbyCount >= 2) break;
            }

            return nearbyCount >= 2 ? 1.12f : 1f;
        }
    }
}