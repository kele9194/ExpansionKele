using ExpansionKele.Content.Projectiles.RangedProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class LaserCutter : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(68, 80); // 每道激光每帧的伤害
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 30;
            Item.useTime = 2; // 每帧发射一次
            Item.useAnimation = 2; // 这样可以连续射击
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true; // 不产生近战碰撞框
            Item.knockBack = 0.1f;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item12; // 激光声音
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LaserCutterProjectile>();
            Item.shootSpeed = 15f; // 激光不需要速度
            Item.channel = true; // 保持按下鼠标才能使用
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LaserMachinegun, 1);
            recipe.AddIngredient(ItemID.FragmentVortex, 3);
            recipe.Register();
        }
    }
}