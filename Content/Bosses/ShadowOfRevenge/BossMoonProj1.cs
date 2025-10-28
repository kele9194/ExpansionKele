using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Bosses.ShadowOfRevenge
{
    public class BossMoonProj1 : ModProjectile
    {
        public override string LocalizationCategory => "Bosses.ShadowOfRevenge.Projectiles";
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        // 直接设置Projectile造成防御损伤
        if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
        {
            calamity.Call("SetDefenseDamageProjectile", Projectile, true);
        }
    }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 7200;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // 获取关联的Boss
            NPC owner = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<ShadowOfRevenge>())
                {
                    owner = Main.npc[i];
                    break;
                }
            }

            if (owner == null || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            // 围绕Boss旋转
            float radius = 200f;
            float rotationSpeed = MathHelper.ToRadians(1f); // 每帧1度
            
            // 使用Projectile.ai[0]作为角度索引
            Projectile.ai[0] += rotationSpeed;
            
            // 使用Projectile.ai[1]作为Boss的索引
            // 不再使用Projectile.ai[1]作为半径，半径固定为200
            
            Vector2 offset = new Vector2(radius, 0).RotatedBy(Projectile.ai[0]);
            Projectile.Center = owner.Center + offset;
            
            // 面向旋转方向
            Projectile.rotation = Projectile.ai[0] + MathHelper.PiOver2;
        }
    }
}