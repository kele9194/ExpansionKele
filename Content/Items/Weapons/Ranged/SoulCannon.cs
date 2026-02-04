using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class SoulCannon : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public static readonly SoundStyle Charge = new SoundStyle("ExpansionKele/Content/Audio/Charge");
        public static readonly SoundStyle Fire = SoundID.Item12;

        public static int AftershotCooldownFrames = 12;
        public static int FullChargeFrames = 88;
        private const string setNameOverride = "灵魂加农炮";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }


        public override void SetDefaults()
        {
            //Item.SetNameOverride(setNameOverride);
            Item.width = 48;
            Item.height = 24;
            Item.damage = ExpansionKele.ATKTool(105,120);
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = (Item.useAnimation = AftershotCooldownFrames);
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<SoulCannonHoldOut>();
            Item.shootSpeed = 12f;
            Item.crit = 6;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override void HoldItem(Player player)
        {
            //player.mouseInterface = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            
        }

        float IChargeableItem.GetCurrentCharge()
        {
            // 遍历所有活跃的弹幕，找到属于当前玩家的灵魂加农炮弹幕
            foreach (var projectile in Main.projectile)
            {
                if (projectile.active && projectile.type == ModContent.ProjectileType<SoulCannonHoldOut>() &&
                    projectile.owner == Main.myPlayer)
                {
                    if (projectile.ModProjectile is SoulCannonHoldOut holdOut)
                    {
                        return holdOut._currentChargingFrames; // 假设SoulCannonHoldOut项目也有CurrentChargingFrames字段
                    }
                }
            }
            // 如果没有找到活跃的弹幕，则返回0
            return 0f;
        }

        float IChargeableItem.GetMaxCharge()
        {
            foreach (var projectile in Main.projectile)
            {
                if (projectile.active && projectile.type == ModContent.ProjectileType<SoulCannonHoldOut>() &&
                    projectile.owner == Main.myPlayer)
                {
                    if (projectile.ModProjectile is SoulCannonHoldOut holdOut)
                    {
                        return holdOut.MaxChargeingFrame; // 假设SoulCannonHoldOut项目也有CurrentChargingFrames字段
                    }
                }
            }
            // 如果没有找到活跃的弹幕，则返回0
            return 0f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<SoulCannon>());
            recipe.AddIngredient(ModContent.ItemType<ProtonCannon>());
            recipe.AddIngredient(ItemID.Ectoplasm, 3);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}