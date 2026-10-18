using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.ArknightsItem.DefenderChip
{
    /// <summary>
    /// 铁卫玩家状态管理器。
    /// 铁卫特性（重装的基础模板）：
    /// 1. 挡4
    /// 2. 自身防御有效性 ×1.2（防御减伤效果提升20%）
    /// </summary>
    public class ProtectorPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持铁卫刻印武器</summary>
        public bool HasProtectorWeaponEquipped { get; private set; } = false;

        public override void ResetEffects()
        {
            HasProtectorWeaponEquipped = false;
        }

        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasProtectorWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasProtectorWeaponEquipped = false;
                return;
            }

            DefenderSubclass subclass = ArknightsGlobalItem.GetDefenderSubclass(held);
            if (subclass != DefenderSubclass.Protector)
            {
                HasProtectorWeaponEquipped = false;
                return;
            }

            HasProtectorWeaponEquipped = true;

            // ===== 同步阻挡（挡4） =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetDefenderBlockingCap(DefenderSubclass.Protector); // 挡4
            blocking.CurrentDefenderSubclass = DefenderSubclass.Protector;

            // ===== 铁壁：防御有效性 ×1.2 =====
            // 与 Crusher 的 `Player.DefenseEffectiveness *= 0f` 同一机制，乘算叠加
            Player.DefenseEffectiveness *= 1.2f;
        }
    }
}