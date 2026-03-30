using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ExpansionKele.Content.Items.OtherItem.BagItem;

namespace ExpansionKele.Global
{
    // ... existing code ...
    public class SummonerRunicTabletGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        /// <summary>
        /// 标记该 NPC 当前是否有任何 Tag Buff
        /// </summary>
        public bool hasAnyTagBuff = false;

        /// <summary>
        /// Tag Buff 的暴击伤害倍率，默认为 2.0（双倍伤害）
        /// </summary>
        public float tagBuffCritDamageMultiplier = 2.0f;

        /// <summary>
        /// 召唤师符文的暴击概率，默认为 50%
        /// </summary>
        public float summonerCritChance = 0f;

        // Deleted:public override void ResetEffects(NPC npc)
        // Deleted:{
        // Deleted:hasAnyTagBuff = false;
        // Deleted:summonerCritChance = 0f;
        // Deleted:tagBuffCritDamageMultiplier = 2.0f;
        // Deleted:summonerCritChance =_additionalCritChance;
        // Deleted:_additionalCritChance = 0f;
        // Deleted:CheckForAnyTagBuff(npc);
        // Deleted:}

        public override void ResetEffects(NPC npc)
        {
            hasAnyTagBuff = false;
            summonerCritChance = 0f;
            tagBuffCritDamageMultiplier = 2.0f;
            CheckForAnyTagBuff(npc);
        }

        // ... existing code ...
        /// <summary>
        /// 检查 NPC 是否有任何 Tag Buff
        /// </summary>
        private void CheckForAnyTagBuff(NPC npc)
        {
            hasAnyTagBuff = false;
            
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                int buffType = npc.buffType[i];
                if (buffType <= 0) 
                    continue;
                
                if (BuffID.Sets.IsATagBuff[buffType])
                {
                    hasAnyTagBuff = true;
                    tagBuffCritDamageMultiplier = 2.0f;
                    break;
                }
            }
        }


        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!hasAnyTagBuff)
                return;
            
            Player player = Main.player[projectile.owner];
            if (!player.active || player.dead || player.ghost)
                return;
            
            if (!player.GetModPlayer<SummonerRunicTabletPlayer>().summonerRuneEquipped)
                return;
            
            if (projectile.DamageType.CountsAsClass(DamageClass.Summon))
            {
                float totalCritChance = SummonerRunicTablet.CritChanceBonus;
                
                if (Main.rand.NextFloat() < totalCritChance)
                {
                    modifiers.SetCrit();
                }
            }
        }
    }
}
// ... existing code ...