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
        public static LocalizedText TooltipsWithCalamity { get; private set; }
        public override string LocalizationCategory => "Items.Accessories";
        public override void SetStaticDefaults()
        {
            TooltipsWithCalamity =this.GetLocalization("TooltipsWithCalamity");
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
            ExpansionKeleTool.MultiplyDamageBonus(player,AttackSpeedBoostDamage);
            
            // 增加 Calamity 模组的 StealthGen 值 30%
            ReflectionHelper.SetStealthGenStandstill(player, ReflectionHelper.GetStealthGenStandstill(player) * 0.55f);
            ReflectionHelper.SetStealthGenMoving(player, ReflectionHelper.GetStealthGenMoving(player) * 0.55f);

        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if(ExpansionKele.calamity != null){
               tooltips.Add(new TooltipLine(Mod, "CalamityEffect", TooltipsWithCalamity.Value));
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
    }
}