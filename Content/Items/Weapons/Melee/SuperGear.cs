using System;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class SuperGear : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(80, 100);
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.MeleeProj.SuperGearProjectile>();
            Item.shootSpeed = 14f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // 基于玩家近战攻速动态设置局部无敌帧
            // 获取玩家的近战攻击速度，基准值为1f
            float meleeAttackSpeed = player.GetTotalAttackSpeed(DamageClass.Melee);
            
            // 攻速越快(数值越大)，局部无敌帧越少
            // 公式：localNPCHitCooldown = max(1, 12 / meleeAttackSpeed)
            // 基准情况下(攻速为1f)，localNPCHitCooldown为12
            // 当攻速为2f时，localNPCHitCooldown为6
            // 当攻速为3f时，localNPCHitCooldown为4
            int localImmunityFrames = Math.Max(1, (int)(12f / meleeAttackSpeed));
            Main.projectile[proj].localNPCHitCooldown = localImmunityFrames;
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Cog, 50)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}