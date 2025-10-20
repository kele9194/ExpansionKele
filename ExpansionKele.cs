using System;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;
using ExpansionKele;
using Terraria;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Items.Weapons;

namespace ExpansionKele
{
    public class ExpansionKele : Mod
    {
        public static Mod calamity;
        public static ModKeybind StarKeyBind;
        public static ModKeybind TrackingKeyBind { get; private set; } // 正确位置：类成员
        public static object Content { get; internal set; }
        public static int[] projectileTypes;

        public static SoundStyle SniperSound = new SoundStyle("ExpansionKele/Content/Audio/SniperSound")
        {
            Volume = 0.3f,
            PitchVariance = 0.2f,
            MaxInstances = 3,
        };

        public static Texture2D sniperLaserTexture = ModContent.Request<Texture2D>("ExpansionKele/Content/StarySniper/SniperLaser").Value;

        public static SoundStyle GetSniperSound()
        {
            return SniperSound;
        }

        public override void Load()
        {
            
            if (ModLoader.HasMod("CalamityMod"))
            {
                calamity = ModLoader.GetMod("CalamityMod");
            }
            else
            {
                calamity = null;
            }

            StarKeyBind = KeybindLoader.RegisterKeybind(this, "StarBonusBuff", Keys.F);
            sniperLaserTexture = ModContent.Request<Texture2D>("ExpansionKele/Content/StarySniper/SniperLaser").Value;
            TrackingKeyBind = KeybindLoader.RegisterKeybind(this, "TrackingLocator", Keys.Y);
            projectileTypes = new int[]
            {
                ModContent.ProjectileType<AAMissile>(),
                ModContent.ProjectileType<ColaProjectile>(),
                ModContent.ProjectileType<ColaProjectileLower>(),
                ModContent.ProjectileType<FrostRayProjectile>(),
                ModContent.ProjectileType<FullMoonArrowProj>(),
                ModContent.ProjectileType<FullMoonEchoProj>(),
                ModContent.ProjectileType<FullMoonProjectile>(),
                ModContent.ProjectileType<IronCurtainCannonLaser>(),
                ModContent.ProjectileType<IronCurtainCannonProjectile>(),
                ModContent.ProjectileType<LifePercentageProjectile>(),
                ModContent.ProjectileType<MagicBlueProjectile>(),
                ModContent.ProjectileType<MagicCyanProjectile>(),
                ModContent.ProjectileType<MagicPurpleProjectile>(),
                ModContent.ProjectileType<MagicRedProjectile>(),
                ModContent.ProjectileType<MagicStarProjectile>(),
                ModContent.ProjectileType<NeutronProjectile>(),
                //ModContent.ProjectileType<ProtonCannonHoldOut>(),
                ModContent.ProjectileType<SharkyBullet>(),
                ModContent.ProjectileType<SharkyBulletPlus>(),
                //ModContent.ProjectileType<SoulCannonHoldOut>(),
                ModContent.ProjectileType<SpectralCurtainCannonProj>(),
                ModContent.ProjectileType<StingerProjectile>(),
                ModContent.ProjectileType<VortexMainProjectile>()
                //以后可以继续完善
            };

            

            base.Load();
            
        }

        public override void Unload()
        {
            base.Unload();
            StarKeyBind = null;
            sniperLaserTexture = null;
            TrackingKeyBind = null;
            // 其他卸载逻辑
        }
        
        /// <summary>
        /// 根据灾厄模组是否存在计算武器伤害
        /// 规则：
        /// 1. 没有灾厄模组：使用 nonCalamityDamage
        /// 2. 有灾厄模组：
        ///    a. 如果 calamityDamage 不是默认值(0)：使用 calamityDamage
        ///    b. 如果 calamityDamage 是默认值：使用 nonCalamityDamage * calamityMultiplier
        /// </summary>
        /// <param name="nonCalamityDamage">无灾厄模组时的武器伤害</param>
        /// <param name="calamityDamage">有灾厄模组时的武器伤害（默认为0）</param>
        /// <param name="calamityMultiplier">灾厄倍率（默认为1.25f）</param>
        /// <returns>根据条件计算出的最终伤害值</returns>
        // ... existing code ...
        const float DefaultMultiplier = 1.15f;
public static int ATKTool(int nonCalamityDamage = 0, int calamityDamage = 0, float calamityMultiplier = DefaultMultiplier)
{
    
    // 检查是否安装了灾厄模组
    if (ModLoader.HasMod("CalamityMod"))
    {
        
        // 如果安装了灾厄模组
        if (calamityDamage != 0)
        {
            // 如果指定了灾厄伤害值，使用该值
            return calamityDamage;
        }
        else if (nonCalamityDamage != 0)
        {
            // 如果指定了非灾厄伤害值，使用非灾厄伤害值
            return (int)(nonCalamityDamage*calamityMultiplier);
        }
        else 
        {
            // 如果两个伤害值都为0，则将第一个数设为100001
            return 100001;
        }
    }
    else
    {
        // 如果没有安装灾厄模组
        if (nonCalamityDamage != 0)
        {
            // 如果指定了非灾厄伤害值，使用该值
            return nonCalamityDamage;
        }
        else if (calamityDamage != 0)
        {
            // 如果只指定了灾厄伤害值，使用灾厄伤害值（因为没有灾厄模组，所以直接返回）
            return (int)(calamityDamage/calamityMultiplier);
        }
        else
        {
            // 如果两个伤害值都为0，则将第一个数设为100001
            return 100001;
        }
    }
}
        // public override object Call(params object[] args)
        // {
        //     // 委托给 ExpansionKeleCallHandler 处理
        //     return ExpansionKeleCallHandler.HandleCall(this, args);
        // }
// ... existing code ...
    }
}