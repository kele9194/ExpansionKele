using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele;
using static Terraria.NPC;
using ExpansionKele.Content.Customs;
using ExpansionKele.Commons; // 添加引用

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class CodeChaos : ModItem
    {
        // 当前伤害类型 0=近战, 1=远程, 2=魔法, 3=召唤
        const int meleeDamageType = 3;
        const int rangedDamageType = (meleeDamageType + 1)%4;
        const int magicDamageType = (rangedDamageType + 1)%4;
        const int summonDamageType = (magicDamageType + 1)%4;
        private int currentDamageType = meleeDamageType;
        
        // 浮动参数
        private float damageMultiplier = 1f;
        private float critDamageMultiplier = 2f;

        private float critChanceMultiplier = 1f;
        private float useTimeMultiplier = 1f;
        
        public override string LocalizationCategory => "Items.Weapons";

        private int GetRandomValidProjectileType()
        {
            // 使用新的分类系统获取对应类型的抛射体
            DamageType damageType = GetCurrentDamageType();
            return ProjectileClassificationManager.GetRandomProjectileByDamageType(damageType);
        }

        // 根据当前伤害类型索引获取DamageType枚举
        private DamageType GetCurrentDamageType()
        {
            return currentDamageType switch
            {
                0 => DamageType.Melee,
                1 => DamageType.Ranged,
                2 => DamageType.Magic,
                3 => DamageType.Summon,
                _ => DamageType.Melee
            };
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(270,320);
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;
            Item.crit = 4;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = false;
            Item.noUseGraphic = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, HitInfo hit, int damageDone)
        {
            target.life -= damageDone * 3;
            if (target.life < 0)
            {
                target.life = 0;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            SwitchDamageType();
            UpdateFloatParameters();
            
            int newType = GetRandomValidProjectileType();
            if (newType > 0 && ContentSamples.ProjectilesByType.ContainsKey(newType))
            {
                type = newType;
            }
            else
            {
                type = ProjectileID.Bullet;
            }
        }

        private void UpdateFloatParameters()
        {
            float t1 = (float)Main.rand.NextDouble();
            float t2 = (float)Main.rand.NextDouble();
            float t3 = (float)Main.rand.NextDouble();
            float t4 = (float)Main.rand.NextDouble();
            
            damageMultiplier = (float)Math.Pow(2.0, (double)(2f * t1 - 1f));
            critDamageMultiplier = 1.5f + t2 * 1f;
            critChanceMultiplier = 0.75f + t3 * 0.5f;
            useTimeMultiplier = 0.75f + t4 * 0.5f;
            
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            switch (currentDamageType)
            {
                case 3:
                    damage = damage.CombineWith(player.GetDamage(DamageClass.Melee).Scale(3f));
                    break;
                case 0:
                    damage = damage.CombineWith(player.GetDamage(DamageClass.Ranged).Scale(3f));
                    break;
                case 1:
                    damage = damage.CombineWith(player.GetDamage(DamageClass.Magic).Scale(3f));
                    break;
                case 2:
                    damage = damage.CombineWith(player.GetDamage(DamageClass.Summon).Scale(3f));
                    break;
            }
            damage = damage.CombineWith(new StatModifier(1f, damageMultiplier, 0f, 0f));
        }

        public override bool CanUseItem(Player player)
        {
            // 切换到下一个伤害类型
            currentDamageType = (currentDamageType + 1) % 4;
            Item.useTime = (int)(20 * useTimeMultiplier);
            Item.useAnimation = (int)(20 * useTimeMultiplier);
            SwitchDamageType();
            return base.CanUseItem(player);
        }

        private void SwitchDamageType()
        {
            // 所有伤害类型使用相同的设置
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = false;
            Item.noUseGraphic = false;
            
            switch (currentDamageType)
            {
                case meleeDamageType:
                    Item.DamageType = DamageClass.Melee;
                    break;
                case rangedDamageType:
                    Item.DamageType = DamageClass.Ranged;
                    break;
                case magicDamageType:
                    Item.DamageType = DamageClass.Magic;
                    break;
                case summonDamageType:
                    Item.DamageType = DamageClass.Summon;
                    break;
            }
        }
        
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(player, target, ref modifiers);
            // 应用暴击伤害浮动
            modifiers.CritDamage *= critDamageMultiplier/2;
        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit *= critChanceMultiplier;
        }
    }
}