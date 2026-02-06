using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// Killings Blade 杀戮刀片 - 近战武器
    /// 类似美工刀的短武器，在真近战每次攻击到敌人后会存储刀片，右键可以更快地扔出刀片
    /// </summary>
    public class KillingsBlade : ModItem,IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        
        // 玩家持有的刀片数量
        public static readonly int MaxBladesStored = 200; // 最大存储数量
        
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 820;
            Item.DamageType = DamageClass.Melee;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Swing; // 使用挥舞
            Item.knockBack = 3f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<KillingsBladeThrownProjectile>();
            Item.shootSpeed = 15f;
        }
        
        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<KillingsBladePlayer>();
            if(player.altFunctionUse == 2){
                // 右键使用需要有存储的刀片
                
                Item.noMelee=true;
                Item.noUseGraphic=true;
                if(modPlayer.storedBlades > 0){
                    return true;
                }
                else{
                    //CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "NoBlade", true);
                    return false;
                }
            }
            else{
                Item.noMelee=false;
                Item.noUseGraphic=false;
                return true;
            }
        }

                public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<KillingsBladePlayer>();
            
            if(player.altFunctionUse == 2){
                // 右键投掷刀片
                if (modPlayer.storedBlades > 0) {
                    // 投掷刀片造成130%伤害
                     Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<KillingsBladeThrownProjectile>(), (int)(damage * 1.3f), knockback, player.whoAmI);
                    
                    // 在多人模式下同步投射物创建
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        // 获取刚创建的投射物并发送同步消息
                        int projIndex = Projectile.GetByUUID(player.whoAmI, Projectile.GetNextSlot() - 1);
                        if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                        {
                            Projectile projectile = Main.projectile[projIndex];
                            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.identity);
                        }
                    }
                    
                    // 减少存储的刀片数量
                    //CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "-1", true);
                    modPlayer.storedBlades--;
                    
                    // 同步玩家状态变化到其他客户端
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
                    }
                }
                return false;
            }
            else{
                // 左键不发射弹幕，使用默认近战攻击
                return false;
            }
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // 右键使用时调整伤害（如果需要）
            if (player.altFunctionUse == 2)
            {
                // 可以在这里添加右键伤害调整逻辑
            }
        }

        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                {
                    return 2; // 2倍攻速
                }
                else{
                    return 1;
                }
        }
            // 右键使用时加快攻击速度
            // ... existing code ...
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 检查是否是Killings Blade武器
            if (player.HeldItem.type == ModContent.ItemType<KillingsBlade>())
            {
                // 增加存储的刀片数量
                var kbPlayer = player.GetModPlayer<KillingsBladePlayer>();
                kbPlayer.storedBlades++;
                
                // 限制最大存储数量
                if (kbPlayer.storedBlades > KillingsBlade.MaxBladesStored)
                {
                    kbPlayer.storedBlades = KillingsBlade.MaxBladesStored;
                    //CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "FullBlade", true);
                }
                else{
                    //CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "+1", true);}
                }
                // 显示视觉效果
                
            }
        }
// ... existing code ...
            

        public override void AddRecipes()
        {
            
        }

        // ... existing code ...
        public float GetCurrentCharge()
        {
            Player player = Main.LocalPlayer; // 获取当前本地玩家
            var kbPlayer = player.GetModPlayer<KillingsBladePlayer>();
            return kbPlayer.storedBlades;
        }

        public float GetMaxCharge()
        {
            return KillingsBlade.MaxBladesStored;
        }
    }
// ... existing code ...

    // 玩家额外数据，用于存储刀片数量
    public class KillingsBladePlayer : ModPlayer
    {
        public int storedBlades = 0;

        public override void ResetEffects()
        {
            // 每帧重置
        }

        public override void PostUpdate()
        {
            // 限制最大存储数量
            if (storedBlades > KillingsBlade.MaxBladesStored)
            {
                storedBlades = KillingsBlade.MaxBladesStored;
            }
        }
    }
}