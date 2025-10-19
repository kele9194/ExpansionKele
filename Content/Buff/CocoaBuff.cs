using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class CocoaBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("可可增益");
            // Description.SetDefault("提升自身移速20%，增加4点防御力，提升5%暴击率");
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = Language.GetText("Mods.ExpansionKele.Buff.CocoaBuff.Description").Format(20, 4, 5, 1);
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.2f; // 提升20%移速
            player.statDefense += 4; // 增加4点防御力
            player.GetCritChance<GenericDamageClass>() += 5; // 提升5%暴击率
            player.lifeRegen += 1;//玩家生命再生+1
        }
    }
}