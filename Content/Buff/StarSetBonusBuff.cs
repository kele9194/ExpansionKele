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

        
        private float AdditiveGenericDamageBonus = 0.1f;

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

        // ... existing code ...
    public override void Update(Player player, ref int buffIndex)
    {
        ExpansionKeleTool.MultiplyDamageBonus(player,1.15f);
        // 计算基于生命值的伤害提升
                float lifePercentage = player.statLife / (float)player.statLifeMax2;
                if (lifePercentage > 1)
                {
                    lifePercentage = 1;
                }
                
                // 计算伤害提升幅度
                float alphaDamageBoost = (1 / (lifePercentage + a1)) - (1 / (1 + a1));
                float damageBoost = (alphaDamageBoost + 1) * extraDamage;
                player.GetDamage<GenericDamageClass>() += damageBoost;
                // 应用增伤效果
        // 设置生命再生减益标志
        var starSetBonusPlayer = player.GetModPlayer<StarSetBonusPlayer>();
        starSetBonusPlayer.lifeRegenDebuff = true;
        
        // 传递 Buff 实例引用给 Player 类
        starSetBonusPlayer.activeBuff = this;
        
        // Deleted:// Deleted:
        // Deleted:float lifePercentage = player.statLife / (float)player.statLifeMax2;
        // Deleted:
        // Deleted:// 常数 a
        // Deleted:if (lifePercentage > 1){
        // Deleted:    lifePercentage = 1;
        // Deleted:}
        // Deleted:
        // Deleted:// 计算伤害提升幅度
        // Deleted:float alphaDamageBoost = (1 / (lifePercentage + a1)) - (1 / (1 + a1));
        // Deleted:
        // Deleted:float damageBoost = (alphaDamageBoost+1) * AdditiveGenericDamageBonus;
        // Deleted:// 应用增伤效果
        // Deleted:player.GetDamage<GenericDamageClass>() += damageBoost;
        // Deleted:			
        // Deleted:int frametime = (int)(extraTime * 12.5 / player.statLifeMax2 *2+ 0.5f);
        // Deleted:            
        // Deleted:        
        // Deleted:
        // Deleted:// 设置生命再生减益标志
        // Deleted:player.GetModPlayer<StarSetBonusPlayer>().lifeRegenDebuff = true;
        // Deleted:
        // Deleted:// 设置玩家盔甲颜色为微微的红色
        // Deleted:player.skinColor = new Color(255, 100, 100, 255);
        // Deleted:
        // Deleted:// 检查 buff 是否即将结束
        // Deleted:int frameCounter = player.GetModPlayer<StarSetBonusPlayer>().frameCounter;
        // Deleted:
        // Deleted:// 每 x 帧回复 1 血量
        // Deleted:if (frameCounter >= frametime)
        // Deleted:{
        // Deleted:    player.Heal(2);
        // Deleted:    frameCounter = 0; // 重置计数器
        // Deleted:}
        // Deleted:else
        // Deleted:{
        // Deleted:    frameCounter++;
        // Deleted:}
        // Deleted:
        // Deleted:// 更新帧计数器
        // Deleted:player.GetModPlayer<StarSetBonusPlayer>().frameCounter = frameCounter;
    }
// ... existing code ...
    public class StarSetBonusPlayer : ModPlayer
    {
        private float liferegenpiece=300;
        public bool lifeRegenDebuff;
        public int frameCounter; // 帧计数器
        
        // 保存当前激活的 Buff 实例引用
        public StarSetBonusBuff activeBuff;
        
        private float AdditiveGenericDamageBonus = 0.30f;
        private float a1 = 1.5f;

        public override void ResetEffects()
        {
            lifeRegenDebuff = false;
            activeBuff = null; // 重置时清除 Buff 引用
        }


        public override void PostUpdate()
        {
            if (lifeRegenDebuff && activeBuff != null)
            {
                
                
                
                
                
                // 计算回血帧间隔（使用 Buff 中的 extraTime）
                int frametime = (int)(activeBuff.extraTime * 12.5 / Player.statLifeMax2 * 2 + 0.5f);
                
                // 每 x 帧回复 2 血量
                if (frameCounter >= frametime)
                {
                    Player.Heal(2);
                    frameCounter = 0; // 重置计数器
                }
                else
                {
                    frameCounter++;
                }
                
                // 设置玩家盔甲颜色为微微的红色
                Player.skinColor = new Color(255, 100, 100, 255);
            }
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
// ... existing code ...
    }
}