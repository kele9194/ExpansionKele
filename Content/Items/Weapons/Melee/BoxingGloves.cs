using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class BoxingGloves : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Melee";
        
        private int ProjType => ModContent.ProjectileType<BoxingGlovesProjectile>();
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = false;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(100, 120);
            Item.DamageType = DamageClass.Melee;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this, ItemRarityID.Blue);
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjType;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            BoxingGlovesPlayer boxingPlayer = player.GetModPlayer<BoxingGlovesPlayer>();
            int handType = boxingPlayer.GetNextHandType();
            
            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, handType);
            
            return false;
        }
        public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[ModContent.ProjectileType<BoxingGlovesProjectile>()] <= 0;
		}
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Leather, 50)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class BoxingGlovesPlayer : ModPlayer
    {
        /// <summary>
        /// 当前手类型：0=左手，1=右手
        /// 默认从左手开始
        /// </summary>
        public int currentHandType = -1;

        /// <summary>
        /// 获取下一个手类型（切换左右手）
        /// </summary>
        public int GetNextHandType()
        {
            currentHandType =  - currentHandType;
            return currentHandType;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public override void ResetEffects()
        {
            // 可以在这里添加重置逻辑
        }
    }
}