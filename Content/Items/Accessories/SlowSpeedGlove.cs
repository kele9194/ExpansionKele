using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Accessories
{
    public class SlowSpeedGlove : ModItem
    {
        public static float AttackSpeedBoostSpeed = 0.7f;// 50% 使用时间减少
        public static float AttackSpeedBoostDamage = 1.65f;// 0.75 倍基础乘算增伤
        public static float StealthGenMultiplier = 0.66f;
        public override string LocalizationCategory => "Items.Accessories";
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(AttackSpeedBoostSpeed * 100),
        ValueUtils.FormatValue(AttackSpeedBoostDamage * 100),
        ValueUtils.FormatValue((AttackSpeedBoostSpeed * AttackSpeedBoostDamage) * 100)
        );

        public override void SetStaticDefaults()
        {
            // 不需要手动获取 Tooltip，tModLoader 会自动从 .hjson 文件加载
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        // ... existing code ...
         public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetModPlayer<AttackSpeedBoosterPlayer>().AttackSpeedBoosterEquipped)
            {
                return;
            }
            player.GetModPlayer<SlowSpeedGlovePlayer>().SlowSpeedGloverMultiplier = AttackSpeedBoostSpeed;
            player.GetModPlayer<SlowSpeedGlovePlayer>().SlowSpeedGloveEquipped = true;
            
            // 对所有武器应用基础伤害加成（包括召唤武器）
            ExpansionKeleTool.MultiplyDamageBonus(player, AttackSpeedBoostDamage);
            
            // 增加 Calamity 模组的 StealthGen 值
            ReflectionHelper.SetStealthGenStandstill(player, ReflectionHelper.GetStealthGenStandstill(player) * StealthGenMultiplier);
            ReflectionHelper.SetStealthGenMoving(player, ReflectionHelper.GetStealthGenMoving(player) * StealthGenMultiplier);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 如果有灾厄模组，添加灾厄效果的 Tooltip
            if(ExpansionKele.calamity != null){
                string calamityTooltipText = Language.GetTextValue($"Mods.{Mod.Name}.Items.Accessories.SlowSpeedGlove.TooltipsWithCalamity",
                    (StealthGenMultiplier * 100).ToString("F1")
                );
                tooltips.Add(new TooltipLine(Mod, "CalamityEffect", calamityTooltipText));
            }
        }

        
// ... existing code ...

        
        // ... existing code ...
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TurtleShell, 1); // 海龟壳
            recipe.AddIngredient(ItemID.PowerGlove, 1); // 强力手套
            recipe.AddIngredient(ItemID.WarriorEmblem, 1); // 暗影之魂（可能是指暗影鳞片）
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 10); // SigwutBar
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
// ... existing code ...
    }

    public class SlowSpeedGlovePlayer : ModPlayer
    {
        public bool SlowSpeedGloveEquipped = false;
        public float  SlowSpeedGloverMultiplier=1f;

        public override void ResetEffects()
        {
            SlowSpeedGloveEquipped = false;
        }

        public override float UseSpeedMultiplier(Item item)
        {
            if (SlowSpeedGloveEquipped)
            {
                return SlowSpeedGloverMultiplier;
            }
            return 1f;
        }
        
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!SlowSpeedGloveEquipped)
            {
                return;
            }

            // 检查是否为召唤物弹幕且不是鞭子
            if (proj.DamageType == DamageClass.Summon && !ProjectileID.Sets.IsAWhip[proj.type])
            {
                // 对于非鞭子的召唤武器，应用 0.7*1.65 倍乘算加成
                float combinedMultiplier = SlowSpeedGlove.AttackSpeedBoostSpeed;
                SummonDamageHelper.ApplyMultiplicativeBonusToSummon(proj, ref modifiers, combinedMultiplier);
            }
        }
    }
    
    public class SlowSpeedGloveGlobalItem : GlobalItem
    {
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (player.GetModPlayer<SlowSpeedGlovePlayer>().SlowSpeedGloveEquipped)
            {
                if (item.DamageType == DamageClass.Summon)
                {
                    int shootType = item.shoot;
                    if (!ProjectileID.Sets.IsAWhip[shootType])
                    {
                        damage *= SlowSpeedGlove.AttackSpeedBoostSpeed;
                    }
                }
            }
            return;
        }
        
        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (player.GetModPlayer<SlowSpeedGlovePlayer>().SlowSpeedGloveEquipped)
            {
                if (item.DamageType == DamageClass.Summon)
                {
                    int shootType = item.shoot;
                    if (!ProjectileID.Sets.IsAWhip[shootType])
                    {
                        return 1 / SlowSpeedGlove.AttackSpeedBoostSpeed;
                    }
                }
            }
            return 1;
        }
    }
}