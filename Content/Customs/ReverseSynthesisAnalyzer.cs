using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    public class ReverseSynthesisAnalyzer
    {
        public class ItemNode
        {
            public int ItemType { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public ItemCategory Category { get; set; }
            public ItemEra Era { get; set; }
            public List<ItemNode> Parents { get; set; } = new List<ItemNode>(); // 能合成出此物品的物品
            public List<ItemNode> Children { get; set; } = new List<ItemNode>(); // 此物品能合成出的物品
            public List<SourceInfo> DirectSources { get; set; } = new List<SourceInfo>(); // 直接获取途径
        }

        public class SourceInfo
        {
            public SourceType Type { get; set; }
            public string Description { get; set; }
            public float Difficulty { get; set; } // 估算难度(0-1)
        }

        public enum SourceType
        {
            BossDrop,
            TreasureBag,
            Crafting,
            Exploration,
            Quest,
            Event,
            Unknown
        }

        public enum ItemCategory
        {
            Craftable,
            BossDrop,
            TreasureBag,
            Unobtainable,
            BasicResource
        }

        public enum ItemEra
        {
            PreBoss,
            PreSkeletron,
            PreWallOfFlesh,
            PreMechanical,
            PrePlantera,
            PreGolem,
            PreLunatic,
            PreMoonlord,
            PostMoonlord,
            Unknown
        }

        private Dictionary<int, ItemNode> _itemGraph;
        private Mod _targetMod;

        public void AnalyzeMod(string modName)
        {
            _targetMod = ModLoader.GetMod(modName);
            if (_targetMod == null) return;

            BuildItemGraph();
            IdentifyBaseItems();
            PropagateCategories();
            DetermineEras();
        }

        private void BuildItemGraph()
        {
            _itemGraph = new Dictionary<int, ItemNode>();

            // 创建所有物品节点
            // 修复：使用ContentSamples.ItemsByType遍历所有物品，筛选出目标Mod的物品
            foreach (var itemType in ContentSamples.ItemsByType.Keys)
            {
                Item item = new Item();
                item.SetDefaults(itemType);
                
                // 确保物品属于目标Mod
                if (item.ModItem != null && item.ModItem.Mod == _targetMod)
                {
                    var node = new ItemNode
                    {
                        ItemType = itemType,
                        InternalName = item.Name,
                        DisplayName = Lang.GetItemNameValue(itemType)
                    };

                    _itemGraph[itemType] = node;
                }
            }

            // 构建合成关系图
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                
                // 检查是否是目标mod的物品
                if (_itemGraph.ContainsKey(recipe.createItem.type))
                {
                    var resultNode = _itemGraph[recipe.createItem.type];
                    
                    // 添加配方中的所有材料作为父节点
                    for (int j = 0; j < recipe.requiredItem.Count; j++)
                    {
                        if (recipe.requiredItem[j].type > 0 && _itemGraph.ContainsKey(recipe.requiredItem[j].type))
                        {
                            var ingredientNode = _itemGraph[recipe.requiredItem[j].type];
                            resultNode.Parents.Add(ingredientNode);
                            ingredientNode.Children.Add(resultNode);
                        }
                    }
                }
            }
        }

        private void IdentifyBaseItems()
        {
            // 识别没有父节点的物品（即不能通过合成获得的物品）
            foreach (var node in _itemGraph.Values)
            {
                if (node.Parents.Count == 0)
                {
                    // 这些是基础物品，需要手动识别其来源
                    IdentifySource(node);
                }
                else
                {
                    node.Category = ItemCategory.Craftable;
                }
            }
        }

        // ... existing code ...
        private void IdentifySource(ItemNode node)
        {
            // 通过名称和其他特征识别物品来源
            string name = node.InternalName.ToLower();
            string displayName = node.DisplayName.ToLower();

            // 检查是否是宝藏袋（通过ItemID.Sets.BossBag集合判断，而不是通过名称）
            if (ItemID.Sets.BossBag[node.ItemType])
            {
                node.Category = ItemCategory.TreasureBag;
                node.DirectSources.Add(new SourceInfo
                {
                    Type = SourceType.TreasureBag,
                    Description = "Boss宝藏袋，右键打开获得奖励",
                    Difficulty = 0.5f
                });
                return;
            }

            // 检查是否可能是Boss掉落
            if (node.ItemType > 0)
            {
                Item item = new Item();
                item.SetDefaults(node.ItemType);
                
                // 高稀有度物品很可能是Boss掉落
                if (item.rare >= ItemRarityID.LightRed)
                {
                    node.Category = ItemCategory.BossDrop;
                    node.DirectSources.Add(new SourceInfo
                    {
                        Type = SourceType.BossDrop,
                        Description = "Boss击败后掉落",
                        Difficulty = 0.7f
                    });
                    return;
                }
            }

            // 检查是否是基础资源（矿石、锭等）
            if (name.Contains("ore") || name.Contains("bar") || name.Contains("矿") || name.Contains("锭"))
            {
                node.Category = ItemCategory.BasicResource;
                node.DirectSources.Add(new SourceInfo
                {
                    Type = SourceType.Exploration,
                    Description = "通过探索和挖掘获得",
                    Difficulty = 0.3f
                });
                return;
            }

            // 默认分类为未知
            node.Category = ItemCategory.Unobtainable;
            node.DirectSources.Add(new SourceInfo
            {
                Type = SourceType.Unknown,
                Description = "获取方式未知",
                Difficulty = 1.0f
            });
        }
// ... existing code ...

        private void PropagateCategories()
        {
            // 从基础物品开始，向子物品传播分类信息
            var queue = new Queue<ItemNode>(_itemGraph.Values.Where(n => n.Parents.Count == 0));
            var visited = new HashSet<ItemNode>();

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                if (visited.Contains(currentNode)) continue;
                visited.Add(currentNode);

                // 更新所有子节点
                foreach (var childNode in currentNode.Children)
                {
                    // 如果子节点还没有分类，则继承父节点的一些特征
                    if (childNode.Category == ItemCategory.Unobtainable && currentNode.Category != ItemCategory.Unobtainable)
                    {
                        childNode.Category = ItemCategory.Craftable;
                    }

                    // 如果所有父节点都已处理，则可以将该节点加入队列
                    if (!visited.Contains(childNode) && childNode.Parents.All(p => visited.Contains(p)))
                    {
                        queue.Enqueue(childNode);
                    }
                }
            }
        }

        private void DetermineEras()
        {
            // 首先为基本物品分配时代
            foreach (var node in _itemGraph.Values.Where(n => n.Parents.Count == 0))
            {
                node.Era = DetermineEraForBasicItem(node);
            }

            // 然后通过合成链传播时代信息
            PropagateEras();
        }

        private ItemEra DetermineEraForBasicItem(ItemNode node)
        {
            // 根据物品的稀有度和其他特征确定时代
            if (node.ItemType > 0)
            {
                Item item = new Item();
                item.SetDefaults(node.ItemType);

                if (item.rare <= ItemRarityID.White) return ItemEra.PreBoss;
                else if (item.rare <= ItemRarityID.Blue) return ItemEra.PreSkeletron;
                else if (item.rare <= ItemRarityID.Green) return ItemEra.PreWallOfFlesh;
                else if (item.rare <= ItemRarityID.Pink) return ItemEra.PreMechanical;
                else if (item.rare <= ItemRarityID.LightRed) return ItemEra.PrePlantera;
                else if (item.rare <= ItemRarityID.Red) return ItemEra.PreGolem;
                else if (item.rare <= ItemRarityID.Purple) return ItemEra.PreLunatic;
                else if (item.rare <= ItemRarityID.Expert) return ItemEra.PreMoonlord;
                else return ItemEra.PostMoonlord;
            }

            return ItemEra.Unknown;
        }

        private void PropagateEras()
        {
            // 通过拓扑排序确保正确传播时代信息
            var sortedNodes = TopologicalSort();
            
            foreach (var node in sortedNodes)
            {
                if (node.Parents.Count > 0)
                {
                    // 物品的时代应该是其材料中最晚的时代
                    var latestEra = node.Parents.Max(p => GetEraValue(p.Era));
                    node.Era = GetEraFromValue(latestEra);
                    
                    // 但不应早于物品本身的基本时代
                    var basicEra = DetermineEraForBasicItem(node);
                    if (GetEraValue(basicEra) > latestEra)
                    {
                        node.Era = basicEra;
                    }
                }
            }
        }

        private List<ItemNode> TopologicalSort()
        {
            var result = new List<ItemNode>();
            var visited = new HashSet<ItemNode>();
            
            foreach (var node in _itemGraph.Values)
            {
                if (!visited.Contains(node))
                {
                    TopologicalSortVisit(node, visited, result);
                }
            }
            
            return result;
        }

        private void TopologicalSortVisit(ItemNode node, HashSet<ItemNode> visited, List<ItemNode> result)
        {
            visited.Add(node);
            
            foreach (var child in node.Children)
            {
                if (!visited.Contains(child))
                {
                    TopologicalSortVisit(child, visited, result);
                }
            }
            
            result.Add(node);
        }

        private int GetEraValue(ItemEra era)
        {
            switch (era)
            {
                case ItemEra.PreBoss: return 0;
                case ItemEra.PreSkeletron: return 1;
                case ItemEra.PreWallOfFlesh: return 2;
                case ItemEra.PreMechanical: return 3;
                case ItemEra.PrePlantera: return 4;
                case ItemEra.PreGolem: return 5;
                case ItemEra.PreLunatic: return 6;
                case ItemEra.PreMoonlord: return 7;
                case ItemEra.PostMoonlord: return 8;
                default: return -1;
            }
        }

        private ItemEra GetEraFromValue(int value)
        {
            switch (value)
            {
                case 0: return ItemEra.PreBoss;
                case 1: return ItemEra.PreSkeletron;
                case 2: return ItemEra.PreWallOfFlesh;
                case 3: return ItemEra.PreMechanical;
                case 4: return ItemEra.PrePlantera;
                case 5: return ItemEra.PreGolem;
                case 6: return ItemEra.PreLunatic;
                case 7: return ItemEra.PreMoonlord;
                case 8: return ItemEra.PostMoonlord;
                default: return ItemEra.Unknown;
            }
        }

        public List<ItemNode> GetItemsByEraAndCategory(ItemEra era, ItemCategory category)
        {
            return _itemGraph.Values
                .Where(n => n.Era == era && n.Category == category)
                .ToList();
        }

        public void PrintAnalysisReport()
        {
            // 打印分析报告
            foreach (ItemEra era in Enum.GetValues(typeof(ItemEra)))
            {
                var eraItems = _itemGraph.Values.Where(n => n.Era == era).ToList();
                if (eraItems.Count > 0)
                {
                    Console.WriteLine($"== {era} 时代 ==");
                    
                    foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory)))
                    {
                        var categoryItems = eraItems.Where(n => n.Category == category).ToList();
                        if (categoryItems.Count > 0)
                        {
                            Console.WriteLine($"  -- {category} 类别 --");
                            foreach (var item in categoryItems)
                            {
                                Console.WriteLine($"    {item.DisplayName} ({item.InternalName})");
                            }
                        }
                    }
                }
            }
        }
    }
}