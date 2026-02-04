using ExpansionKele.Content.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class LaserCutterProjectile : ModProjectile
    {
        private const float PARALLEL_OFFSET = 16f; // 两道激光之间的距离
        private const float MAX_LASER_LENGTH = 900f; // 激光最大长度 - 修改为600像素，不再考虑阻挡
        
        // 添加用于绘制激光范围的变量
        private Vector2 laserStartPos;
        private Vector2 laserDirection;
        private static Asset<Texture2D> _cachedTexture;

        public override void Load()
        {
            // 预加载纹理资源
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }

        public override void Unload()
        {
            // 清理资源引用
            _cachedTexture = null;
        }
        
        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // 检查玩家是否仍在使用此武器
            if (!player.channel || player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<LaserCutter>())
            {
                Projectile.Kill();
                return;
            }

            // 更新武器使用时间以保持动画播放
            player.itemTime = 2;
            player.itemAnimation = 2;
            
            // 设置玩家朝向
            Vector2 mousePosition = Main.MouseWorld;
            Vector2 diff = mousePosition - player.Center;
            if (diff.X < 0)
            {
                player.direction = -1;
            }
            else
            {
                player.direction = 1;
            }
            
            // 设置玩家旋转
            Projectile.rotation = diff.ToRotation();
            
            // 发射两道平行激光 - 将起点设为玩家中心
            Vector2 direction = diff.SafeNormalize(Vector2.UnitX * player.direction);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // 左侧激光起点
            Vector2 laserStart1 = player.Center + perpendicular * PARALLEL_OFFSET / 2;
            // 右侧激光起点
            Vector2 laserStart2 = player.Center - perpendicular * PARALLEL_OFFSET / 2;
            
            // 计算激光终点 - 不再考虑阻挡，直接使用最大长度
            Vector2 laserEnd1 = player.Center + direction * MAX_LASER_LENGTH + perpendicular * PARALLEL_OFFSET / 2;
            Vector2 laserEnd2 = player.Center + direction * MAX_LASER_LENGTH - perpendicular * PARALLEL_OFFSET / 2;
            
            // 处理激光伤害 - 将激光起始点移到玩家中心
            Vector2 centerStart = player.Center;
            Vector2 centerEnd = player.Center + direction * MAX_LASER_LENGTH;
            ProcessLaserDamage(centerStart, centerEnd);
            
            // 创建激光视觉效果 - 显示两条平行激光
            CreateLaserVisual(laserStart1, laserEnd1, Color.Blue, 4f, player);
            CreateLaserVisual(laserStart2, laserEnd2, Color.Blue, 4f, player);
            
            // 保存激光起点和方向用于绘制
            laserStartPos = player.Center;
            laserDirection = direction;
        }

        private Vector2 CalculateLaserEndpoint(Vector2 startPos, Vector2 direction)
        {
            // 直接返回最大激光长度，不考虑阻挡
            return startPos + direction * MAX_LASER_LENGTH;
        }

        private void ProcessLaserDamage(Vector2 start, Vector2 end)
        {
            // 计算激光线段的方向和长度
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float length = Vector2.Distance(start, end);
            
            // 检查激光路径上的敌人
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    // 检查NPC是否在激光路径上
                    float distanceToLine;
                    Vector2 closestPoint = FindClosestPointOnLine(start, end, npc.Center, out distanceToLine);
                    
                    if (distanceToLine < npc.width / 2 + 10) // 如果NPC在激光范围内
                    {
                        // 在交叉点创建一个小抛射体来造成伤害，而不是直接调用StrikeNPC
                        Vector2 hitPosition = closestPoint;
                        
                        // 创建伤害抛射体
                        int damageProj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            hitPosition.X,
                            hitPosition.Y,
                            0f, // 速度为0
                            0f,
                            ModContent.ProjectileType<LaserDamageProjectile>(), // 新增的小伤害抛射体
                            Projectile.damage,
                            Projectile.knockBack,
                            Projectile.owner
                        );
                        
                        // 同步抛射体到所有客户端
                        if (Main.projectile[damageProj].active)
                        {
                            Main.projectile[damageProj].Center = hitPosition;
                        }
                        
                        // 减少击中特效的粒子数量
                        if (Main.rand.NextBool(2)) // 只有50%的机会产生特效
                        {
                            int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
                            Main.dust[dust].scale = 0.8f; // 缩短粒子存留时间通过减小尺寸
                            Main.dust[dust].velocity *= 0.3f; // 减小速度
                            Main.dust[dust].noGravity = true; // 添加重力属性以便更快消失
                        }
                        
                        // 减少播放音效的频率
                        if (Main.rand.NextBool(15)) // 从5改为15，降低音效频率
                        {
                            SoundEngine.PlaySound(SoundID.Item92, npc.Center);
                        }
                    }
                }
            }
        }

        // 找到线段上离点最近的点并返回距离
        private Vector2 FindClosestPointOnLine(Vector2 start, Vector2 end, Vector2 point, out float distance)
        {
            Vector2 segment = end - start;
            float segmentLengthSquared = segment.LengthSquared();
            
            if (segmentLengthSquared == 0)
            {
                distance = Vector2.Distance(point, start);
                return start;
            }
            
            float t = Math.Max(0, Math.Min(1, Vector2.Dot(point - start, segment) / segmentLengthSquared));
            Vector2 closestPoint = start + t * segment;
            distance = Vector2.Distance(point, closestPoint);
            
            return closestPoint;
        }

        // ... existing code ...
        // ... existing code ...
        private void CreateLaserVisual(Vector2 start, Vector2 end, Color color, float width, Player player)
        {
            // 在激光路径上创建尘埃效果，模拟激光视觉
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            float length = Vector2.Distance(start, end);
            
            // 从距离96处开始创建激光路径上的尘埃，间隔为24
            for (float i = 72f; i < length; i += 24f) // 修改间隔从32f到24f，并从距离96处开始生成
            {
                Vector2 dustPos = start + direction * i;
                
                // 添加一个小偏移量，让尘埃稍微向前移动一点
                Vector2 offset = direction * 8f; // 向激光方向前进8像素
                
                // 主激光束尘埃 - 使用新颜色 #34A8B4
                Dust laserDust = Dust.NewDustPerfect(dustPos + offset, DustID.Electric, null, 100, new Color(52, 168, 180), 0.8f); // 降低比例从1.2f到0.8f
                laserDust.noGravity = true;
                
                // 设置尘埃的速度使其沿激光方向缓慢移动
                laserDust.velocity = direction * 2f; // 给尘埃一个沿激光方向的速度
                
                // 只在每隔三个位置添加发光尘埃效果 - 调整以匹配新的间隔
                if ((int)((i - 72f) / 24f) % 3 == 0)
                {
                    Dust glowDust = Dust.NewDustPerfect(dustPos + offset, DustID.Electric, null, 100, new Color(100, 200, 220), 0.4f); // 降低比例从0.6f到0.4f
                    glowDust.noGravity = true;
                    
                    // 给发光尘埃同样的速度
                    glowDust.velocity = direction * 2f;
                }
            }
        }
