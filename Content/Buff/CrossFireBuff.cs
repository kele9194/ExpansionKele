using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.OtherItem;

namespace ExpansionKele.Content.Buff
{
    public class CrossFireBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public static int maxStacks =50; // 最大30层
        public static float stackBonus=0.012f;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            
            // 获取当前buff层数
            var crossFirePlayer = player.GetModPlayer<CrossFirePlayer>();
            int stacks = crossFirePlayer.crossFireStacks;
            if (stacks > maxStacks) stacks = maxStacks;
            
            // 应用乘算增伤 (每层2%)
            damagePlayer.AddMultiplicativeDamageBonus(stacks *stackBonus);
        }
    }

    public class CrossFireGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // 检查是否有玩家参与了击杀
            if (npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers)
            {
                Player player = Main.player[npc.lastInteraction];
                
                // 检查玩家是否装备了CrossFire饰品
                if (player.GetModPlayer<CrossFirePlayer>().crossFireEquipped)
                {
                    var crossFirePlayer = player.GetModPlayer<CrossFirePlayer>();
                    // 增加一层buff层数
                    crossFirePlayer.crossFireStacks++;
                    if (crossFirePlayer.crossFireStacks > CrossFireBuff.maxStacks)
                        crossFirePlayer.crossFireStacks = CrossFireBuff.maxStacks;
                    
                    // 添加或刷新CrossFireBuff，持续5秒 (300 ticks)
                    player.AddBuff(ModContent.BuffType<CrossFireBuff>(), 300);
                }
            }
        }
    }
}