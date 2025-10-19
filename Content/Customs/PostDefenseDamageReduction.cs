using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ExpansionKele.Content.Customs
{
    public class PostDefenseDamageReduction : GlobalNPC
    {
        /// <summary>
        /// 存储NPC的防御后百分比减伤值
        /// </summary>
        public Dictionary<int, float> PostDefenseReduction = new Dictionary<int, float>();

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            // 每帧重置减伤值
            if (PostDefenseReduction.ContainsKey(npc.whoAmI))
            {
                PostDefenseReduction[npc.whoAmI] = 0f;
            }
        }

        /// <summary>
        /// 设置防御后百分比减伤值（直接设置方式）
        /// </summary>
        /// <param name="npc">NPC实例</param>
        /// <param name="reduction">减伤百分比（0.0-1.0）</param>
        public void SetPostDefenseReduction(NPC npc, float reduction)
        {
            PostDefenseReduction[npc.whoAmI] = reduction;
        }

        /// <summary>
        /// 增加防御后百分比减伤值（加算方式）
        /// </summary>
        /// <param name="npc">NPC实例</param>
        /// <param name="reduction">要增加的减伤百分比（0.0-1.0）</param>
        public void AddPostDefenseReduction(NPC npc, float reduction)
        {
            if (PostDefenseReduction.ContainsKey(npc.whoAmI))
            {
                PostDefenseReduction[npc.whoAmI] += reduction;
            }
            else
            {
                PostDefenseReduction[npc.whoAmI] = reduction;
            }

            // 限制最大减伤为90%
            if (PostDefenseReduction[npc.whoAmI] > 1f)
                PostDefenseReduction[npc.whoAmI] = 1f;
        }

        /// <summary>
        /// 在防御计算之后应用减伤
        /// </summary>
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            ApplyPostDefenseReduction(npc, ref modifiers);
        }

        /// <summary>
        /// 应用防御后减伤
        /// </summary>
        private void ApplyPostDefenseReduction(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (PostDefenseReduction.ContainsKey(npc.whoAmI) && PostDefenseReduction[npc.whoAmI] > 0)
            {
                // 防御后减伤在所有其他计算后应用，进一步减少最终伤害
                modifiers.FinalDamage *= (1f - PostDefenseReduction[npc.whoAmI]);
            }
        }
    }
}