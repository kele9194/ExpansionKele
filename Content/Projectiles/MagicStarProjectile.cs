using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using ExpansionKele.Content.Customs;
using Terraria.WorldBuilding;
using ExpansionKele.Content.Buff;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Projectiles{

public static class Data{
    public const int BaseLife=2;
    public const int BaseMana=12;
    public const float BasePreLife=0.02f;
    public const float BasePreMana=0.15f;
    public const float BaseWingTime=0.1f;
    //public const float BaseMoveSpeed=1.15f;  
    public const int lifeRegenTime=120;
    public const float maxTrackingDistance=640f;
    public static readonly int buffDuration=65;
    public static readonly float redDamageBonus=2f;

    public const float StarDamageBonus=5f;

    public const float PreTargetLife100=0.25f;
    public const float PreTargetLifeMax100=0.25f;
    


}

public class MagicBlueProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
    }

    public override void AI()
    {
        // 定义最大追踪距离
        float maxTrackingDistance = Data.maxTrackingDistance; // 你可以根据需要调整这个值

        // 定义追踪速度和平滑度
        float speed = 30f;
        float turnResistance = 10f; // 调整这个值以改变追踪的平滑度
        Vector2 mousePosition = Main.MouseWorld;

        // 追踪目标
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed,maxTrackingDistance,turnResistance, mousePosition);
    }

     public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    target.immune[Projectile.owner] = 1;
    Player player = Main.player[Projectile.owner];
    int ManaUp=(int)(player.statMana+ Data.BaseMana +((player.statManaMax2 - player.statMana )* Data.BasePreMana));
    int ManaNow=player.statMana;
    player.statMana =Math.Min(ManaUp,player.statManaMax2);
    player.ManaEffect(player.statMana-ManaNow);
}
public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.immune[Projectile.owner] = 1;
            Player player = Main.player[Projectile.owner];
            player.AddBuff(ModContent.BuffType<BlueStarBuff>(), Data.buffDuration);
        }
    
    
}


public class MagicRedProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
    {
        // 定义最大追踪距离
        float maxTrackingDistance = Data.maxTrackingDistance; // 你可以根据需要调整这个值

        // 定义追踪速度和平滑度
        float speed = 30f;
        float turnResistance = 10f; // 调整这个值以改变追踪的平滑度
        Vector2 mousePosition = Main.MouseWorld;

        // 追踪目标
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed,maxTrackingDistance,turnResistance, mousePosition);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    target.immune[Projectile.owner] = 1;
    Player player = Main.player[Projectile.owner];
    player.AddBuff(ModContent.BuffType<RedStarBuff>(), Data.buffDuration);
    target.AddBuff(ModContent.BuffType<ArmorPodwered>(), Data.buffDuration*4);
    
}


    

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
          modifiers.FinalDamage *= Data.redDamageBonus;
        }
    }

    public class MagicPurpleProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
    {
        // 定义最大追踪距离
        float maxTrackingDistance = Data.maxTrackingDistance; // 你可以根据需要调整这个值

        // 定义追踪速度和平滑度
        float speed = 30f;
        float turnResistance = 10f; // 调整这个值以改变追踪的平滑度
        Vector2 mousePosition = Main.MouseWorld;

        // 追踪目标
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed,maxTrackingDistance,turnResistance, mousePosition);
    }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    target.immune[Projectile.owner] = 1;
    Player player = Main.player[Projectile.owner];
    int LifeUp=(int)(player.statLife+ Data.BaseLife +((player.statLifeMax2 - player.statLife )* Data.BasePreLife));
    int LifeNow=player.statLife;
    player.statLife =Math.Min(LifeUp,player.statLifeMax2);
    player.HealEffect(player.statLife-LifeNow);
}

public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
          target.immune[Projectile.owner] = 1;
            Player player = Main.player[Projectile.owner];
            player.AddBuff(ModContent.BuffType<PurpleStarBuff>(), Data.buffDuration);
        }

    }


public class MagicCyanProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
    {
        // 定义最大追踪距离
        float maxTrackingDistance = Data.maxTrackingDistance; // 你可以根据需要调整这个值

        // 定义追踪速度和平滑度
        float speed = 30f;
        float turnResistance = 10f; // 调整这个值以改变追踪的平滑度
        Vector2 mousePosition = Main.MouseWorld;

        // 追踪目标
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed,maxTrackingDistance,turnResistance, mousePosition);
    }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    
    target.immune[Projectile.owner] = 1;
    Player player = Main.player[Projectile.owner];
    player.wingTime+=Data.BaseWingTime;
    
    player.lifeRegenTime+=Data.lifeRegenTime;

    
    player.AddBuff(ModContent.BuffType<CyanStarBuff>(), Data.buffDuration);
    
}

