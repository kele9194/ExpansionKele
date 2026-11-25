using ExpansionKele.Content.Audio;
using ExpansionKele.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 本体形态的能量剑弧光。
    /// 贴图与挥舞逻辑全部来自 <see cref="EnergySwordProjectile"/>，这里只改配色与视线检查距离。
    /// </summary>
    public class TomorrowsEdgeSlash : EnergySwordProjectile
    {
        public override string Texture => "ExpansionKele/Content/Projectiles/MeleeProj/SpectreSwordProjectile";

        protected override float TextureScaleMultiplier => 1.25f;

        protected override Color backDarkColor => new Color(0x26, 0x48, 0xF5);
        protected override Color middleMediumColor => new Color(0x87, 0xA1, 0xFF);
        protected override Color frontLightColor => new Color(0xD3, 0xE2, 0xFF);

        public override void SetDefaults()
        {
            base.SetDefaults();

            // 本体能量剑不能穿墙：ownerHitCheck 会做视线检查，攻击范围之外即使可见也不判定。
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = TomorrowsEdgePlayer.AttackRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 基类负责粒子与击退方向，这里只补一声命中音。
            base.OnHitNPC(target, hit, damageDone);
            SoundEngine.PlaySound(ExpansionKeleSounds.EdgeBaseHit, target.Center);
        }
    }
}
