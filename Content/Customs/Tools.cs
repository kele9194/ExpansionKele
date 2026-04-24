using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
// ... existing code ...

    /// <summary>
    /// 提供与Boss召唤相关的实用工具方法
    /// </summary>
    public static class BossSpawnUtils
    {
        /// <summary>
        /// 使用物品召唤Boss
        /// </summary>
        /// <param name="player">召唤Boss的玩家</param>
        /// <param name="npcType">要召唤的Boss的NPC类型ID</param>
        /// <param name="spawnSound">召唤时播放的音效（可选）</param>
        /// <remarks>
        /// 该方法会处理单人和多人模式的网络同步问题。
        /// 在单人模式下立即生成Boss，在多人模式下向服务器发送请求。
        /// </remarks>
        public static void ItemSpawnBoss(Player player, int npcType, in SoundStyle? spawnSound = null)
        {
            DebugMarker.Mark();
            if (spawnSound.HasValue)
            {
                SoundEngine.PlaySound(spawnSound.Value, player.Center);
            }

            if (player.whoAmI != Main.myPlayer){
                DebugMarker.Mark();
                return;
            }

            switch (Main.netMode)
            {
                case NetmodeID.SinglePlayer:
                DebugMarker.Mark();
                    NPC.SpawnOnPlayer(player.whoAmI, npcType);
                    break;

                //为其设置特殊参数
                case NetmodeID.MultiplayerClient:
                DebugMarker.Mark();
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, npcType);
                    break;
            }
        }
    }

    /// <summary>
    /// 提供与玩家相关的实用工具方法
    /// </summary>
    public static class PlayerUtils
    {
        /// <summary>
        /// 减少玩家的魔力病（Mana Sickness）持续时间
        /// </summary>
        /// <param name="player">要减少魔力病持续时间的玩家对象</param>
        /// <remarks>
        /// 该方法每帧有 50% 的概率减少 1 帧的魔力病持续时间（约 0.0167 秒）。
        /// 当持续时间被减少到小于 0 时，会自动设置为 0。
        /// </remarks>
        public static void ReduceManaSicknessDuration(Player player)
        {
            int buffIndex = player.FindBuffIndex(BuffID.ManaSickness);
            if (buffIndex != -1)
            {
                // 每帧减少 1 帧（≈ 0.0167 秒）持续时间
                if (Main.rand.NextDouble() < 0.5f)
                {
                    player.buffTime[buffIndex] -= 1;
                }

                // 防止负值
                if (player.buffTime[buffIndex] < 0)
                {
                    player.buffTime[buffIndex] = 0;
                }
            }
        }
    }

    /// <summary>
    /// 提供与物品（Item）相关的实用工具方法
    /// </summary>
    public static class ItemUtils
    {
        /// <summary>
        /// 根据合成配方计算物品的价值
        /// </summary>
        /// <param name="item">要计算价值的模组物品</param>
        /// <param name="profitMargin">利润率系数，默认为 1.0（即材料总价）</param>
        /// <param name="defaultPrice">当物品没有配方时的默认价格，默认为 1000 铜币</param>
        /// <returns>计算后的物品价值（铜币）</returns>
        /// <remarks>
        /// 该方法会遍历所有配方，找到以该物品为结果的第一个配方，
        /// 然后计算所有材料的总价值并乘以利润率。
        /// 如果没有找到配方，则返回默认价格。
        /// </remarks>
        public static int CalculateValueFromRecipes(ModItem item, float profitMargin = 1.0f, int defaultPrice = 1000)
        {
            var recipes = new List<Recipe>();
            
            // 遍历所有配方，找出结果为此物品的配方
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe currentRecipe = Main.recipe[i];
                if (currentRecipe.createItem.type == item.Type)
                {
                    recipes.Add(currentRecipe);
                }
            }

            // 如果没有配方，返回默认值
            if (recipes.Count == 0)
                return defaultPrice;

            // 使用第一个配方进行计算（通常是最主要的配方）
            var recipe = recipes[0];
            int totalValue = 0;

            // 计算所有材料的价值
            foreach ((Item ingredient, int stack) in recipe.requiredItem.ToArray().WithStack())
            {
                totalValue += ingredient.value * stack;
            }

            // 应用利润率并确保结果至少为 1 铜币
            int calculatedValue = (int)(totalValue * profitMargin);
            return calculatedValue > 0 ? calculatedValue : 1;
        }

        /// <summary>
        /// 根据合成配方计算物品的稀有度
        /// </summary>
        /// <param name="item">要计算稀有度的模组物品</param>
        /// <param name="defaultRarity">当物品没有配方时的默认稀有度，默认为绿色（ItemRarityID.Green）</param>
        /// <param name="referenceItem">如果提供了参考物品，则直接返回该物品的稀有度</param>
        /// <returns>计算后的物品稀有度值</returns>
        /// <remarks>
        /// 该方法优先使用参考物品的稀有度。
        /// 如果没有参考物品，则遍历所有配方，找到以该物品为结果的第一个配方，
        /// 然后返回所有材料中的最高稀有度。
        /// 如果没有找到配方，则返回默认稀有度。
        /// </remarks>
        public static int CalculateRarityFromRecipes(ModItem item, int defaultRarity = ItemRarityID.Green, ModItem referenceItem = null)
        {
            // 如果指定了参考物品，直接返回该物品的稀有度
            if (referenceItem != null)
            {
                return referenceItem.Item.rare;
            }

            var recipes = new List<Recipe>();
            
            // 遍历所有配方，找出结果为此物品的配方
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe currentRecipe = Main.recipe[i];
                if (currentRecipe.createItem.type == item.Type)
                {
                    recipes.Add(currentRecipe);
                }
            }

            // 如果没有配方，返回默认稀有度（绿色）
            if (recipes.Count == 0)
                return defaultRarity;

            // 使用第一个配方进行计算
            var recipe = recipes[0];
            int highestRarity = int.MinValue;

            // 找到所有材料中最高的稀有度
            foreach ((Item ingredient, int stack) in recipe.requiredItem.ToArray().WithStack())
            {
                if (ingredient.rare > highestRarity)
                {
                    highestRarity = ingredient.rare;
                }
            }

            // 如果找到材料，则返回最高稀有度，否则返回默认稀有度
            return highestRarity != int.MinValue ? highestRarity : defaultRarity;
        } 
    }

    /// <summary>
    /// 提供 IEnumerable&lt;Item&gt; 的扩展方法
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 将物品数组转换为包含物品和数量的元组序列
        /// </summary>
        /// <param name="items">要转换的物品数组</param>
        /// <returns>返回一个枚举器，每个元素包含有效的物品对象及其数量</returns>
        /// <remarks>
        /// 该方法会过滤掉 null 值和类型为 ItemID.None 的无效物品。
        /// 使用 yield return 实现延迟加载。
        /// </remarks>
        public static IEnumerable<(Item item, int stack)> WithStack(this Item[] items)
        {
            foreach (var item in items)
            {
                if (item != null && item.type > ItemID.None)
                    yield return (item, item.stack);
            }
        }
    }

    /// <summary>
    /// 提供与数值处理相关的实用工具方法
    /// </summary>
    public static class ValueUtils
    {
        /// <summary>
        /// 根据小数部分进行概率性舍入
        /// </summary>
        /// <param name="value">输入的小数值（格式为 a+b，其中 a 为整数部分，b 为小数部分）</param>
        /// <returns>以 b 的概率返回上界（a+1），否则返回下界（a）</returns>
        /// <example>
        /// 例如：ProbabilisticRound(3.7) 有 70% 的概率返回 4，30% 的概率返回 3
        /// </example>
        public static int ProbabilisticRound(float value)
        {
            int floor = (int)MathF.Floor(value);
            float decimalPart = value - floor;
            
            return Main.rand.NextFloat() < decimalPart ? floor + 1 : floor;
        }

// ... existing code ...
        /// <summary>
        /// 根据精度自动格式化浮点数为字符串
        /// </summary>
        /// <param name="value">要格式化的浮点数值</param>
        /// <param name="forcePercent">如果为 true，则强制输出百分比格式（乘以 100 并添加%符号）</param>
        /// <returns>格式化后的字符串</returns>
        /// <remarks>
        /// 该方法会依次尝试将数值四舍五入到 0 位、1 位、2 位小数。
        /// 如果四舍五入后的值与原值的差小于 EPSILON（0.001），则使用该精度格式化。
        /// 优先级：整数 > 1 位小数 > 2 位小数
        /// 当 forcePercent 为 true 时，会将数值乘以 100 并添加%符号
        /// </remarks>
        public static string FormatValue(float value, bool forcePercent = false)
        {
            bool usePercent = forcePercent ;
            float processedValue = usePercent ? value * 100 : value;
            
            float EPSILON = 0.001f;
            float roundedTo0 = (float)Math.Round(processedValue, 0);
            float roundedTo1 = (float)Math.Round(processedValue, 1);
            float roundedTo2 = (float)Math.Round(processedValue, 2);
            
            string result;
            if (Math.Abs(processedValue - roundedTo0) < EPSILON)
            {
                result = roundedTo0.ToString("F0");
            }
            else if (Math.Abs(processedValue - roundedTo1) < EPSILON)
            {
                result = roundedTo1.ToString("F1");
            }
            else
            {
                result = roundedTo2.ToString("F2");
            }
            
            return usePercent ? result + "%" : result;
        }
// ... existing code ...

        /// <summary>
        /// 计算两个整数的最大公约数（GCD）
        /// </summary>
        /// <param name="a">第一个整数</param>
        /// <param name="b">第二个整数</param>
        /// <returns>最大公约数</returns>
        private static int GreatestCommonDivisor(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            
            return a;
        }

        /// <summary>
        /// 格式化分数为 a/b 形式（自动约分）
        /// </summary>
        /// <param name="numerator">分子</param>
        /// <param name="denominator">分母</param>
        /// <returns>格式化后的分数字符串</returns>
        /// <remarks>
        /// 该方法会自动约分到最简分数形式。
        /// 如果分母为 1，只返回分子。
        /// 如果分子为 0，返回 "0"。
        /// </remarks>
        public static string FormatFraction(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("分母不能为零", nameof(denominator));
            }

            if (numerator == 0)
            {
                return "0";
            }

            int gcd = GreatestCommonDivisor(numerator, denominator);
            int simplifiedNumerator = numerator / gcd;
            int simplifiedDenominator = denominator / gcd;

            if (simplifiedDenominator == 1)
            {
                return simplifiedNumerator.ToString();
            }

            return $"{simplifiedNumerator}/{simplifiedDenominator}";
        }

        /// <summary>
        /// 格式化分数为 1/a 形式
        /// </summary>
        /// <param name="denominator">分母</param>
        /// <returns>格式化后的分数字符串</returns>
        /// <remarks>
        /// 该方法是 FormatFraction 的便捷重载，固定分子为 1。
        /// 如果分母为 1，返回 "1"。
        /// </remarks>
        public static string FormatFraction(int denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("分母不能为零", nameof(denominator));
            }

            return FormatFraction(1, denominator);
        }

        /// <summary>
        /// 对两位整数数组进行分式约分并返回格式化字符串
        /// </summary>
        /// <param name="values">包含两个整数的数组，第一个元素为分子，第二个元素为分母</param>
        /// <returns>约分后的分数字符串（格式为 a/b）</returns>
        /// <remarks>
        /// 该方法会自动将分数约分到最简形式。
        /// 如果分母为 1，只返回分子。
        /// 如果分子为 0，返回 "0"。
        /// </remarks>
        /// <example>
        /// 例如：fractionValue(new int[] {4, 8}) 返回 "1/2"
        /// fractionValue(new int[] {10, 5}) 返回 "5"
        /// fractionValue(new int[] {0, 3}) 返回 "0"
        /// </example>
        public static string FormatFraction(int[] values)
        {
            if (values == null || values.Length != 2)
            {
                throw new ArgumentException("输入数组必须包含两个整数", nameof(values));
            }

            return FormatFraction(values[0], values[1]);
        }
         /// <summary>
        /// 将两个整数的数组转换为浮点数
        /// </summary>
        /// <param name="values">包含两个整数的数组，第一个元素为被除数，第二个元素为除数</param>
        /// <returns>计算后的浮点数值（被除数/除数）</returns>
        /// <remarks>
        /// 该方法会将两个整数相除得到浮点数结果。
        /// 如果分母为 0，将抛出 DivideByZeroException 异常。
        /// </remarks>
        /// <example>
        /// 例如：ConvertToFloat(new int[] {5, 2}) 返回 2.5f
        /// ConvertToFloat(new int[] {10, 4}) 返回 2.5f
        /// ConvertToFloat(new int[] {7, 2}) 返回 3.5f
        /// </example>
        public static float ConvertToFloat(int[] values)
        {
            if (values == null || values.Length != 2)
            {
                throw new ArgumentException("输入数组必须包含两个整数", nameof(values));
            }

            return ConvertToFloat(values[0], values[1]);
        }

        /// <summary>
        /// 将两个整数转换为浮点数
        /// </summary>
        /// <param name="numerator">分子（被除数）</param>
        /// <param name="denominator">分母（除数）</param>
        /// <returns>计算后的浮点数值（numerator/denominator）</returns>
        /// <exception cref="DivideByZeroException">当 denominator 为 0 时抛出</exception>
        public static float ConvertToFloat(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("分母不能为零");
            }

            return (float)numerator / denominator;
        }
    }
}