public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float extraDamage = target.life * Data.PreTargetLife100/100f;
            modifiers.FlatBonusDamage+=extraDamage;
        }

    }


        public class MagicStarProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
    {
        // 定义最大追踪距离
        float maxTrackingDistance = Data.maxTrackingDistance; // 你可以根据需要调整这个值

        // 定义追踪速度和平滑度
        float speed = 30f;
        float turnResistance = 10f; // 调整这个值以改变追踪的平滑度
        Vector2 mousePosition = Main.MouseWorld;

        // 追踪目标
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed,maxTrackingDistance,turnResistance, mousePosition);
    }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    target.immune[Projectile.owner] = 1;
    Player player = Main.player[Projectile.owner];
    player.wingTime+=Data.BaseWingTime;
    
    player.lifeRegenTime+=Data.lifeRegenTime;
    
    int LifeUp=(int)(player.statLife+ Data.BaseLife +((player.statLifeMax2 - player.statLife )* Data.BasePreLife));
    int LifeNow=player.statLife;
    player.statLife =Math.Min(LifeUp,player.statLifeMax2);
    player.HealEffect(player.statLife-LifeNow);

    
    int ManaUp=(int)(player.statMana+ Data.BaseMana +((player.statManaMax2 - player.statMana )* Data.BasePreMana));
    int ManaNow=player.statMana;
    player.statMana =Math.Min(ManaUp,player.statManaMax2);
    player.ManaEffect(player.statMana-ManaNow);

    player.AddBuff(ModContent.BuffType<CyanStarBuff>(), Data.buffDuration);
    player.AddBuff(ModContent.BuffType<BlueStarBuff>(), Data.buffDuration);
    player.AddBuff(ModContent.BuffType<RedStarBuff>(), Data.buffDuration);
    player.AddBuff(ModContent.BuffType<PurpleStarBuff>(), Data.buffDuration);
    target.AddBuff(ModContent.BuffType<ArmorPodwered>(), Data.buffDuration*4);

    
}

public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage*=Data.StarDamageBonus;
            float extraDamage = target.lifeMax * Data.PreTargetLifeMax100/100f;
            modifiers.FlatBonusDamage+=extraDamage;
            
        }
    }






    public class MagicStarProjectileLower : ModProjectile
    {
        private Vector2 InitialVelocity;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 140; // 最大时间
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    target.immune[Projectile.owner] = 1;
    Player player = Main.player[Projectile.owner];
    player.wingTime+=Data.BaseWingTime;
    
    player.lifeRegenTime+=Data.lifeRegenTime;
    
    int LifeUp=(int)(player.statLife+ Data.BaseLife/2 +((player.statLifeMax2 - player.statLife )* Data.BasePreLife/2));
    int LifeNow=player.statLife;
    player.statLife =Math.Min(LifeUp,player.statLifeMax2);
    player.HealEffect(player.statLife-LifeNow);

    
    int ManaUp=(int)(player.statMana+ Data.BaseMana/4 +((player.statManaMax2 - player.statMana )* Data.BasePreMana/4));
    int ManaNow=player.statMana;
    player.statMana =Math.Min(ManaUp,player.statManaMax2);
    player.ManaEffect(player.statMana-ManaNow);

    player.AddBuff(ModContent.BuffType<CyanStarBuff>(), Data.buffDuration);
    player.AddBuff(ModContent.BuffType<BlueStarBuff>(), Data.buffDuration);
    player.AddBuff(ModContent.BuffType<RedStarBuff>(), Data.buffDuration);
    player.AddBuff(ModContent.BuffType<PurpleStarBuff>(), Data.buffDuration);
    target.AddBuff(ModContent.BuffType<ArmorPodwered>(), Data.buffDuration*4);

    
}
public override void OnSpawn(IEntitySource source)
{
    // 存储初始速度
    InitialVelocity = Projectile.velocity;
}

        public override void AI()
        {
            float time=80f;
            float Pertime=40f;
            float timerun=time;
            
            //按 e^(-t) 减速
            if(Projectile.timeLeft<=timerun){
            float t = (time-Projectile.timeLeft ) / Pertime;
            float speedMultiplier = (float)Math.Exp(-t)-(float)Math.Exp(-time/Pertime)/time*(time-Projectile.timeLeft) ;
            Projectile.velocity = InitialVelocity*speedMultiplier;
            }

            // // 滞留范围为320像素至640像素
            

            // // 如果超过最大时间，移除射弹
            // if (Projectile.timeLeft <= 0)
            // {
            //     Projectile.Kill();
            // }
        }

        

    
    

}
}





