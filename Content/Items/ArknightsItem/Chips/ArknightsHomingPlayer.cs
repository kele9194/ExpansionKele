using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 处理刻印武器的制导开关（isHoming）切换。
    /// 按 J 键切换当前手持刻印武器的制导状态。
    /// 不同职业的制导效果由各自的 GlobalProjectile 实现。
    /// </summary>
    public class ArknightsHomingPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (!ExpansionKele.ArkHomingKeyBind.JustPressed)
                return;

            Item held = Player.HeldItem;
            if (held == null || held.IsAir) return;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0) return;

            SniperSubclass subclass = (SniperSubclass)arkNum;
            // ★ 白名单过滤：非白名单职业不能切换制导
            if (!ArknightsGlobalItem.CanSubclassHoming(subclass)) return;

            var arkItem = held.GetGlobalItem<ArknightsGlobalItem>();
            arkItem.isHoming = !arkItem.isHoming;

            string msg = arkItem.isHoming
                ? Language.GetTextValue("Mods.ExpansionKele.Items.Chips.HomingOn")
                : Language.GetTextValue("Mods.ExpansionKele.Items.Chips.HomingOff");

            Main.NewText(msg, arkItem.isHoming ? Color.Green : Color.Gray);
        }
    }
}