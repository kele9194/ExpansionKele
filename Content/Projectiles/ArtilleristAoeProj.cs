using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles
{
    /// <summary>
    /// 炮手芯片的溅射载体：原地炸开一圈，靠原版碰撞结算。
    /// <para/>
    /// 用弹幕而不是手动遍历 NPC，是为了让溅射伤害完整走 <c>ModifyHitByProjectile</c> 管线
    /// （其他芯片的破甲 / 暴击 / 大体型增伤都能作用在它身上），同时自动获得暴击、飘字与击杀归属。
    /// </summary>
    public class ArtilleristAoeProj : ModProjectile
    {
        /// <summary>存在总时长（帧）。</summary>
        public const int Lifetime = 12;

        /// <summary>参与命中判定的帧数（之后只做视觉）。</summary>
        public const int DamageFrames = 2;

        /// <summary>直径（像素）：128 半径 × 2。</summary>
        public const int Diameter = 256;

        public override void SetDefaults()
        {
            Projectile.width = Diameter;
            Projectile.height = Diameter;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 每个敌人只吃一次：这是一次性爆炸，不是持续伤害区。
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 继承源弹幕的伤害类型，避免以后给其他职业复用时刻死成 Ranged。
            if (source is EntitySource_Parent parent && parent.Entity is Projectile parentProjectile)
                Projectile.DamageType = parentProjectile.DamageType;
        }

        /// <summary>只有最初几帧参与判定，之后纯视觉。</summary>
        public override bool? CanDamage()
        {
            return Projectile.timeLeft > Lifetime - DamageFrames ? null : false;
        }

        /// <summary>
        /// 圆形碰撞：判定框是正方形，对角线比半径长 41%，
        /// 不裁剪的话四角会「隔空炸到」明显在圈外的目标。
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Projectile.width * 0.5f;

            float closestX = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
            float closestY = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);

            float dx = Projectile.Center.X - closestX;
            float dy = Projectile.Center.Y - closestY;

            return dx * dx + dy * dy <= radius * radius;
        }

        public override void AI()
        {
            // 原地不动，只是「范围判定 + 视觉」的载体。
            Projectile.velocity = Vector2.Zero;

            // 特效只放一次
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;

            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Lighting.AddLight(Projectile.Center, 0.9f, 0.5f, 0.2f);

            float radius = Projectile.width * 0.5f;

            for (int i = 0; i < 20; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + offset,
                    DustID.Torch,
                    offset.SafeNormalize(Vector2.Zero) * 2.4f,
                    0,
                    default,
                    1.3f);

                dust.noGravity = true;
                dust.fadeIn = 0.35f;
            }
        }

        /// <summary>
        /// 橙色范围圈。半径直接用判定半径（<c>Projectile.width * 0.5f</c>），
        /// 视觉与判定完全重合，不再依赖弹幕自带贴图的尺寸。
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            float progress = 1f - Projectile.timeLeft / (float)Lifetime;

            AoeRingDrawer.Draw(
                Projectile.Center - Main.screenPosition,
                Projectile.width * 0.5f,
                new Color(255, 140, 80),
                progress);

            return false;
        }
    }
}
