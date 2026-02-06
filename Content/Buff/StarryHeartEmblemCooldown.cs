using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class StarryHeartEmblemCooldown : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("星元之心冷却中");
            // Description.SetDefault("星元之心的致命保护正在冷却中");
            Main.buffNoTimeDisplay[Type] = false; // 显示剩余时间
            Main.debuff[Type] = true; // 这是一个debuff（负面效果）
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // 护士无法移除此debuff
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            
        }

        // 这个buff不需要更新玩家属性，只是一个标记buff
        // public override void Update(Player player, ref int buffIndex)
        // {
        //     // 不需要任何效果更新
        // }
    }
}