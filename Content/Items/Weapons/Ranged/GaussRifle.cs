using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class GaussRifle : ModItem
    {
        // 弹药计数
        private const int MaxAmmoCount = 5;
        private int ammoCount = MaxAmmoCount;
        private const int MaxDamage = 820;
        
        public override string LocalizationCategory => "Items.Weapons";

        private const int reloadTime = 8;
        private const int AmmoTime=44;

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.damage = MaxDamage; // 基础伤害1000
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = AmmoTime; // 使用时间60
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item40;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<GaussRifleProjectile>();
            Item.shootSpeed = 16f;
        }
        public override Vector2? HoldoutOffset() {  
            return new Vector2(-16f, 4f); // 持有偏移量。  
        }  

        public override void SetStaticDefaults()
        {
            // 允许右键重复使用
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override bool AltFunctionUse(Player player)
        {
            // 允许右键使用
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            // 右键装填
            if (player.altFunctionUse == 2)
            {
                Item.useTime = reloadTime;
                Item.useAnimation = reloadTime;
                Item.noUseGraphic = true;
                // 只要弹药不是满的就可以装填
                return ammoCount < MaxAmmoCount;
            }
            else{
                Item.useTime = AmmoTime;
                Item.useAnimation =AmmoTime;
                Item.noUseGraphic = false;
                if (ammoCount == 0)
                {
                    CombatText.NewText(player.getRect(), Color.Cyan, "NoAmmo", true);
                    return false;
                }
                return ammoCount >0;
            }
            
            // 左键射击，如果弹药用完则不能使用
            
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 右键装填
            if (player.altFunctionUse == 2)
            {
                // 装填弹药
                ammoCount = MaxAmmoCount;
                // 更新伤害到初始状态
                Item.damage = MaxDamage;
                CombatText.NewText(player.getRect(), Color.Cyan, $"{ammoCount}/{MaxAmmoCount}", true);
                // 播放装填音效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, player.position);
                return false;
            }
            
            // 左键射击
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GaussRifleProjectile>(), damage, knockback, player.whoAmI);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item91, player.position);
            
            // 减少弹药数量
            ammoCount--;
            CombatText.NewText(player.getRect(), Color.Cyan, $"{ammoCount}/{MaxAmmoCount}", true);
            
            // 更新伤害
            Item.damage = (int)(MaxDamage * System.Math.Pow(0.8, MaxAmmoCount - ammoCount));
            
            return false;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {

        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 7);
            recipe.AddIngredient(ItemID.SoulofFlight, 4);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}