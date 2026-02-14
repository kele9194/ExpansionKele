using ExpansionKele.Content.Projectiles.SummonProj;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class SpaceFunnelMinionBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("空间浮游炮");
            // Description.SetDefault("浮游炮会为你而战");
            
            Main.buffNoSave[Type] = true; // buff不会保存到存档
            Main.buffNoTimeDisplay[Type] = true; // 不显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 如果还有浮游炮存在，重置buff时间
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SpaceFunnelMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000; // 保持长时间活跃
            }
            else
            {
                // 没有浮游炮时移除buff
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}