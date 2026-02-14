using System;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class TangDynastySwordProjectile : EnergySwordProjectile
    {
        protected override float TextureScaleMultiplier => 1f; // 标准大小

        // 唐朝横刀的颜色定义 - 金色调体现唐朝盛世
        protected override Color backDarkColor => new Color(0xB8, 0x86, 0x0B); // 深金色
        protected override Color middleMediumColor => new Color(0xFF, 0xD7, 0x00); // 中等金色
        protected override Color frontLightColor => new Color(0xFF, 0xFF, 0xE0); // 浅金色

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // 穿透3个目标
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            
            var player = Main.player[Projectile.owner];
            var swordPlayer = player.GetModPlayer<SwordEnergyPlayer>();
            
            // 处理剑气收集逻辑
            swordPlayer.HandleSwordHit();
        }
    }
    public class SwordEnergyPlayer : ModPlayer
    {
        // 剑气能量相关字段
        public int swordEnergy = 0;          // 当前剑气能量值
        public const int MaxSwordEnergy = 100; // 剑气能量上限
        public bool usingTangSword = false;   // 是否正在使用唐横刀
        
        // 击中计数相关字段
        private int hitCount = 0;            // 当前挥击的击中次数
        private bool isFirstHit = true;      // 是否为首次击中

        public override void ResetEffects()
        {
            // 每帧重置状态
            usingTangSword = false;
        }

        public override void PostUpdate()
        {
        }

        /// <summary>
        /// 处理剑气收集逻辑
        /// 每次挥击首次命中敌人收集5点剑气能量，后续命中每个敌人+1点剑气
        /// </summary>
        public void HandleSwordHit()
        {
            if (!usingTangSword) return;

            if (isFirstHit)
            {
                // 首次命中收集5点剑气
                AddSwordEnergy(5);
                isFirstHit = false;
                hitCount = 1;
            }
            else
            {
                // 后续命中每个敌人+1点剑气
                AddSwordEnergy(1);
                hitCount++;
            }
        }

        /// <summary>
        /// 增加剑气能量
        /// </summary>
        /// <param name="amount">增加的能量值</param>
        public void AddSwordEnergy(int amount)
        {
            swordEnergy = Math.Min(swordEnergy + amount, MaxSwordEnergy);
        }

        /// <summary>
        /// 消耗剑气能量
        /// </summary>
        /// <param name="amount">消耗的能量值</param>
        /// <returns>是否成功消耗</returns>
        public bool ConsumeSwordEnergy(int amount)
        {
            if (swordEnergy >= amount)
            {
                swordEnergy -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前剑气能量百分比
        /// </summary>
        /// <returns>0.0到1.0之间的数值</returns>

        /// <summary>
        /// 重置当前挥击的击中计数器
        /// </summary>
        public void ResetHitCounter()
        {
            hitCount = 0;
            isFirstHit = true;
        }

        /// <summary>
        /// 检查是否有足够的剑气能量
        /// </summary>
        /// <param name="amount">需要的能量值</param>
        /// <returns>是否足够</returns>
        public bool HasEnoughEnergy(int amount)
        {
            return swordEnergy >= amount;
        }
    }
}