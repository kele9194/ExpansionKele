using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 唐横刀 - 传承千年的经典武器
    /// 每次挥击首次命中敌人收集5点剑气能量，后续命中每个敌人+1点剑气
    /// 剑气能量上限100点，能量耗尽时仍可正常挥击
    /// </summary>
    public class TangDynastySaber : ModItem, IChargeableItem
    {
        public override string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(170,210);                    // 基础伤害
            Item.DamageType = DamageClass.Melee; // 近战伤害类型
            Item.width = 40;                     // 物品宽度
            Item.height = 40;                    // 物品高度
            Item.useTime = 15;                  // 使用时间
            Item.useAnimation = 15;             // 动画时间
            Item.useStyle = ItemUseStyleID.Swing; // 挥舞动作
            Item.knockBack = 5;                 // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this,1,1000000); // 价值
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this,ItemRarityID.Red); // 稀有度
            Item.UseSound = SoundID.Item1;      // 使用音效
            Item.autoReuse = true;              // 自动连击
            
            // 修改为能量剑模式
            Item.shoot = ModContent.ProjectileType<TangDynastySwordProjectile>();
            Item.shootsEveryUse = true;
            Item.noMelee = true; // 禁用直接接触伤害，使用弹幕代替
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            var swordPlayer = player.GetModPlayer<SwordEnergyPlayer>();
            swordPlayer.ResetHitCounter();
            float adjustedItemScale = player.GetAdjustedItemScale(Item); // Get the melee scale of the player and item.
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
            NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI); // Sync the changes in multiplayer.
            if(swordPlayer.ConsumeSwordEnergy(1)){
            // 计算面向鼠标的准确方向
            Vector2 shootDirection = Main.MouseWorld - position;
            shootDirection.Normalize();
            shootDirection *= 12f; // 设置剑气速度
            Projectile.NewProjectile(
            source,
            position,
            shootDirection, // 使用准确的方向向量
            ModContent.ProjectileType<TangDynastySwordEnergyProjectile>(),
            damage,
            knockback,
            player.whoAmI,
            0,           // ai[0]: 生命周期计时器（会在AI中使用）
            180,         // ai[1]: 总生命周期（帧数）
            adjustedItemScale  // ai[2]: 缩放系数
        );
        
    }
    else{
        Vector2 shootDirection = Main.MouseWorld - position;
        shootDirection.Normalize();
        shootDirection *= 12f; // 设置剑气速度
        Projectile.NewProjectile(
        source,
        position,
        shootDirection, // 使用准确的方向向量
        ModContent.ProjectileType<TangDynastySwordEnergyProjectile>(),
        damage/2,
        knockback,
        player.whoAmI,
        0,           // ai[0]: 生命周期计时器（会在AI中使用）
        180,         // ai[1]: 总生命周期（帧数）
        adjustedItemScale);  // ai[2]: 缩放系数
    }

            return false; // 返回false因为我们自己处理了弹幕生成
        }

        public override void HoldItem(Player player)
        {
            // 标记玩家正在使用唐横刀
            var swordPlayer = player.GetModPlayer<SwordEnergyPlayer>();
            swordPlayer.usingTangSword = true;
        }


        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            
        }
        public float GetCurrentCharge()
        {
 
            var swordPlayer = Main.LocalPlayer.GetModPlayer<SwordEnergyPlayer>();
            return swordPlayer.swordEnergy;
        }

        public float GetMaxCharge()
        {
            // 返回剑气能量的最大值
            return SwordEnergyPlayer.MaxSwordEnergy;
        }

        public override void AddRecipes()
        {
        }
    }
}