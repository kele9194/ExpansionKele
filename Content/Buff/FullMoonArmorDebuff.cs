using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

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

                    // 创建爆炸效果
                    CombatText.NewText(npc.Hitbox, Color.LightBlue, "满月爆炸!", false, true);

                    // 对NPC造成伤害
                    NPC.HitInfo hitInfo = new NPC.HitInfo();
                    hitInfo.Damage = explosionDamage;
                    hitInfo.Knockback = 0;
                    hitInfo.HitDirection = 0;
                    hitInfo.Crit = false;
                    npc.StrikeNPC(hitInfo, true, true);
                }
            }

            base.Update(npc, ref buffIndex);
        }
    }
}