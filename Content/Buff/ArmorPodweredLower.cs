using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework; // 添加这行以引入 Microsoft.Xna.Framework.Color
using System.Collections.Generic;
using Terraria.Localization; // 添加这行以引入 Dictionary

namespace ExpansionKele.Content.Buff
{
    public class ArmorPodweredLower : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public static int DefenseReduction = 20;
        public static float MultiplicativeDefenseReduction = 0.8f; // 25% 减少意味着剩下 75%
        public static float MultiplicativeEnduranceReduction = 0.5f; // 50% 减少伤害减免

        private static readonly Dictionary<int, int> originalDefenses = new Dictionary<int, int>(); // 声明并初始化字典

        public override void SetStaticDefaults()
        {
            Main.debuff[base.Type] = true;
            Main.pvpBuff[base.Type] = true;
            Main.buffNoSave[base.Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[base.Type] = true;
            BuffID.Sets.LongerExpertDebuff[base.Type] = true;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = Language.GetText("Mods.ExpansionKele.Buff.ArmorPodweredLower.Description").Format(DefenseReduction, (1 - MultiplicativeDefenseReduction) * 100, MultiplicativeEnduranceReduction * 100);
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (!originalDefenses.ContainsKey(npc.whoAmI))
            {
                // 保存原始防御值
                originalDefenses[npc.whoAmI] = npc.defense;

                // 计算新的防御值
                npc.defense = (int)(npc.defense * MultiplicativeDefenseReduction- DefenseReduction);
                if (npc.defense < 0)
                {
                    npc.defense = 0;
                }

                // 设置颜色为蓝色
                npc.color = Color.Blue;

                // 添加日志输出
                //Main.NewText($"NPC {npc.type} 的防御减少为 {npc.defense}");
            }

            npc.buffTime[buffIndex] -= 1;

            // 如果 buff 时间结束，移除 buff
            if (npc.buffTime[buffIndex] <= 0)
            {
                npc.DelBuff(buffIndex);
                buffIndex--;
                npc.color = Color.White;
                if (originalDefenses.ContainsKey(npc.whoAmI))
                {
                    npc.defense = originalDefenses[npc.whoAmI];
                    originalDefenses.Remove(npc.whoAmI);

                    // 添加日志输出
                    //Main.NewText($"NPC {npc.type} 的防御恢复为 {npc.defense}");
                }
            }
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 增加玩家的伤害减免效果
            player.endurance *= 0.5f; // 50% 减少伤害减免

            // 如果你需要对玩家也应用其他效果，可以在这里添加
        }
        
    }
}