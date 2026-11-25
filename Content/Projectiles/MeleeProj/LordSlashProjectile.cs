using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 领主剑气：由 <see cref="Items.ArknightsItem.Accessories.Guard.LordChip"/> 在挥动近战武器时发射。
    /// <para/>
    /// 直线飞行、命中一个目标即消散（<c>penetrate = 1</c>），自带 20 点固定穿甲，
    /// 命中后附加 5 秒「着火了！」。整体走橙红色调。
    /// </summary>
    public class LordSlashProjectile : ModProjectile
    {
        /// <summary>剑气自带的固定穿甲，装备档与收藏档一致。</summary>
        public const int ArmorPenetration = 20;

        /// <summary>命中后附加「着火了！」的持续时间（帧）：5 秒。</summary>
        public const int OnFireDuration = 300;

        /// <summary>剑气主色（橙红）。</summary>
        private static readonly Color TintColor = new Color(255, 130, 60);

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;      // 不穿透
            Projectile.timeLeft = 60;      // 速度 14、不减速 ⇒ 射程约 840px
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.light = 0.35f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration += ArmorPenetration;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 减益放在 OnHitNPC 而不是 ModifyHitNPC：后者是伤害计算阶段，可能被重算多次。
            target.AddBuff(BuffID.OnFire, OnFireDuration);
        }

        public override void AI()
        {
            // 贴图沿用 RAc 那张 36×36 的斜向图（基准朝向右上），
            // 所以对齐飞行方向要减 45°；写成 +45° 会让剑气整体歪 90°。
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, 1.0f, 0.5f, 0.2f);

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 只改绘制色调，不动贴图本身；保留环境光的明暗影响。
            lightColor = TintColor * (lightColor.A / 255f);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.FlameBurst,
                    Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    100, default, 1.1f);
                dust.noGravity = true;
            }
        }
    }
}
