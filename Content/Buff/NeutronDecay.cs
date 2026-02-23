using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.NPC;
using System.Collections.Generic;

namespace ExpansionKele.Content.Buff
{
    // 全局NPC类用于跟踪中子衰变的独立计时器
    public class NeutronDecayNPC : GlobalNPC
    {
        // 存储每个NPC的伤害计时器
        public static Dictionary<int, int> damageTimers = new Dictionary<int, int>();
        
        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            // 每帧检查并更新计时器
            if (damageTimers.ContainsKey(npc.whoAmI))
            {
                damageTimers[npc.whoAmI]--;
                // 不要在这里移除计时器，让buff的Update方法处理
            }
        }

        // 添加或重置伤害计时器的方法
        public static void SetDamageTimer(NPC npc, int timer = 120)
        {
            damageTimers[npc.whoAmI] = timer;
        }

        // 检查是否应该造成伤害（计时器归零时）
        public static bool ShouldDealDamage(NPC npc)
        {
            return damageTimers.ContainsKey(npc.whoAmI) && damageTimers[npc.whoAmI] <= 0;
        }
        
        // 清理计时器的方法
        public static void RemoveDamageTimer(NPC npc)
        {
            if (damageTimers.ContainsKey(npc.whoAmI))
            {
                damageTimers.Remove(npc.whoAmI);
            }
        }
    }
    
    public class NeutronDecay : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public static float decayBonus = 0.5f; // 0.5% 每次伤害
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
            
            // 初始化伤害计时器（如果不存在）
            if (!NeutronDecayNPC.damageTimers.ContainsKey(npc.whoAmI))
            {
                NeutronDecayNPC.SetDamageTimer(npc, 120); // 2秒间隔（60FPS）
            }
            
            // 检查是否应该造成伤害
            if (NeutronDecayNPC.ShouldDealDamage(npc))
            {
                // 修正伤害计算：基于最大生命的百分比
                int damage = (int)(npc.lifeMax * decayBonus / 100f + 0.5f);
                
                // 确保最小伤害为1
                if (damage < 1) damage = 1;
                
                if (npc.life - damage > 0)
                {
                    npc.life -= damage; // 减少当前生命值
                    CombatText.NewText(npc.Hitbox, Color.Blue, damage, false, true);
                    // 触发生命值变化事件
                    npc.netUpdate = true;
                }
                else
                {
                    if (npc.type == NPCID.TargetDummy)
                    {
                        npc.life = 1; // 目标假人至少保留1点生命
                    }
                    else
                    {
                        npc.StrikeInstantKill();
                    }
                    CombatText.NewText(npc.Hitbox, Color.Aqua, damage, true, true);
                }
                
                // 重置伤害计时器
                NeutronDecayNPC.SetDamageTimer(npc, 120);
            }
        }
        
    }
}