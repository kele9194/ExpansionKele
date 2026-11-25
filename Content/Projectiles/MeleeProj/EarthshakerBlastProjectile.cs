using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 撼地者的地裂冲击：近战命中点在主目标周围炸开的范围伤害。
    /// <para/>
    /// 通过 <c>ai0</c> 记录被近战直接命中的目标索引并在 <see cref="CanHitNPC"/> 里排除，
    /// 避免主目标同时吃到「近战本体 + 溅射」两份伤害。
    /// <para/>
    /// <c>usesIDStaticNPCImmunity</c> 让同一类型的所有冲击共享每个 NPC 的无敌计时，
    /// 因此连续挥动不会让同一个敌人被反复炸伤。
    /// </summary>
    public class EarthshakerBlastProjectile : ModProjectile
    {
        /// <summary>地裂半径（像素）。碰撞箱边长是它的两倍。</summary>
        public const int Radius = 120;

        /// <summary>存在总时长（帧）。</summary>
        public const int Lifetime = 12;

        /// <summary>参与命中判定的帧数（之后只做视觉）。</summary>
        public const int DamageFrames = 2;

        public override void SetDefaults()
        {
            Projectile.width = Radius * 2;
            Projectile.height = Radius * 2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;      // 无限穿透：范围内全部命中
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        /// <summary>只有最初几帧参与判定，之后纯视觉。</summary>
        public override bool? CanDamage()
        {
            return Projectile.timeLeft > Lifetime - DamageFrames ? null : false;
        }

        /// <summary>排除被近战直接命中的主目标。</summary>
        public override bool? CanHitNPC(NPC target)
        {
            return target.whoAmI != (int)Projectile.ai[0];
        }

        public override void AI()
        {
            // 特效只放一次（localAI[0] 做闸），范围环由 PreDraw 绘制
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;

            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Lighting.AddLight(Projectile.Center, 0.9f, 0.7f, 0.4f);

            // 圆周撒尘 + 向外速度：让溅射范围一眼可见。
            float radius = Projectile.width * 0.5f;

            for (int i = 0; i < 20; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + offset,
                    DustID.GoldFlame,
                    offset.SafeNormalize(Vector2.Zero) * 2.4f,
                    0,
                    default,
                    1.3f);

                dust.noGravity = true;
                dust.fadeIn = 0.35f;
            }
        }

        /// <summary>淡棕色地裂圈，半径与判定框一致。</summary>
        public override bool PreDraw(ref Color lightColor)
        {
            float progress = 1f - Projectile.timeLeft / (float)Lifetime;

            AoeRingDrawer.Draw(
                Projectile.Center - Main.screenPosition,
                Radius,
                new Color(200, 165, 110),
                progress);

            return false;
        }
    }
}
