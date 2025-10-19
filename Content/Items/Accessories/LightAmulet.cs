using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Accessories
{
    /// <summary>
    /// 明光护符类，继承自 ModItem。
    /// 这个饰品增加玩家的防御和最大生命值，并根据最近的敌对生物的距离动态调整这些属性。
    /// </summary>
    [AutoloadEquip(EquipType.Waist)]
    public class LightAmulet : ModItem
    {
        // 常量定义
        private const int DefenseBonus = 3; // 基础防御加成
        private const int LifeMaxBonus = 30; // 基础最大生命值加成
        private const int SearchRadius = 384; // 搜索敌对生物的半径（像素）
        private const int DefenseMultiBonus = 15; // 根据距离增加的额外防御
        private const float enduranceMultiBonus = 0.10f; // 根据距离增加的额外耐力
        public override string LocalizationCategory => "Items.Accessories";

        /// <summary>
        /// 设置饰品的基本属性。
        /// </summary>
        public override void SetDefaults()
        {
            //Item.SetNameOverride("明光护符"); // 设置饰品名称
            Item.width = 24; // 设置饰品宽度
            Item.height = 28; // 设置饰品高度
            Item.value = Item.buyPrice(0, 1, 0, 0); // 设置饰品价值（1银币）
            Item.rare = ItemRarityID.Blue; // 设置饰品稀有度为蓝色
            Item.accessory = true; // 设置饰品为可装备的饰品
            Item.defense= DefenseBonus;
        }

        /// <summary>
        /// 更新饰品效果。
        /// </summary>
        /// <param name="player">装备饰品的玩家对象。</param>
        /// <param name="hideVisual">是否隐藏饰品的视觉效果。</param>
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 增加基础防御和最大生命值
            player.statLifeMax2 += LifeMaxBonus;

            // 如果灾厄模组加载，则增加额外的防御和最大生命值
            // if (ExpansionKele.calamity != null)
            // {
            //     player.statDefense += DefenseBonus;
            //     player.statLifeMax2 += LifeMaxBonus;
            // }

            // 寻找最近的敌对生物
            NPC closestNPC = null;
            float closestDistance = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(player.Center) < SearchRadius)
                {
                    float distance = player.Distance(npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            // 如果找到了最近的敌对生物，则根据距离动态调整属性
            if (closestNPC != null)
            {
                float t = 1 - closestDistance / SearchRadius; // 计算 t 值

                // 增加额外的防御
                player.statDefense += (int)(DefenseMultiBonus * t + 0.5f);

                // 增加额外的耐力
                ExpansionKeleTool.AddDamageReduction(player,enduranceMultiBonus * t);

                // 根据 t 值的概率增加生命再生时间
                if (Main.rand.NextFloat() < t)
                {
                    player.lifeRegenTime += 1;
                }
            }
        }

        /// <summary>
        /// 修改饰品的工具提示。
        /// </summary>
        /// <param name="tooltips">工具提示列表。</param>
        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"DefenseBonus", $"[c/00FF00:防御力增加 {DefenseBonus}点]"},
                    {"LifeMaxBonus", $"[c/00FF00:最大生命值增加 {LifeMaxBonus}点]"},
                    {"DynamicEffect", $"[c/00FF00:在{SearchRadius / 16}格范围内，根据敌人距离额外增加最多{DefenseMultiBonus}点防御和{enduranceMultiBonus*100}%减伤]"},
                    {"ExtraTip", "[c/00FF00:根据敌人距离增加生命恢复速度]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...
        /// <summary>
        /// 定义饰品的制作食谱。
        /// </summary>
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(ModContent.ItemType<LightAmulet>()); // 创建饰品的食谱
            recipe.AddIngredient(ItemID.WormScarf, 1); // 添加蠕虫围巾作为材料
            recipe.AddIngredient(ItemID.LifeCrystal, 2); // 添加生命水晶作为材料
            recipe.AddIngredient(ItemID.SoulofLight, 3); // 添加光之魂作为材料
            recipe.AddIngredient(ItemID.BandofRegeneration, 1); // 添加再生手环作为材料
            recipe.AddTile(TileID.TinkerersWorkbench); // 设置制作台为工匠台
            recipe.Register(); // 注册食谱
        }
    }
}