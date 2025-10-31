using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;

namespace ExpansionKele.Content.Items.Weapons
{
    /// <summary>
    /// Killings Blade 杀戮刀片 - 近战武器
    /// 类似美工刀的短武器，在真近战每次攻击到敌人后会存储刀片，右键可以更快地扔出刀片
    /// </summary>
    public class KillingsBlade : ModItem
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
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
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
                    CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "NoBlade", true);
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
                    // 减少存储的刀片数量
                    CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "-1", true);
                    modPlayer.storedBlades--;
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
                    CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "FullBlade", true);
                }
                else{
                    CombatText.NewText(player.getRect(), Microsoft.Xna.Framework.Color.Cyan, "+1", true);}
                
                // 显示视觉效果
                
            }
        }
// ... existing code ...
            

        public override void AddRecipes()
        {
            
        }
    }

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