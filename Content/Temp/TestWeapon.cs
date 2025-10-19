using System;
using System.Collections.Generic;
using ExpansionKele.Content.Items;
using ExpansionKele.Content.Weapons.Effects;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Temp
{
    public class TestWeapon : MentalOmegaItem
{
    public override string LocalizationCategory=>"Temp";
    protected override void SetOmegaDefaults()
        {
            SetCategory(ItemCategory.AirForce);
            SetTechnology(ItemTechnology.T0);
        }
    
    
    public override void SetDefaults()
    {
        // 原有SetDefaults实现
        //Item.SetNameOverride("测试武器");
        Item.damage = 100;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 4f;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item11;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<TestWeaponProjectile>(); // 修改为实际子弹类型
        Item.shootSpeed = 10f;
        base.SetDefaults();
    }
    public override void AddRecipes()
        {
            
        }
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            
        }
        public override void UpdateInventory(Player player)
        {
            AirForceBonus(player);
            base.UpdateInventory(player);
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
{
    return true;
}
    
    
public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                {"MoveSpeedBonus", $"测试用武器"},

			
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
        }

    }

public class TestWeaponProjectile : MentalOmegaProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.aiStyle = 0;
            Projectile.penetrate = 1;
            base.SetDefaults();
        }

        public override void AI()
        {
            // 自定义弹道逻辑可在此添加
        }

        protected override void SetOmegaDefaults()
        {
            SetAntiInfantry(OmegaLevel.Level1);
            SetAntiArmor(OmegaLevel.Level2);
            SetAntiBuilding(OmegaLevel.Level2);
            SetAntiAirForce(OmegaLevel.Level3);
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 使用简化版的MentalOmegaModifyHit方法
            MentalOmegaUtils.MentalOmegaModifyHit(this, target, ref modifiers);
            //MentalOmegaUtils.ModifyHitNPCAgainstArmor(target, ref modifiers, AntiArmor);
            //MentalOmegaUtils.ApplyBallisticSplash(target, ref modifiers,Projectile.direction, Projectile.damage, AntiArmor);
        }

        
    }


}