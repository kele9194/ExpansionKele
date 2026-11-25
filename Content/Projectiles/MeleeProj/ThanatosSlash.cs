using ExpansionKele.Content.Audio;
using ExpansionKele.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 一阶段「塔纳托斯」的斩击：直接生成在目标身上、没有飞行过程。
    /// 命中时伤害 +50%，并通过 <c>ScalingArmorPenetration = 1</c> 完全无视防御。
    /// </summary>
    public class ThanatosSlash : ModProjectile
    {
        /// <summary>存活帧数，只用于表现（14 帧玩家嫌磨蹭，缩到 9）。</summary>
        public const int LifeTime = 9;

        /// <summary>剑影基础缩放（约为原 1.6 的三分之一）。</summary>
        private const float SlashScale = 0.55f;

        /// <summary>剑影固定朝向（度）：0 = 贴图原角度。</summary>
        private const float SlashRotationDegrees = 105f;

        /// <summary>绘制用弧度值（const 不能调用 MathHelper.ToRadians，故单独缓存）。</summary>
        private static readonly float SlashRotation = MathHelper.ToRadians(SlashRotationDegrees);

        // 借 ThoughtsCrossBladeMiniProjectile 的剑影贴图做临时表现。
        public override string Texture => "ExpansionKele/Content/Projectiles/MeleeProj/ThoughtsCrossBladeMiniProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 不能设为 1：命中即 Kill，视觉只存在 1 帧
            Projectile.timeLeft = LifeTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = TomorrowsEdgePlayer.AttackRadius;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            // 跟着被标记的目标走；目标死了就停在原地淡出。
            int targetIndex = (int)Projectile.ai[1];
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active)
                {
                    Projectile.Center = target.Center;
                }
            }

            // 首帧在目标身上炸开一圈冲刺感。
            if (Projectile.localAI[0]++ == 0f)
            {
                // 多目标时同一帧会起好几个实例，MaxInstances 在 SoundStyle 里留了余量。
                SoundEngine.PlaySound(ExpansionKeleSounds.EdgeStageOneSlash, Projectile.Center);

                for (int i = 0; i < 6; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, Main.rand.NextVector2Circular(5f, 5f), 100, new Color(0x87, 0xA1, 0xFF), 1.1f);
                    dust.noGravity = true;
                }
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            // 只结算被选中的那一个目标，避免判定框波及旁边的敌人。
            return target.whoAmI == (int)Projectile.ai[1];
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // ai[0] 是本次斩击的伤害倍率：一阶段普攻 2.25，替身结束的归宿斩击 6.0。
            // 为 0 时兑底一阶段的值，避免旧存档/异常传入把伤害抹平。
            float multiplier = Projectile.ai[0] > 0f ? Projectile.ai[0] : TomorrowsEdge.ThanatosDamageMultiplier;
            modifiers.SourceDamage *= multiplier;
            modifiers.ScalingArmorPenetration += 1f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float progress = 1f - Projectile.timeLeft / (float)LifeTime;
            float fade = 1f - progress;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            // 随淡出略微放大，模拟斩击扩散。
            float scale = MathHelper.Lerp(SlashScale * 0.8f, SlashScale * 1.15f, progress);

            Color blade = new Color(0x26, 0x48, 0xF5) * fade;
            Color core = Color.White * (fade * 0.7f);

            // 单道剑影 + 同角度亮芯，朝向固定。
            // 曾经画三道交叉并随 ai[0] 随机旋转，实测既过密又看不出劈砍方向。
            Main.EntitySpriteDraw(texture, center, null, blade, SlashRotation, origin, scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, center, null, core, SlashRotation, origin, scale * 0.55f, SpriteEffects.None, 0);

            return false;
        }
    }
}
