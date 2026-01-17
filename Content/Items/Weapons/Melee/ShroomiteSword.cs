using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class ShroomiteSword : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public static int shroomiteSwordMaxCharge =10;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(70, 95);
            Item.DamageType = DamageClass.Melee;
            Item.width = 64;
            Item.height = 64;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            // 设置为能量剑模式
            Item.shoot = ModContent.ProjectileType<ShroomiteSwordProjectile>();
            Item.shootsEveryUse = true;
            Item.noMelee = true; // 禁用直接接触伤害，使用弹幕代替
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
        {
            float adjustedItemScale = player.GetAdjustedItemScale(Item); // 获取玩家和物品的近战规模
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
            NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI); // 在多人游戏中同步变化

            return false; // 返回false因为我们在覆盖默认的射击行为
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShroomiteBar, 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public float GetCurrentCharge()
        {
            // 统计当前被标记的敌人数量
            int markedEnemyCount = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && npc.HasBuff(ModContent.BuffType<Buff.MushroomSwordMark>()))
                {
                    markedEnemyCount++;
                }
            }
            
            // 如果当前值大于最大值则强制设置为等于最大值
            if (markedEnemyCount > shroomiteSwordMaxCharge)
            {
                markedEnemyCount = shroomiteSwordMaxCharge;
            }
            
            return markedEnemyCount;
        }

        public float GetMaxCharge()
        {
            return shroomiteSwordMaxCharge;
        }
    }
}