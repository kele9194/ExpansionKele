using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ExpansionKele.Content.Projectiles.RangedProj;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.OtherItem;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class AutoAimingSniperRifle : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        
        // 存储标记的NPC索引
        private int markedNPC = -1;
        private int markerProjectile = -1;
        // 添加子弹计数器
        private int shotCounter = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 32;
            Item.damage = ExpansionKele.ATKTool(3000,4500);
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 180;
            Item.useAnimation = 180;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = ExpansionKele.SniperSound;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AutoAimingSniperBullet>();
            Item.shootSpeed = 16f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, 0f);
        }

        // ... existing code ...
        public override void HoldItem(Player player)
        {
            // 确保标记抛射体存在或重新创建它
            EnsureMarkerProjectileExists(player);
            
            FindAndMarkHighestHealthNPC(player);
            // 更新标记抛射体位置以跟随目标
            UpdateMarkerProjectilePosition(player);

            // 检查是否按下目标切换键
            if (ExpansionKele.AutoAimingKeyBind.JustPressed)
            {
                NPC target = FindNearestNPCToMouse(player);
                if (target != null)
                {
                    markedNPC = target.whoAmI;
                    UpdateMarkerProjectile(player, target);
                }
            }
            
            // 查找血量最高的敌人进行标记（如果没有已标记的目标）
            if (markedNPC == -1)
            {
                FindAndMarkHighestHealthNPC(player);
            }
            
            // 标记当前持有此武器的玩家
            var modPlayer = player.GetModPlayer<AutoAimingSniperRiflePlayer>();
            modPlayer.holdingAutoAimingSniperRifle = true;
        }

        public override bool CanUseItem(Player player)
        {
            // 统一使用设置，不再区分左右键
            Item.useTime = 90;
            Item.useAnimation = 90;
            return base.CanUseItem(player);
        }


        private void EnsureMarkerProjectileExists(Player player)
        {
            // 如果有标记的目标但没有对应的标记抛射体，则创建一个
            if (markedNPC != -1 && Main.npc[markedNPC].active && (markerProjectile == -1 || !Main.projectile[markerProjectile].active || Main.projectile[markerProjectile].type != ModContent.ProjectileType<AutoAimingMarker>()))
            {
                NPC target = Main.npc[markedNPC];
                markerProjectile = Projectile.NewProjectile(
                    new EntitySource_ItemUse_WithAmmo(player, Item, Item.ammo),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<AutoAimingMarker>(),
                    0,
                    0,
                    player.whoAmI
                );
            }
        }

        private void FindAndMarkHighestHealthNPC(Player player)
        {
            NPC target = FindHighestHealthNPC(player);
            
            // 如果找到了新目标或者之前的标记目标已失效
            if (target != null && (markedNPC == -1 || !Main.npc[markedNPC].active || 
                Vector2.Distance(player.Center, Main.npc[markedNPC].Center) > 800f))
            {
                markedNPC = target.whoAmI;
                UpdateMarkerProjectile(player, target);
            }
            // 如果已有标记但距离过远，则重新寻找
            else if (markedNPC != -1 && Main.npc[markedNPC].active && 
                     Vector2.Distance(player.Center, Main.npc[markedNPC].Center) > 800f)
            {
                markedNPC = target?.whoAmI ?? -1;
                UpdateMarkerProjectile(player, target);
            }
        }

        private NPC FindHighestHealthNPC(Player player)
        {
            NPC highestHealthNPC = null;
            int maxHealth = 0;
            float maxDistance = 800f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage && 
                    npc.type != NPCID.TargetDummy)
                {
                    float distance = Vector2.Distance(npc.Center, player.Center);
                    if (distance <= maxDistance && npc.life > maxHealth)
                    {
                        maxHealth = npc.life;
                        highestHealthNPC = npc;
                    }
                }
            }

            return highestHealthNPC;
        }

        private void UpdateMarkerProjectilePosition(Player player)
        {
            // 如果有活动的目标和标记抛射体，则更新其位置
            if (markedNPC != -1 && Main.npc[markedNPC].active && markerProjectile != -1 && Main.projectile[markerProjectile].active && Main.projectile[markerProjectile].type == ModContent.ProjectileType<AutoAimingMarker>())
            {
                Main.projectile[markerProjectile].Center = Main.npc[markedNPC].Center;
                Main.projectile[markerProjectile].timeLeft = 2; // 续命
            }
            // 如果标记目标已失效，清理标记
            else if (markedNPC != -1 && !Main.npc[markedNPC].active)
            {
                ClearMarkers();
            }
        }

        private void UpdateMarkerProjectile(Player player, NPC target)
        {
            // 移除旧的标记抛射体
            if (markerProjectile != -1 && Main.projectile[markerProjectile].active && Main.projectile[markerProjectile].type == ModContent.ProjectileType<AutoAimingMarker>())
            {
                Main.projectile[markerProjectile].Kill();
            }

            // 创建新的标记抛射体
            if (target != null)
            {
                markerProjectile = Projectile.NewProjectile(
                    new EntitySource_ItemUse_WithAmmo(player, Item, Item.ammo),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<AutoAimingMarker>(),
                    0,
                    0,
                    player.whoAmI
                );
            }
            // 即使目标为空，也要确保清理旧的标记抛射体
            else
            {
                markerProjectile = -1;
            }
        }

        // 新增方法：清除所有标记
        public void ClearMarkers()
        {
            // 清除标记的NPC
            markedNPC = -1;
            
            // 移除标记抛射体
            if (markerProjectile != -1 && Main.projectile[markerProjectile].active && Main.projectile[markerProjectile].type == ModContent.ProjectileType<AutoAimingMarker>())
            {
                Main.projectile[markerProjectile].Kill();
                markerProjectile = -1;
            }
            
            // 重置计数器
            shotCounter = 0;
        }

        // 添加静态方法用于外部调用
        public static void ClearAllMarkers()
        {
            // 查找并清理所有相关的标记抛射体
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<AutoAimingMarker>())
                {
                    proj.Kill();
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 增加射击计数器
            shotCounter++;
            
            // 判断是否是第六发子弹
            bool isSixthShot = shotCounter % 6 == 0;
            
            // 左键：在标记的NPC处生成弹丸
            if (markedNPC != -1 && Main.npc[markedNPC].active)
            {
                Vector2 targetPosition = Main.npc[markedNPC].Center;
                // 发射普通子弹
                    Projectile.NewProjectile(
                        source,
                        targetPosition,
                        Vector2.Zero,
                        ModContent.ProjectileType<AutoAimingSniperBullet>(),
                        damage,
                        0,
                        player.whoAmI
                    );
                
                if (isSixthShot)
                {
                    // 发射第六发特殊子弹
                    Projectile.NewProjectile(
                        source,
                        targetPosition,
                        Vector2.Zero,
                        ModContent.ProjectileType<SixthSniperBullet>(),
                        1,
                        0,
                        player.whoAmI
                    );
                }
            }

            return false;
        }

        private NPC FindNearestNPCToMouse(Player player)
        {
            NPC nearestNPC = null;
            float minDistance = 800f; // 最大距离限制

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage &&
                    npc.type != NPCID.TargetDummy)
                {
                    float distanceToMouse = Vector2.Distance(npc.Center, Main.MouseWorld);
                    if (distanceToMouse <= minDistance)
                    {
                        minDistance = distanceToMouse;
                        nearestNPC = npc;
                    }
                }
            }

            return nearestNPC;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddIngredient(ModContent.ItemType<GolemShard>(),3)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class AutoAimingSniperRiflePlayer : ModPlayer
    {
        /// <summary>
        /// 标记玩家当前是否持有自动瞄准狙击步枪
        /// </summary>
        public bool holdingAutoAimingSniperRifle = false;

        /// <summary>
        /// 重置每帧的效果状态
        /// </summary>
        public override void ResetEffects()
        {
            // 每帧开始时重置状态
            holdingAutoAimingSniperRifle = false;
        }

        /// <summary>
        /// 在所有物品效果处理完毕后执行
        /// </summary>
        public override void PostUpdate()
        {
            // 如果这一帧没有持有自动瞄准狙击步枪，则需要清除相关标记
            if (!holdingAutoAimingSniperRifle)
            {
                AutoAimingSniperRifle.ClearAllMarkers();
            }
        }

    }
}