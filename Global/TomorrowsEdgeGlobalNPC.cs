using System.Collections.Generic;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 让本武器的所有斩击命中时只出武器自己的音效，不再叠 <see cref="NPC.HitSound"/>。
    /// <para>
    /// 覆盖本体弧光（<see cref="TomorrowsEdgeSlash"/>）与一阶段贴怪斩击 / 归宿斩击
    /// （两者都是 <see cref="ThanatosSlash"/>）。
    /// </para>
    /// <para>
    /// TML 没有提供拦截受击音的钩子；<see cref="NPC.StrikeNPC"/> 内部是
    /// <c>HitEffect(hit)</c> → <c>if (HitSound != null) SoundEngine.PlaySound(...)</c>，
    /// 所以唯一入口是命中前把 <see cref="NPC.HitSound"/> 临时置空。
    /// </para>
    /// <para>
    /// 时序依据（Projectile.cs）：<c>ModifyHitByProjectile</c> @10481 早于
    /// <c>StrikeNPC</c> @11204，<c>OnHitByProjectile</c> @11485 晚于它，正好把播放点夹在中间。
    /// </para>
    /// <para>
    /// 这两个钩子只在伤害计算端执行，所以只有攻击者自己听不到受击音，旁观玩家不受影响。
    /// </para>
    /// </summary>
    public class TomorrowsEdgeGlobalNPC : GlobalNPC
    {
        // 只恢复「确实被我们改过」的 NPC，避免覆盖别的模组动态设置的 HitSound。
        // 命中流程是同步的，存取之间不会插入另一个 NPC 的命中，所以用 whoAmI 做键是安全的。
        private static readonly Dictionary<int, SoundStyle?> SuppressedHitSound = new();

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!IsOwnSlash(projectile.type))
            {
                return;
            }

            // 上一次异常路径没走到恢复的话，先复原，免得这个 NPC 永久失声。
            Restore(npc);

            SuppressedHitSound[npc.whoAmI] = npc.HitSound;
            npc.HitSound = null;
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Restore(npc);
        }

        private static bool IsOwnSlash(int projectileType)
        {
            return projectileType == ModContent.ProjectileType<TomorrowsEdgeSlash>()
                || projectileType == ModContent.ProjectileType<ThanatosSlash>();
        }

        private static void Restore(NPC npc)
        {
            if (SuppressedHitSound.Remove(npc.whoAmI, out SoundStyle? original))
            {
                npc.HitSound = original;
            }
        }
    }
}
