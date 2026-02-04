using ExpansionKele.Content.Projectiles.SummonProj;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class FullMoonMinionBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月守护");
            // Description.SetDefault("月亮会保护你");
            
            Main.buffNoSave[Type] = true; // buff不会保存到存档
            Main.buffNoTimeDisplay[Type] = true; // 不显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 如果还有月亮存在，重置buff时间
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000; // 保持长时间活跃
            }
            else
            {
                // 没有月亮时移除buff
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}