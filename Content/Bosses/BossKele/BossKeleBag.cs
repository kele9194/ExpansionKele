using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ExpansionKele.Content.Items.Weapons;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Bosses.BossKele
{
    
    public class BossKeleBag : ModItem
    {
        public override string LocalizationCategory=>"Bosses.BossKele";
        public override void SetStaticDefaults()
        {
            // 指定为宝藏袋
            ItemID.Sets.BossBag[Type] = true; 
            ItemID.Sets.PreHardmodeLikeBossBag[Type] = true; 

            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("可乐的宝藏袋");
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.expert = true; // 专家模式物品
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // 添加 30-40 个星元锭
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<StarryBar>(), 1, 30, 40));
            
            // 添加 2 个铂金币
            itemLoot.Add(ItemDropRule.Common(ItemID.PlatinumCoin, 1, 2, 2));
            
            // 添加 15-20 瓶超级治疗药水
            itemLoot.Add(ItemDropRule.Common(ItemID.SuperHealingPotion, 1, 15, 20));

            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<CodeChaos>(), 4, 1, 1));
        }
        }
    }