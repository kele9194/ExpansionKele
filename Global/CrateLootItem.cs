using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Bosses.ShadowOfRevenge;
using ExpansionKele.Content.Bosses.BossKele;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 宝箱掉落修改系统
    /// 为特定宝箱添加额外的掉落规则
    /// </summary>
    public class CrateLootItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            // 检查是否为圆柏秘银匣 (IronCrateHard)
            if (item.type == ItemID.IronCrateHard)
            {
                // 使用ByCondition方法添加基于复仇之影击败状态的掉落规则
                var shadowCondition = new ShadowOfRevengeDefeatedCondition();
                itemLoot.Add(ItemDropRule.ByCondition(shadowCondition, ModContent.ItemType<FullMoonBar>(), 20, 3, 7));
            }
            
            // 检查是否为神圣渔获匣 (HallowedFishingCrateHard)
            if (item.type == ItemID.HallowedFishingCrateHard)
            {
                // 使用ByCondition方法添加基于BossKele击败状态的掉落规则
                var bossKeleCondition = new BossKeleDefeatedCondition();
                itemLoot.Add(ItemDropRule.ByCondition(bossKeleCondition, ModContent.ItemType<StarryBar>(), 20, 3, 7));
            }
            
            // 检查是否为金匣 (GoldenCrate)
            if (item.type == ItemID.GoldenCrate)
            {
                // 使用ByCondition方法添加基于骷髅王击败状态的掉落规则
                var skeletronCondition = new SkeletronDefeatedCondition();
                
                // 添加铬钢锭掉落规则
                itemLoot.Add(ItemDropRule.ByCondition(skeletronCondition, ModContent.ItemType<ChromiumBar>(), 20, 3, 7));
                
                // 添加赛格锭掉落规则
                itemLoot.Add(ItemDropRule.ByCondition(skeletronCondition, ModContent.ItemType<SigwutBar>(), 20, 3, 7));
            }
        }
    }

    /// <summary>
    /// 复仇之影被击败的掉落条件
    /// </summary>
    public class ShadowOfRevengeDefeatedCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            var system = ModContent.GetInstance<DownedShadowOfRevengeBoss>();
            return system != null && system.downedShadowOfRevenge;
        }

        public bool CanShowItemDropInUI()
        {
            // 在UI中显示此掉落，因为这是一个世界状态条件
            return true;
        }

        public string GetConditionDescription()
        {
            return "击败复仇之影后";
        }
    }

    /// <summary>
    /// BossKele被击败的掉落条件
    /// </summary>
    public class BossKeleDefeatedCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            var system = ModContent.GetInstance<DownedBossKele>();
            return system != null && system.downedBossKele;
        }

        public bool CanShowItemDropInUI()
        {
            // 在UI中显示此掉落，因为这是一个世界状态条件
            return true;
        }

        public string GetConditionDescription()
        {
            return "击败BossKele后";
        }
    }

    /// <summary>
    /// 骷髅王被击败的掉落条件
    /// </summary>
    public class SkeletronDefeatedCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return NPC.downedBoss3;
        }

        public bool CanShowItemDropInUI()
        {
            // 在UI中显示此掉落，因为这是一个游戏进度条件
            return true;
        }

        public string GetConditionDescription()
        {
            return "击败骷髅王后";
        }
    }
}