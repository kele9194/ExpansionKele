using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System;

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
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 削减敌人2点防御
            target.defense = Math.Max(0, target.defense - 2);
            
            // 施加切割减益3秒
            target.AddBuff(ModContent.BuffType<Buff.SlicingBuff>(), 180); // 3秒 = 180 ticks

            base.OnHitNPC(player, target, hit, damageDone);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // 有概率反弹弹幕
            if (Main.rand.NextBool(2)) // 50%
            {
                TryReflectProjectiles(player, hitbox);
            }
        }

        private void TryReflectProjectiles(Player player, Rectangle hitbox)
        {
            // 遍历所有弹幕
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.hostile && !proj.friendly)
                {
                    // 检查弹幕是否在武器范围内
                    Rectangle projRect = new Rectangle((int)proj.position.X, (int)proj.position.Y, proj.width, proj.height);
                    if (hitbox.Intersects(projRect))
                    {
                        // 50%概率反弹弹幕
                        if (Main.rand.NextBool(2))
                        {
                            // 将敌对弹幕转换为友方弹幕
                            proj.hostile = false;
                            proj.friendly = true;

                            // 反转弹幕速度方向
                            proj.velocity = -proj.velocity;

                            // 设置弹幕所有者为玩家
                            proj.owner = player.whoAmI;

                            // 添加视觉效果
                            for (int j = 0; j < 10; j++)
                            {
                                Dust dust = Dust.NewDustDirect(
                                    proj.position,
                                    proj.width,
                                    proj.height,
                                    DustID.MagicMirror,
                                    0,
                                    0,
                                    100,
                                    default(Color),
                                    1.2f
                                );
                                dust.noGravity = true;
                                dust.velocity *= 2f;
                            }
                        }
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Items.Placeables.ChromiumBar>(), 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}