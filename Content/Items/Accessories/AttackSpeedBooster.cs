using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.Accessories
{
    // ... existing code ...
public class AttackSpeedBooster : ModItem
{
     public static float AttackSpeedBoostSpeed = 1.5f;// 50% 使用时间减少
    public static float AttackSpeedBoostDamage = 0.75f;// 0.75 倍基础乘算增伤
    public static float StealthGenMultiplier = 1.55f;
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
            //Item.SetNameOverride("攻速手套");
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.GetModPlayer<SlowSpeedGlovePlayer>().SlowSpeedGloveEquipped)
            {
                return;
            }
            // 为装备此饰品的玩家添加修饰器
            player.GetModPlayer<AttackSpeedBoosterPlayer>().AttackSpeedBoosterEquipped = true;
            player.GetModPlayer<AttackSpeedBoosterPlayer>().attackSpeedBoosterMultiplier = AttackSpeedBoostSpeed;
            
            var modPlayer = player.GetModPlayer<AttackSpeedBoosterPlayer>();
            
            // 对所有武器应用基础伤害加成（包括召唤武器）
            
            
            // 对非召唤武器或鞭子，应用攻速加成对应的伤害乘区

            ExpansionKeleTool.MultiplyDamageBonus(player, AttackSpeedBoostDamage);
            
            // 增加 Calamity 模组的 StealthGen 值（只对盗贼武器生效）
            ReflectionHelper.SetStealthGenStandstill(player, ReflectionHelper.GetStealthGenStandstill(player) * StealthGenMultiplier);
            ReflectionHelper.SetStealthGenMoving(player, ReflectionHelper.GetStealthGenMoving(player) * StealthGenMultiplier);
        }
        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            
            // 如果有灾厄模组，添加灾厄效果的 Tooltip
            if(ExpansionKele.calamity != null){
                string calamityTooltipText = Language.GetTextValue($"Mods.{Mod.Name}.Items.Accessories.AttackSpeedBooster.TooltipsWithCalamity",
                    (StealthGenMultiplier * 100).ToString("F1")
                );
                tooltips.Add(new TooltipLine(Mod, "CalamityEffect", calamityTooltipText));
            }
        }
// ... existing code ...

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PowerGlove, 1);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 10);
            recipe.AddIngredient(ItemID.WarriorEmblem, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }

    public class AttackSpeedBoosterPlayer : ModPlayer
    {
        public bool AttackSpeedBoosterEquipped = false;
        public float attackSpeedBoosterMultiplier=1f;

        public override void ResetEffects()
        {
            AttackSpeedBoosterEquipped = false;
        }

        // ... existing code ...
    public override float UseSpeedMultiplier(Item item)
    {
        if (AttackSpeedBoosterEquipped)
        {
            return attackSpeedBoosterMultiplier;
        }
        else
        {
            return 1f;
        }
    }


        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!AttackSpeedBoosterEquipped)
        {
            return;
        }

        // 检查是否为召唤物弹幕且不是鞭子
        if (proj.DamageType == DamageClass.Summon && !ProjectileID.Sets.IsAWhip[proj.type])
        {
            // 对于非鞭子的召唤武器，应用 1.5*0.75 倍乘算加成
            float combinedMultiplier = AttackSpeedBooster.AttackSpeedBoostSpeed;
            SummonDamageHelper.ApplyMultiplicativeBonusToSummon(proj, ref modifiers, combinedMultiplier);
        }
    }
        
    }
    public class AttackSpeedBoosterGlobalItem : GlobalItem
    {
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (player.GetModPlayer<AttackSpeedBoosterPlayer>().AttackSpeedBoosterEquipped)
            {
                 if (item.DamageType == DamageClass.Summon)
                {
                    int shootType = item.shoot;
                    if ( !ProjectileID.Sets.IsAWhip[shootType])
                    {
                        damage*=AttackSpeedBooster.AttackSpeedBoostSpeed;
                    }
                }
            }
            return;
           
        }
        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (player.GetModPlayer<AttackSpeedBoosterPlayer>().AttackSpeedBoosterEquipped)
            {
                 if (item.DamageType == DamageClass.Summon)
                {
                    int shootType = item.shoot;
                    if ( !ProjectileID.Sets.IsAWhip[shootType])
                    {
                        return 1/AttackSpeedBooster.AttackSpeedBoostSpeed;
                    }
                }
            }
            return 1;
           
        }
    }
}