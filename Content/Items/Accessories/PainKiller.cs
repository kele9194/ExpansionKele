using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.Localization;
using System.Collections.Generic;

namespace ExpansionKele.Content.Items.Accessories
{
    public class PainKiller : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        public LocalizedText ZenithText {get;private set;}
        public LocalizedText FTWText {get;private set;}

        public override void SetStaticDefaults()
        {
            ZenithText = this.GetLocalization("ZenithText");
            FTWText = this.GetLocalization("FTWText");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PainKillerPlayer>().hasPainKiller = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.zenithWorld)
            {
                tooltips.Add(new TooltipLine(Mod, "ZenithText", $"[i:ExpansionKele/PainKiller] {ZenithText}")
                {
                    OverrideColor = Microsoft.Xna.Framework.Color.LightGreen
                });
            }
            else if (Main.getGoodWorld)
            {
                tooltips.Add(new TooltipLine(Mod, "FTWText", $"[i:ExpansionKele/PainKiller] {FTWText}")
                {
                    OverrideColor = Microsoft.Xna.Framework.Color.LightBlue
                });
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bottle, 1)
                .AddIngredient(ItemID.ShroomiteBar, 3)
                .AddIngredient(ItemID.HallowedBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class PainKillerPlayer : ModPlayer
    {
        public bool hasPainKiller = false;

        public override void ResetEffects()
        {
            hasPainKiller = false;
        }

        // ... existing code ...
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (hasPainKiller)
            {
                //根据游戏难度设置减伤值
                int flatReduction;
                switch (Main.GameModeInfo.IsExpertMode)
                {
                    case true when Main.GameModeInfo.IsMasterMode:
                        flatReduction = 24; // 大师模式
                        break;
                    case true:
                        flatReduction =16; // 专家模式
                        break;
                    default:
                        flatReduction = 8; // 经典模式
                        break;
                }

                // 检查是否是特殊世界
                if (Main.zenithWorld) // GFB世界
                {
                     // 基础减伤提升到80
                    
                    // 25%概率额外受到100点伤害
                    if (Main.rand.Next(4) == 0) 
                    {
                        modifiers.FinalDamage.Flat += 60;
                    }
                    else{
                        modifiers.FinalDamage.Flat -= 60;
                    }
                }
                else if (Main.getGoodWorld) // FTW世界
                {
                    flatReduction = 30; // 减伤提升到36
                }

                // 应用固定数值减伤
                modifiers.FinalDamage.Flat -= flatReduction;
            }
        }
// ... existing code ...
        
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (hasPainKiller &&info.Dodgeable&&info.Damage<=1)
            {
                return true;
            }
            return base.FreeDodge(info);
        }
        
        
        public override void PostHurt(Player.HurtInfo info)
        {
            base.PostHurt(info);
        }
    }
}