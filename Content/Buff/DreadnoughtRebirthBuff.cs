using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 无畏重生 Buff — 由无畏者致命伤害触发。
    /// 效果：生命上限降至40%，攻速×1.3。
    /// 持续期间若再次死亡，则真正死亡。
    /// Buff结束时施加6分钟冷却Debuff。
    /// </summary>
    public class DreadnoughtRebirthBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";


        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;  // 正面效果
            Main.buffNoSave[Type] = false; // 死亡不保留
            BuffID.Sets.NurseCannotRemoveDebuff[base.Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<DreadnoughtRebirthPlayer>().isRebirth=true;
        }
    }

    public class DreadnoughtRebirthPlayer : ModPlayer{
        public bool isRebirth=false;
        public const float RebirthLife=0.4f;
        public const float AttackSpeedBonus=0.3f;

        public override void ResetEffects()
        {
            isRebirth=false;
        }
        public override void PostUpdateMiscEffects()
        {
            if(isRebirth){
                Player.statLifeMax2=(int)(Player.statLifeMax2*RebirthLife);
            }
        }
        public override float UseSpeedMultiplier(Item item)
        {
            if(isRebirth){
                return 1f+AttackSpeedBonus;
            }
            return 1f;
        }
    }
}