// ... existing code ...
// ... existing code ...

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制激光范围纹理
            DrawLaserRange();
            return false; // 因为我们自己绘制了视觉效果，所以阻止默认绘制
        }

        private void DrawLaserRange()
        {
            // 检查是否存在激光起点和方向
            if (laserStartPos != Vector2.Zero && laserDirection != Vector2.Zero)
            {
                // 获取激光终点
                Vector2 laserEnd = CalculateLaserEndpoint(laserStartPos, laserDirection);
                
                // 获取纹理
                Texture2D texture =_cachedTexture.Value;
                
                // 计算激光的总长度
                float laserLength = Vector2.Distance(laserStartPos, laserEnd);
                
                // 绘制原点设置在纹理的左边缘中心，这样纹理会从起始点开始延伸
                Vector2 drawOrigin = new Vector2(0, texture.Width * 0.5f); // 从左边中心开始绘制
                
                // 计算缩放比例以适应实际激光长度
                Vector2 scale = new Vector2(laserLength / texture.Width, 1f);
                
                // 绘制激光范围纹理
                Main.spriteBatch.Draw(
                    texture, 
                    laserStartPos - Main.screenPosition, 
                    null, 
                    Color.White, // 半透明效果
                    laserDirection.ToRotation(), 
                    drawOrigin, 
                    scale, 
                    SpriteEffects.None, 
                    0
                );
            }
        }

        public override bool? CanDamage()
        {
            return false; // 伤害已在AI中手动处理
        }
    }

    // 新增激光伤害抛射体类
    public class LaserDamageProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2; // 单个像素大小
            Projectile.height = 2;
            Projectile.alpha = 255; // 完全透明
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1; // 穿透1次后消失
            Projectile.timeLeft = 2; // 2帧后自动消失
            Projectile.tileCollide = false; // 不与瓦片碰撞
            Projectile.ignoreWater = true;
            Projectile.hide = true; // 隐藏抛射体本身
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // 简单的AI - 保持静止
            Projectile.velocity = Vector2.Zero;
        }

        public override bool? CanDamage()
        {
            // 只在第一帧允许造成伤害
            return Projectile.timeLeft > 1;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration+=9999;
            modifiers.DefenseEffectiveness*=0f;
            base.ModifyHitNPC(target, ref modifiers);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 与瓦片碰撞时不反弹，直接消失
            return false;
        }
    }
}