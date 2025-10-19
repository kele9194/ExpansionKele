using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures; // 添加这一行
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.NPC;

namespace ExpansionKele.Content.Buff
{
    
    public class NeutronDecay : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public static float decayBonus=0.5f;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }
        
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = Language.GetText("Mods.ExpansionKele.Buff.NeutronDecay.Description").Format(decayBonus);
        }

        public static float ReturnDEcay(){
            return decayBonus;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.color = Color.Blue;
            if (npc.buffTime[buffIndex] % 120 == 0) // 每120ticks执行一次
            {
                int damage = (int)(npc.lifeMax * decayBonus/100f+0.5f); // 计算要减少的生命值
                if (npc.life - damage > 0)
                {
                    npc.life -= damage; // 减少当前生命值
                    CombatText.NewText(npc.Hitbox, Color.Blue, damage, false, true);
                }
                else
                {
                    // 使用 StrikeNPC 来模拟攻击并触发死亡逻辑
                    HitInfo hitInfo = new HitInfo()
                    {
                        Damage = 9999,
                        Crit = false,
                        Knockback = 0f,
                        HitDirection = 0,
                        InstantKill = true
                    };

                    // 特殊处理训练假人
                    if (npc.type == NPCID.TargetDummy)
                    {
                        npc.life = 0;
                    }
                    else
                    {
                        npc.StrikeNPC(hitInfo);
                    }
                    CombatText.NewText(npc.Hitbox, Color.Aqua, damage, true, true);
                }
            }
        }
    }
}