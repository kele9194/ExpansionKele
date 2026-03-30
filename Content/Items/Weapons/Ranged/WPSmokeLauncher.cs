using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class WPSmokeLauncher : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Ranged";

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(WPSmokeCluster.MAX_SHOTS)
        );
        public const int MAX_LOCK_DISTANCE = 200;
        private const int RELOAD_TIME = 60;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(80,120);
            Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 32;
            Item.useTime = RELOAD_TIME;
            Item.useAnimation = RELOAD_TIME;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<WPSmokeProjectile>();
            Item.shootSpeed = 10f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-40f, -6f);
        }

        public override bool CanUseItem(Player player)
        {
            return true;
        }

        // ... existing code ...

        // ... existing code ...

        // ... existing code ...

        // ... existing code ...

        // ... existing code ...

        // ... existing code ...

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 mousePos = Main.MouseWorld;
                NPC lockedTarget = null;
                
                bool mouseAbovePlayer = mousePos.Y < player.Center.Y;
                
                if (mouseAbovePlayer)
                {
                    float mouseRange = MAX_LOCK_DISTANCE;
                    
                    lockedTarget = FindFireControlTarget(player.Center, mousePos, mouseRange);
                }
                
                int clusterId = Projectile.NewProjectile(
                    source,
                    mousePos,
                    Vector2.Zero,
                    ModContent.ProjectileType<WPSmokeCluster>(),
                    damage,
                    knockback,
                    player.whoAmI
                );
                
                if (lockedTarget != null && clusterId >= 0)
                {
                    Main.projectile[clusterId].ai[0] = lockedTarget.whoAmI + 1;
                    Main.projectile[clusterId].netUpdate = true;
                }
            }
            
            return false;
        }

        private NPC FindFireControlTarget(Vector2 playerPosition, Vector2 mousePos, float mouseRange)
        {
            NPC bestTarget = null;
            float bestScore = float.MinValue;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.type == NPCID.TargetDummy || 
                    npc.immortal || npc.dontTakeDamage)
                    continue;
                
                float distanceToMouse = Vector2.Distance(mousePos, npc.Center);
                if (distanceToMouse > mouseRange)
                    continue;
                
                float score = 0f;
                
                if (npc.boss)
                {
                    score += 5000f;
                }
                
                score += npc.lifeMax * 0.5f;
                
                Vector2 directionToNpc = npc.Center - playerPosition;
                Vector2 directionToMouse = mousePos - playerPosition;
                float angleDiff = Math.Abs(directionToNpc.ToRotation() - directionToMouse.ToRotation());
                score += Math.Max(0, 100f - angleDiff * 20f);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = npc;
                }
            }
            
            return bestTarget;
        }

// ... existing code ...
// ... existing code ...


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 10);
            recipe.AddIngredient(ItemID.Grenade, 5);
            recipe.AddIngredient(ItemID.IllegalGunParts, 3);
            recipe.AddIngredient(ItemID.ExplosivePowder, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}