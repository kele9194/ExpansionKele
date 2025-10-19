using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace ExpansionKele.Content.Buff
{
    public class FrostShatterDebuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = Main.pvpBuff[Type] = Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 减益效果会在FrostShatterNPC中处理
        }
    }

    public class FrostShatterNPC : GlobalNPC
    {
        // 存储每个NPC的减益时间
        public Dictionary<int, int> frostShatterTimes = new Dictionary<int, int>();
        
        // 存储原始防御值
        public Dictionary<int, int> originalDefenses = new Dictionary<int, int>();

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            // 每帧减少减益时间
            if (frostShatterTimes.ContainsKey(npc.whoAmI))
            {
                frostShatterTimes[npc.whoAmI]--;
                if (frostShatterTimes[npc.whoAmI] <= 0)
                {
                    // 移除减益时恢复原始防御力
                    if (originalDefenses.ContainsKey(npc.whoAmI))
                    {
                        npc.defense = originalDefenses[npc.whoAmI];
                        originalDefenses.Remove(npc.whoAmI);
                    }
                    frostShatterTimes.Remove(npc.whoAmI);
                }
            }
        }

        // 修改被物品击中时的效果
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            ApplyDamageModifier(npc, ref modifiers);
        }

        // 修改被弹道击中时的效果
        public override void ModifyHitByProjectile(NPC npc, Terraria.Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            ApplyDamageModifier(npc, ref modifiers);
        }

        private void ApplyDamageModifier(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (frostShatterTimes.TryGetValue(npc.whoAmI, out int time))
            {
                // 根据减益时间计算伤害倍数，最多增加200%伤害 (3600 ticks = 60秒)
                float damageMultiplier = 1f + time / 2400f;
                modifiers.FinalDamage *= damageMultiplier;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // 应用防御减少效果
            if (frostShatterTimes.TryGetValue(npc.whoAmI, out int time))
            {
                // 如果这是第一次应用减益，保存原始防御值
                if (!originalDefenses.ContainsKey(npc.whoAmI))
                {
                    originalDefenses[npc.whoAmI] = npc.defense;
                }
                
                // 减少50%防御力
                npc.defense = Math.Max(1, originalDefenses[npc.whoAmI] / 2);
            }
        }

        // 添加减益时间的方法
        public void AddFrostShatterTime(NPC npc, int timeToAdd)
        {
            // 如果没有原始防御值记录，则记录当前防御值
            if (!originalDefenses.ContainsKey(npc.whoAmI))
            {
                originalDefenses[npc.whoAmI] = npc.defense;
            }

            // 增加减益时间，最大不超过3600 ticks (60秒)
            int currentTime = frostShatterTimes.GetValueOrDefault(npc.whoAmI, 0);
            frostShatterTimes[npc.whoAmI] = Math.Min(currentTime + timeToAdd, 3600);
        }
    }
}