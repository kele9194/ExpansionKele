using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles.GenericProj; // 添加抛射体命名空间引用

namespace ExpansionKele.Content.Buff
{
    public class FullMoonArmorDebuff : ModBuff
    {
        public override string LocalizationCategory=>"Buff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.color = Color.LightBlue;

            // 检查是否是减益的最后一刻
            if (npc.buffTime[buffIndex] == 1)
            {
                // 获取最后一次与此NPC互动的玩家
                Player player = Main.player[npc.lastInteraction];
                if (player != null && player.active)
                {
                    // 计算爆炸伤害（武器当前伤害的12倍）
                    int explosionDamage = (int)(player.GetWeaponDamage(player.HeldItem) * 12f);


                    // 生成FullMoonExplosion抛射体，伤害直接就是12倍武器伤害
                    Projectile.NewProjectile(
                        npc.GetSource_FromAI(), // 伤害来源
                        npc.Center, // 抛射体生成位置
                        Vector2.Zero, // 初始速度为0
                        ModContent.ProjectileType<FullMoonExplosion>(), // 抛射体类型
                        explosionDamage, // 伤害值（12倍武器伤害）
                        0f, // 击退
                        player.whoAmI // 所有者
                    );
                }
            }

            base.Update(npc, ref buffIndex);
        }
    }
}