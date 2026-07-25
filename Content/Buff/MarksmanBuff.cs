using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 神射手未击杀补偿：射弹未能击败敌人时，攻速 ×1.1，持续2秒
    /// </summary>
    public class MarksmanBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;   // 死亡后不保留
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 增加 10% 远程攻击速度
            player.GetAttackSpeed<RangedDamageClass>() += 0.1f;
        }
    }
}