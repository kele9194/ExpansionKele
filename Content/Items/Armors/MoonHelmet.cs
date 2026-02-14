using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Accessories;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Buff;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Armors
{
    [AutoloadEquip(EquipType.Head)]
    public class MoonHelmet : ModItem

    {
        public override string LocalizationCategory => "Items.Armors";
        public static LocalizedText SetBonusText { get; private set; }
		public static LocalizedText SetBonusTextWithCalamity { get; private set; }



		public override void SetStaticDefaults() {


			SetBonusText = this.GetLocalization("SetBonus");
			SetBonusTextWithCalamity = this.GetLocalization("SetBonusWithCalamity");
		}

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月头盔");
            Item.width = 18;
            Item.height = 18;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.defense = 7;
        }

        public override void UpdateEquip(Player player)
        {
            ExpansionKeleTool.MultiplyDamageBonus(player,1.05f);
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.manaCost -= 0.15f;
            player.whipRangeMultiplier+=0.25f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<MoonBreastplate>() && 
                   legs.type == ModContent.ItemType<MoonLeggings>();
        }

        

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            // "武器面板直接增加12点伤害\n" +
            //                  "如果加载了灾厄模组,增加70点潜伏值\n" +
            //                  "武器使用时会发射造成30点伤害的满月弹幕\n" +
            //                  "满月减益:未拥有减益的敌人被命中时获得6%伤害增益,持续2秒\n" +
            //                  "满月减益持续30秒,若已拥有则被满月弹幕击中时减益时间减半\n" +
            //                  "减益结束时造成武器伤害12倍的爆炸伤害";

            // 增加武器伤害
            player.GetModPlayer<DamageFlatBonusPlayer>().DamageFlatBonus += 12;

            // 如果灾厄模组被加载，增加潜伏值
            if (ExpansionKele.calamity != null)
            {
                player.setBonus = SetBonusTextWithCalamity.Value;
                // 增加潜伏值的代码将通过反射实现
                ReflectionHelper.ApplyRogueStealth(player, 70f/100f);
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<FullMoonBar>(10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

        
}
public class FullMoonPlayer : ModPlayer
    {
        private int fullMoonCooldown;

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 检查玩家是否装备了满月套装
            if (Player.armor[0].type == ModContent.ItemType<MoonHelmet>() &&
                Player.armor[1].type == ModContent.ItemType<MoonBreastplate>() &&
                Player.armor[2].type == ModContent.ItemType<MoonLeggings>())
            {
                // 检查是否是满月弹幕
                if (proj.type == ModContent.ProjectileType<FullMoonArmorProj>())
                {
                    int debuffTime = 1800;
                    // 检查敌人是否已经有满月减益
                    bool hasDebuff = target.HasBuff(ModContent.BuffType<FullMoonArmorDebuff>());
                    if(hasDebuff)
                    {
                        for (int i = 0; i < target.buffType.Length; i++)
                        {
                            if (target.buffType[i] == ModContent.BuffType<FullMoonArmorDebuff>())
                            {
                                target.buffTime[i] = target.buffTime[i] / 2 + 1;
                                break;
                            }
                        }
                    }
                    else{
                        target.AddBuff(ModContent.BuffType<FullMoonArmorDebuff>(), debuffTime);
                    }
                    if (!hasDebuff)
                    {
                        // 给玩家添加临时伤害增益（2秒）
                        Player.AddBuff(ModContent.BuffType<FullMoonBonusBuff>(), 120);
                    }
                    
                }
            }
        }

        // ... existing code ...
public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // 检查玩家是否装备了满月套装
            if (Player.armor[0].type == ModContent.ItemType<MoonHelmet>() &&
                Player.armor[1].type == ModContent.ItemType<MoonBreastplate>() &&
                Player.armor[2].type == ModContent.ItemType<MoonLeggings>())
            {
                // 冷却时间控制 - 基于当前武器的使用时间
                if (fullMoonCooldown <= 0)
                {
                    // 发射满月弹幕
                    Projectile.NewProjectile(
                        Player.GetSource_ItemUse(item),
                        position,
                        velocity * 0.5f, // 稍微降低速度
                        ModContent.ProjectileType<FullMoonArmorProj>(),
                        30, // 30点伤害
                        knockback,
                        Player.whoAmI,
                        Main.MouseWorld.X,
                        Main.MouseWorld.Y
                    );

                    // 冷却时间基于武器的使用时间，最小为1帧
                    fullMoonCooldown = Math.Max(1, Player.HeldItem.useAnimation);
                }
            }
        }
// ... existing code ...

        public override void PostUpdate()
        {
            // 更新冷却时间
            if (fullMoonCooldown > 0)
            {
                fullMoonCooldown--;
            }

    }
}

public class FullMoonArmorProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // 设置尾迹长度为 10 帧，使用尾迹模式 2（平滑渐变）
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            // 基础属性设置
            Projectile.width = 8;                   // 弹幕宽度
            Projectile.height = 8;                  // 弹幕高度
            Projectile.friendly = true;             // 对玩家友好
            Projectile.penetrate = 1;              // 无限穿透
            Projectile.timeLeft = 420;              // 总生存时间为 420 帧（7 秒）
            Projectile.tileCollide = false;         // 不与地形碰撞
            Projectile.ignoreWater = true;          // 忽略水体阻挡
            Projectile.light = 1f;                  // 发光强度
            Projectile.extraUpdates = 3;            // 每帧只更新一次
            Projectile.usesLocalNPCImmunity = true; // 使用本地无敌帧系统
            Projectile.localNPCHitCooldown = 5;     // 击中 NPC 后冷却 5 帧再可击中下一个
        }

        // ... existing code ...
public override void OnSpawn(IEntitySource source)
        {
            Projectile.originalDamage=Projectile.damage;
        }
// ... existing code ...

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 添加发光效果
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.8f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100,
                    default(Color), 1.2f);
                dust.noGravity = true;
            }
        }

        // 碰撞到地形时不触发反弹或销毁
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        // 击中敌人时可添加特效或音效（目前为空）
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 可选：在此添加击中特效或音效
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration += 15;
        }
    }

    


}