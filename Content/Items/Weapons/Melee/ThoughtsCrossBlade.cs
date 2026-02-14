using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using ExpansionKele.Content.Projectiles.MeleeProj;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class ThoughtsCrossBlade : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            // 基本属性设置
            Item.width = 60;
            Item.height = 60;
            Item.damage = ExpansionKele.ATKTool(100,120);
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 22;
            Item.useTurn = true;
            Item.knockBack = 6f;
            Item.scale=0.75f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.shoot = ModContent.ProjectileType<ThoughtsCrossBladeProjectile>();
            Item.shootSpeed = 1f; // 速度不重要，因为我们控制方向
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        // ... existing code ...
          // ... existing code ...
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 竖向斩击（原始方向）
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI, 45f, 1f);
            // 横向斩击（原始方向）
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI, 135f, 1f);

            return false;
        }
        
       // ... existing code ...
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<SlicingBuff>(), 180); // 3秒 = 180帧
    }

        
// ... existing code ...
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ChromiumSword>(),1)    
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddIngredient(ItemID.Ectoplasm, 3)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}