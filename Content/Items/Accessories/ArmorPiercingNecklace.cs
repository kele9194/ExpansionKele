using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class ArmorPiercingNecklace : ModItem,ILocalizedModType
    {
        // 定义常量
        private const int BaseArmorPenetration = 8;
        private const int BaseDamage = 6;
        private const float ArmorPenetrationBaseDamage = 2f; // 每4%额外远程伤害提供1穿甲
        private const float DamageBaseDamage = 4f; // 每8%额外远程伤害提供1点面板伤害
        public override string LocalizationCategory => "Items.Accessories";
        
        
        



        public override void SetDefaults()
        {
            //Item.SetNameOverride("穿甲项链");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            float additionalRangedDamage = player.GetDamage(DamageClass.Ranged).Additive - 1f;
            additionalRangedDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            //Main.NewText($"1:{additionalRangedDamage}");
            player.GetModPlayer<DamageFlatBonusPlayer>().DamageFlatBonus += BaseDamage;// +6伤害
            player.GetModPlayer<DamageFlatBonusPlayer>().DamageFlatBonus += (int)(additionalRangedDamage / DamageBaseDamage*100);//每8%额外远程伤害加成提供1点面板伤害
            //Main.NewText($"2:{(additionalRangedDamage / DamageBaseDamage*100)}");
            player.GetArmorPenetration(DamageClass.Ranged) += BaseArmorPenetration; // +8穿甲 
            player.GetArmorPenetration(DamageClass.Ranged) += additionalRangedDamage / ArmorPenetrationBaseDamage*100;// 每4%额外远程伤害提供1穿甲
            //Main.NewText($"3:{additionalRangedDamage / ArmorPenetrationBaseDamage*100}");

            // 
            
        }


         public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 检查配置是否启用了详细工具提示
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                    // 添加详细信息
                    tooltips.Add(new TooltipLine(Mod, "ArmorPiercingNecklaceDetailed0", "[c/00FF00:详细信息:]"));
                    tooltips.Add(new TooltipLine(Mod, "ArmorPiercingNecklaceDetailed1", $"[c/00FF00:+{BaseArmorPenetration}点护甲穿透,+{BaseDamage}点伤害]"));
                    tooltips.Add(new TooltipLine(Mod, "ArmorPiercingNecklaceDetailed3", $"[c/00FF00:每{ArmorPenetrationBaseDamage}%额外远程伤害增加1点护甲穿透]"));
                    tooltips.Add(new TooltipLine(Mod, "ArmorPiercingNecklaceDetailed4", $"[c/00FF00:每{DamageBaseDamage}%额外远程伤害增加1点面板伤害]"));
                }
            }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SharkToothNecklace);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 1);
            recipe.AddIngredient(ItemID.WoodenArrow, 100);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
        
    }
}