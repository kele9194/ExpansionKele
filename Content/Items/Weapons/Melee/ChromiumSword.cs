using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class ChromiumSword : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(75, 110);
            Item.DamageType = DamageClass.Melee;
            Item.width = 72;
            Item.height = 72;
            Item.useTime = 27;
            Item.useAnimation = 27;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            
            // 修改为能量剑模式
            Item.shoot = ModContent.ProjectileType<ChromiumSwordProjectile>();
            Item.shootsEveryUse = true;
            Item.noMelee = true; // 禁用直接接触伤害，使用弹幕代替
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float adjustedItemScale = player.GetAdjustedItemScale(Item); // Get the melee scale of the player and item.
			Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
			NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI); // Sync the changes in multiplayer.

			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

        // 删除原来的OnHitNPC方法，因为它现在由弹幕处理
        // public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        // {
        //     // 削减敌人2点防御
        //     target.defense = Math.Max(0, target.defense - 2);
        //     
        //     // 施加切割减益3秒
        //     target.AddBuff(ModContent.BuffType<Buff.SlicingBuff>(), 180); // 3秒 = 180 ticks
        //
        //     base.OnHitNPC(player, target, hit, damageDone);
        // }

        // 删除原来的MeleeEffects方法，能量剑没有弹幕反弹效果
        // public override void MeleeEffects(Player player, Rectangle hitbox)
        // {
        //     // 有概率反弹弹幕
        //     if (Main.rand.NextBool(2)) // 50%
        //     {
        //         TryReflectProjectiles(player, hitbox);
        //     }
        // }

        // 删除原来的TryReflectProjectiles方法

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Items.Placeables.ChromiumBar>(), 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}