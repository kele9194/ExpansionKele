using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles.EnergySword;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class FullMoonSwordProjectile : EnergySwordProjectile
    {
        protected override float TextureScaleMultiplier => 1.3f; // 紫色剑气大小为1.6f

        // 紫色系颜色定义
        protected override Color backDarkColor => new Color(0x03, 0x08, 0x0c); // 深紫色
        protected override Color middleMediumColor => new Color(0x19, 0x0c, 0x1e); // 中紫色（薰衣草紫）
        protected override Color frontLightColor => new Color(0x2c, 0x24, 0x4d); // 浅紫色（薰衣草白）

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
        }
    }
}