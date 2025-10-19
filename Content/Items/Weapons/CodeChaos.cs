using System;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Weapons
{
    public class CodeChaos : ModItem
    {
        // 当前伤害类型 0=近战, 1=远程, 2=魔法, 3=召唤
        const int meleeDamageType = 3;
        const int rangedDamageType = (meleeDamageType + 1)%4;
        const int magicDamageType = (rangedDamageType + 1)%4;
        const int summonDamageType = (magicDamageType + 1)%4;
        private int currentDamageType = meleeDamageType;
        
        // 浮动参数
        private float damageMultiplier = 1f;
        private float critDamageMultiplier = 1.5f;
        private float critChanceMultiplier = 1f;
        private float useTimeMultiplier = 1f;
        
        public override string LocalizationCategory => "Items.Weapons";

        // 添加一个确保数组正确初始化且元素有效的方法
        // ... existing code ...
private int GetValidProjectileType(int index)
        {
            // 确保数组已初始化
            if (ExpansionKele.projectileTypes == null || index < 0 || index >= ExpansionKele.projectileTypes.Length)
            {
                // 返回默认有效的Projectile类型
                return ProjectileID.FireArrow;
            }

            // 检查数组中的Projectile类型是否有效
            if (ExpansionKele.projectileTypes[index] > 0)
            {
                // 再次检查该Projectile类型是否在游戏中注册
                if (ContentSamples.ProjectilesByType.ContainsKey(ExpansionKele.projectileTypes[index]))
                {
                    return ExpansionKele.projectileTypes[index];
                }
            }

            // 如果无效，返回默认Projectile类型
            return ProjectileID.FireArrow;
        }
// ... existing code ...

        public override void SetDefaults()
        {
            Item.damage = 42;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Master;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;
            Item.crit = 4;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = false;
            Item.noUseGraphic = false;
        }
// ... existing code ...

        public override void UpdateInventory(Player player)
        {
            // 每帧更新浮动参数
            damageMultiplier = Main.rand.NextFloat(0.5f, 2f);
            critDamageMultiplier = Main.rand.NextFloat(0.75f, 1.25f);
            critChanceMultiplier = Main.rand.NextFloat(0.75f, 1.25f);
            useTimeMultiplier = Main.rand.NextFloat(0.75f, 1.25f);
            
            // 应用浮动参数到物品属性
            Item.useTime = (int)(20 * useTimeMultiplier);
            Item.useAnimation = (int)(20 * useTimeMultiplier);
        }
// ... existing code ...

        public override bool CanUseItem(Player player)
        {
            // 切换到下一个伤害类型
            currentDamageType = (currentDamageType + 1) % 4;
            SwitchDamageType();
            return base.CanUseItem(player);
        }
// ... existing code ...

        private void SwitchDamageType()
        {
            switch (currentDamageType)
            {
                case meleeDamageType:
                    Item.DamageType = DamageClass.Melee;
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.noMelee = false;
                    Item.noUseGraphic = false;
                    break;
                case rangedDamageType:
                    Item.DamageType = DamageClass.Ranged;
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    break;
                case magicDamageType:
                    Item.DamageType = DamageClass.Magic;
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    break;
                case summonDamageType:
                    Item.DamageType = DamageClass.Summon;
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.noMelee=false;
                    Item.noUseGraphic = false;
                    break;
            }
            //Main.NewText($"SwitchDamageType!currentDamageType: {currentDamageType}");
        }
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(player, target, ref modifiers);
            // 应用暴击伤害浮动
            modifiers.CritDamage *= critDamageMultiplier/2;//防止有其他伤害倍率出现
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // // 添加说明文字
            // tooltips.Add(new TooltipLine(Mod, "ChaosDescription", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosDescription").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect3", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect3").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect4", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect4").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect7", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect7").Value));
            // tooltips.Add(new TooltipLine(Mod, "ChaosEffect8", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.ChaosEffect8").Value));
            // tooltips.Add(new TooltipLine(Mod, "Pity", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Pity").Value));

            
            // string currentType = "";
            // switch(currentDamageType)
            // {
            //     case meleeDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Melee").Value; break;
            //     case rangedDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Ranged").Value; break;
            //     case magicDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Magic").Value; break;
            //     case summonDamageType: currentType = Language.GetText("Mods.ExpansionKele.Items.CodeChaos.Summon").Value; break;
            // }
            // tooltips.Add(new TooltipLine(Mod, "CurrentType", Language.GetText("Mods.ExpansionKele.Items.CodeChaos.CurrentType").Format(currentType)));
        }
    }
}