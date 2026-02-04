using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles
{

public class FullMoonArrowProj : ModProjectile
{
    private enum ArrowState
    {
        Forward, // 前进阶段
        Returning // 返回阶段
    }

    private const int MaxPenetrate = 4;
    private const int LifeTime = 400; // ticks

    private const int ForwardLifeTime=LifeTime/2;
    private const float ReturnSpeedMultiplier = 1f;

    private const float ReturningDamageMultiplier = 0.6f; // 返回时的伤害比例
    
    private ArrowState _state = ArrowState.Forward;
    private List<int> _hitNPCs = new List<int>();
    private int _penetrateCount = 0;
    private int _lifeTimer = 0;
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
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.penetrate = -1; // 无限穿透，但我们自己控制
        Projectile.timeLeft = LifeTime;
        Projectile.light = 1f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.extraUpdates = 0;
        Projectile.usesLocalNPCImmunity = true;  // 使用本地无敌帧系统
        Projectile.localNPCHitCooldown = 20; 
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        
        switch (_state)
        {
            case ArrowState.Forward:
                HandleForwardMotion(owner);
                break;
                
            case ArrowState.Returning:
                HandleReturningMotion(owner);
                break;
        }
        
        // 添加发光效果
        Lighting.AddLight(Projectile.Center, Color.Yellow.ToVector3() * 0.75f);
        
        // 旋转箭矢朝向移动方向
        if (Projectile.velocity != Vector2.Zero)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }

    private void HandleForwardMotion(Player owner)
    {
        // 增加生命计时器
        _lifeTimer++;
        
        // 如果超过生命周期或已穿透3个敌人，则开始返回
        if (_lifeTimer >= ForwardLifeTime || _penetrateCount >= MaxPenetrate)
        {
            _state = ArrowState.Returning;
            _hitNPCs.Clear();
            Projectile.penetrate = -1; // 允许无限穿透返回
            return;
        }
        
        // 让箭有一个自然的弧度（模拟重力）
        Projectile.velocity.Y += 0.04f; // 调整这个值可以改变弹道弯曲程度
        
        // 添加轨迹粒子效果
        if (Main.rand.NextBool(3))
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.GoldFlame, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, 
                default(Color), 1.2f);
            dust.noGravity = true;
        }
    }

    private void HandleReturningMotion(Player owner)
{
    // 设置 tileCollide 为 false，使箭矢在返回时不与地形碰撞
    Projectile.tileCollide = false;

    // 计算方向到玩家
    Vector2 directionToPlayer = owner.Center - Projectile.Center;
    float distance = directionToPlayer.Length();
    
    // 如果距离很近，直接销毁
    if (distance < 16f)
    {
        Projectile.Kill();
        return;
    }
    
    // 设置速度朝向玩家并加速
    directionToPlayer.Normalize();
    Projectile.velocity = directionToPlayer * (Projectile.velocity.Length() * ReturnSpeedMultiplier);
    
    // 移除穿透限制
    Projectile.penetrate = -1;
    
    // 添加返回时的特效
    if (Main.rand.NextBool(2))
    {
        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
            DustID.BlueFlare, -Projectile.velocity.X * 0.5f, -Projectile.velocity.Y * 0.5f, 
            100, default(Color), 1.5f);
        dust.noGravity = true;
        dust.scale = 1.2f;
    }
}

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // 碰撞到瓷砖时开始返回
        if (_state == ArrowState.Forward)
        {
            _state = ArrowState.Returning;
            _hitNPCs.Clear();
            Projectile.penetrate = -1; // 允许无限穿透返回
            
            // 可选：添加碰撞效果音效
            SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
            
            return false; // 不再处理默认的反弹行为
        }
        
        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
        // // 防止对同一NPC连续攻击
        // if (_hitNPCs.Contains(target.whoAmI) && _state == ArrowState.Forward)
        // {
        //     // 如果已经击中过这个NPC且在前进阶段，不造成伤害
        //     return;
        // }
        
        // // 添加NPC到已击中列表
        modifiers.ArmorPenetration+=15;
        if (_state == ArrowState.Forward)
        {
            _hitNPCs.Add(target.whoAmI);
            _penetrateCount++;
        }
        
        // // 在返回阶段不造成伤害
        // if (_state == ArrowState.Returning)
        // {
        //     damage = 0;
        // }

        else if (_state == ArrowState.Returning)
    {
        // 返回阶段：伤害变为原来的 60%
        modifiers.SourceDamage *= ReturningDamageMultiplier;
    }
    }

    public override bool PreDraw(ref Color lightColor)
{
    // 获取箭矢纹理
    Texture2D texture = _cachedTexture.Value;
    Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

    // 判断当前状态，调整颜色透明度
    Color drawColor = lightColor;

    if (_state == ArrowState.Returning)
    {
        drawColor = new Color(drawColor.R, drawColor.G, drawColor.B, (byte)(drawColor.A * 0.6f));
    }

    // 绘制拖尾效果
    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

    for (int k = 0; k < Projectile.oldPos.Length; k++)
    {
        Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin;
        Color trailColor = drawColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
        Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
    }

    Main.spriteBatch.End();
    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

    return true;
}
}

}