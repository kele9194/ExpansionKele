using ExpansionKele.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class FullMoonMinionBuff : ModBuff
    {
        public override string LocalizationCategory =>"Buff";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月守护");
            // Description.SetDefault("月亮会保护你");
            
            Main.buffNoSave[Type] = true; // buff不会保存
            Main.buffNoTimeDisplay[Type] = true; // 不显示剩余时间
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 给予一些缓冲时间，避免在生成瞬间因为检测不到而移除
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonMinionController>()] > 0)
            {
                player.buffTime[buffIndex] = 1800; // 重置buff时间
            }
            // 如果主控弹幕不存在但普通月亮还存在，也保留buff一段时间
            else if (player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 1800; // 重置buff时间
            }
            else
            {
                player.DelBuff(buffIndex); // 移除buff
                buffIndex--; // 调整索引
            }
        }
    }
}