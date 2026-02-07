using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class ShadowCrossbow : BaseCrossbow
    {
        // ShadowCrossbow特有的蓄力时间

        protected override int GetBaseDamage()
        {
            return ExpansionKele.ATKTool(32, 40);
        }

        protected override float GetKnockback()
        {
            return 2f;
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<ShadowCrossbowPlayer>().holdingShadowCrossbow = true;
        }



        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 5)
                .AddIngredient(ItemID.DemoniteBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class ShadowCrossbowPlayer : ModPlayer
    {
        public bool holdingShadowCrossbow = false;

        public override void ResetEffects()
        {
            // 每帧重置标记
            holdingShadowCrossbow = false;
        }

        public override void PostUpdate()
        {
            // 如果玩家持有血弩，增加生命再生速度
            if (holdingShadowCrossbow)
            {
                Player.accRunSpeed *= 1.3f;  // 加速度提高150%
                Player.maxRunSpeed *= 1.3f;  // 最大速度提高30%
                Player.runAcceleration *= 2.5f;  // 跑步加速度提高150%
                Player.runSlowdown *= 2.5f;
            }
        }
    }
}