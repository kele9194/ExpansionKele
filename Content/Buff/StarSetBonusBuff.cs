using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.Localization;
namespace ExpansionKele.Content.Buff
{
    public class StarSetBonusBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public float extraDamage=0f;
        public float extraTime=504;

        public void  SetExtraDamage(float Damage) {
            extraDamage = Damage;
        }
        public void  SetTime(float Time) {
            extraTime = Time;
        }

        
        private float AdditiveGenericDamageBonus = 0.30f;

        private  float a1=1.5f;//未完成

        public override void SetStaticDefaults()
        {
            // 设置 Buff 的显示名称和描述
            // DisplayName.SetDefault("星元套装增益");
            // Description.SetDefault($"增加所有伤害 {AdditiveGenericDamageBonus}%");
            Main.buffNoTimeDisplay[Type] = false; // 显示计时器
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = Language.GetText("Mods.ExpansionKele.Buff.StarSetBonusBuff.Description").Value;
        }

         public override void Update(Player player, ref int buffIndex)
        {
            ExpansionKeleTool.AddDamageBonus(player,0.15f);
            float lifePercentage = player.statLife / (float)player.statLifeMax2;

            // 常数 a
            if (lifePercentage > 1){
                lifePercentage = 1;
            }

            // 计算伤害提升幅度
            float alphaDamageBoost = (1 / (lifePercentage + a1)) - (1 / (1 + a1));

            float damageBoost = (alphaDamageBoost+1) * AdditiveGenericDamageBonus;
            // 应用增伤效果
            player.GetDamage<GenericDamageClass>() += damageBoost;
			
            int frametime = (int)(extraTime * 12.5 / player.statLifeMax2 *2+ 0.5f);
            
            
            

            // 设置生命再生减益标志
            player.GetModPlayer<StarSetBonusPlayer>().lifeRegenDebuff = true;

            // 设置玩家盔甲颜色为微微的红色
            player.skinColor = new Color(255, 100, 100, 255);

            // 检查 buff 是否即将结束
            int frameCounter = player.GetModPlayer<StarSetBonusPlayer>().frameCounter;

    // 每 x 帧回复 1 血量
    if (frameCounter >= frametime)
    {
        player.Heal(2);
        frameCounter = 0; // 重置计数器
    }
    else
    {
        frameCounter++;
    }

    // 更新帧计数器
    player.GetModPlayer<StarSetBonusPlayer>().frameCounter = frameCounter;
        }
    }

    public class StarSetBonusPlayer : ModPlayer
    {
        private float liferegenpiece=300;
        public bool lifeRegenDebuff;
        public int frameCounter; // 帧计数器

        public override void ResetEffects()
        {
            lifeRegenDebuff = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (lifeRegenDebuff)
            {
                // 确保没有正面的生命再生效果
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                // 防止生命再生时间累积
                Player.lifeRegenTime=0;

            }
        }

        public void RestoreLifeRegenTime()
        {
            // 将生命再生时间设置为 1800
            Player.lifeRegenTime = 1800f;
        }
    }
}