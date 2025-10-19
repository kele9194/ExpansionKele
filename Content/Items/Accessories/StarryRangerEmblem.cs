using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryRangerEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float RangedDamageBonus = 0.15f; // +15%远程伤害
        private const int BaseArmorPenetration = 10;
        private const int BaseDamage = 6;
        private const float ArmorPenetrationPerDamage = 2f; // 每3%额外远程伤害提供1穿甲
        private const float DamagePerDamage = 4f; // 每7%额外远程伤害提供1点面板伤害
        private const float MaxDistanceBonus = 0.20f; // 最多增加20%远程暴击率
        private const float CritPerDistance = 0.1f; // 每4像素点增加0.1%远程暴击率
        private const float DamagePerDistance = 0.001f; // 每4像素点增加0.1%独立增伤
        private const float OptimalEnemyDistance = 60 * 16; // 最优敌人距离，960像素

        public override void SetDefaults()
        {
            //Item.SetNameOverride("神射星元徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Cyan;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
            
            // 检查当前玩家是否已经有星元徽章生效，且不是自己
            if (modPlayer.activeStarryEmblemType != -1 && 
                modPlayer.activeStarryEmblemType != Item.type)
                return; // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeStarryEmblemType = Item.type;


            float additionalRangedDamage = player.GetDamage(DamageClass.Ranged).Additive - 1f;
            additionalRangedDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.GetModPlayer<DamageFlatBonusRanger>().DamageFlatBonus += BaseDamage;// +6武器伤害
            player.GetModPlayer<DamageFlatBonusRanger>().DamageFlatBonus += (int)(additionalRangedDamage / DamagePerDamage * 100);//每7%额外远程伤害加成提供1点面板伤害
            player.GetArmorPenetration(DamageClass.Ranged) += BaseArmorPenetration; // +10穿甲 
            player.GetArmorPenetration(DamageClass.Ranged) += additionalRangedDamage / ArmorPenetrationPerDamage * 100;// 每3%额外远程伤害提供1穿甲
            player.GetDamage(DamageClass.Ranged) += RangedDamageBonus; // +15%远程伤害
            
            // 弹幕更新速度加快
            player.GetModPlayer<StarryRangerPlayer>().extraUpdates += 1;
            // 弹药消耗减少50%
            player.GetModPlayer<StarryRangerPlayer>().cantConsumeAmmo = true;

            // 计算距离最近敌人的距离
            // ... existing code ...
            // 计算距离最近敌人的距离
            float closestEnemyDistance = float.MaxValue; // 初始化为最大值
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(player.Center, npc.Center);
                    if (distance < closestEnemyDistance)
                    {
                        closestEnemyDistance = distance;
                    }
                }
            }

            // 如果没有找到敌人，设置为默认距离
            if (closestEnemyDistance == float.MaxValue)
            {
                closestEnemyDistance = OptimalEnemyDistance;
            }

            // 当离最近的敌人超过最优距离像素点之外每超过4像素点增加0.1%远程暴击率，最多增加20%
            if (closestEnemyDistance > OptimalEnemyDistance)
            {
                float distanceBonus = (closestEnemyDistance - OptimalEnemyDistance) / 4 * CritPerDistance;
                player.GetCritChance(DamageClass.Ranged) += MathHelper.Min(distanceBonus, MaxDistanceBonus*100);
            }
            // 当距离最近的敌人小于最优距离像素点时，每减少4个像素点增加自身0.1%的独立增伤
            else
            {
                float damageBonus = (OptimalEnemyDistance - closestEnemyDistance) / 4 * DamagePerDistance;
                ExpansionKeleTool.AddDamageBonus(player,damageBonus);
            }
// ... existing code ...

            player.GetModPlayer<StarryRangerPlayer>().hasStarryRangerEmblem = true;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"StarryRangerEmblemDamage", $"[c/00FF00:+{RangedDamageBonus * 100}%远程伤害]"},
                    {"StarryRangerEmblemPenetration", $"[c/00FF00:+{BaseArmorPenetration}远程穿甲]"},
                    {"StarryRangerEmblemDamageFlat", $"[c/00FF00:+{BaseDamage}远程武器伤害]"},
                    {"StarryRangerEmblemExtraUpdates", "[c/00FF00:弹幕更新速度加快]"},
                    {"StarryRangerEmblemAmmo", "[c/00FF00:弹药消耗减少50%]"},
                    {"StarryRangerEmblemCritBonus", $"[c/00FF00:当离最近的敌人超过{OptimalEnemyDistance}像素之外每超过4像素点增加{CritPerDistance}%远程暴击率，最多增加{MaxDistanceBonus * 100}%]"},
                    {"StarryRangerEmblemDamageBonus", $"[c/00FF00:当距离最近的敌人小于{OptimalEnemyDistance}像素时,每减少4个像素点增加自身{DamagePerDistance*100}%的独立增伤]"},
                    {"StarryRangerEmblemBonus1", $"[c/00FF00:每{ArmorPenetrationPerDamage}%额外远程伤害提供1远程穿甲]"},
                    {"StarryRangerEmblemBonus2", $"[c/00FF00:每{DamagePerDamage}%额外远程伤害提供1点远程面板伤害]"},
                    {"WARNING", "[c/800000:注意：多个星元徽章装备将只有第一个生效]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MoonRangerEmblem>(), 1);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }

    public class StarryRangerPlayer : ModPlayer
    {
        public int extraUpdates = 0;
        public bool cantConsumeAmmo = false;
        public bool hasStarryRangerEmblem = false;

        public override void ResetEffects()
        {
            extraUpdates = 0;
            cantConsumeAmmo = false;
            hasStarryRangerEmblem = false;
        }

        // ... existing code ...
        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (cantConsumeAmmo && weapon.DamageType == DamageClass.Ranged)
            {
                // 50%几率不消耗弹药
                return Main.rand.NextFloat() >= 0.5f;
            }
            return base.CanConsumeAmmo(weapon, ammo);
        }
// ... existing code ...

        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (item.DamageType == DamageClass.Ranged && hasStarryRangerEmblem)
            {
                Projectile.NewProjectileDirect(Player.GetSource_ItemUse(item), position, velocity, type, damage, knockback, Player.whoAmI).extraUpdates += extraUpdates;
            }
        }
    }
}