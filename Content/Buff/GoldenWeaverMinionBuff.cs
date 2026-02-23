using ExpansionKele.Content.Projectiles.SummonProj;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class GoldenWeaverMinionBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("金锦针");
            // Description.SetDefault("小金针会为你而战，永不消失");
            
            Main.buffNoSave[Type] = true; // buff不会保存到存档
            Main.buffNoTimeDisplay[Type] = true; // 不显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 检查是否还有金针存在
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GoldenWeaverMinion>()] > 0)
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