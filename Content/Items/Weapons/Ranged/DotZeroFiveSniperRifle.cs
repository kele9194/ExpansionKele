using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.RangedProj;
using Terraria.GameContent.RGB;
using ExpansionKele.Content.Items.OtherItem;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class DotZeroFiveSniperRifle : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.damage = ExpansionKele.ATKTool(1000,1200); // 伤害500
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 50; // 使用时间180
            Item.useAnimation =50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = ExpansionKele.SniperSound; 
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DotZeroFiveSniperBullet>();
            Item.shootSpeed = 16f;
            // 不消耗子弹
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-16f, 4f);
        }

        public override bool CanUseItem(Player player)
        {
            // 确保每次只能发射一次
            return true;
        }

        public override void HoldItem(Player player)
        {
            // 当玩家持有武器时持续生成瞄准镜
            if (player.whoAmI == Main.myPlayer) // 只在玩家自己身上生成
            {
                // 检查是否已经有瞄准镜存在
                bool hasCrosshair = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.type == ModContent.ProjectileType<DotZeroFiveSniperCrosshair>() && p.owner == player.whoAmI)
                    {
                        hasCrosshair = true;
                        break;
                    }
                }

                // 如果没有瞄准镜，则生成一个
                if (!hasCrosshair)
                {
                    Projectile.NewProjectile(
                        new EntitySource_ItemUse_WithAmmo(player, Item, Item.ammo),
                        Main.MouseWorld,
                        Vector2.Zero,
                        ModContent.ProjectileType<DotZeroFiveSniperCrosshair>(),
                        0,
                        0,
                        player.whoAmI
                    );
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 创建实际的子弹
            Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<DotZeroFiveSniperBullet>(), damage, knockback, player.whoAmI);
            
            return false; // 不使用默认弹丸
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddIngredient(ItemID.ShroomiteBar, 7)
                .AddIngredient(ModContent.ItemType<GolemShard>(), 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}