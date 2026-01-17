using System;
using System.IO;
using System.Reflection;
using Terraria.ModLoader;
using Terraria;

namespace ExpansionKele.Content.Customs
{
    public static class TexturePathHelper
    {
        /// <summary>
        /// 根据相对路径获取纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="relativePath">相对路径，支持 ./ 和 ../ </param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTexturePath(ModItem item, string relativePath)
        {
            // 获取物品类型的完整名称
            Type itemType = item.GetType();
            string fullName = itemType.FullName; // 例如: ExpansionKele.Content.StaryMelee.StarySwordA
            
            // 转换命名空间为路径格式
            string basePath = fullName.Replace('.', '/');
            
            // 移除类名部分，只保留命名空间路径
            int lastSlash = basePath.LastIndexOf('/');
            if (lastSlash > 0)
            {
                basePath = basePath.Substring(0, lastSlash);
            }
            
            // 处理相对路径
            if (relativePath.StartsWith("./"))
            {
                // 当前目录
                string fileName = relativePath.Substring(2); // 移除 "./"
                return basePath + "/" + fileName;
            }
            else if (relativePath.StartsWith("../"))
            {
                // 父级目录
                string[] parts = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string[] basePathParts = basePath.Split('/');
                
                int levelUp = 0;
                int i = 0;
                while (i < parts.Length && parts[i] == "..")
                {
                    levelUp++;
                    i++;
                }
                
                // 构建基础路径
                if (levelUp >= basePathParts.Length)
                {
                    // 如果向上级数超过了路径层级，则返回根路径
                    basePath = "ExpansionKele";
                }
                else
                {
                    // 向上移动指定层级
                    int newLength = basePathParts.Length - levelUp;
                    basePath = string.Join("/", basePathParts, 0, newLength);
                }
                
                // 添加剩余路径
                string remainingPath = string.Join("/", parts, i, parts.Length - i);
                if (!string.IsNullOrEmpty(remainingPath))
                {
                    return basePath + "/" + remainingPath;
                }
                else
                {
                    // 如果没有剩余路径，使用类名
                    return basePath + "/" + itemType.Name;
                }
            }
            
            // 如果不是相对路径，直接返回
            return relativePath;
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(ModItem item, string defaultTexturePath)
        {
            // 获取物品类型名称作为基础文件名
            Type itemType = item.GetType();
            string itemName = itemType.Name;
            
            // 构建预期的纹理路径（ModItem 默认查找的路径）
            string expectedTexturePath = itemType.FullName.Replace('.', '/') + ".png";
            
            // 检查默认纹理是否存在
            if (ModContent.FileExists(expectedTexturePath))
            {
                return expectedTexturePath;
            }
            
            // 如果默认路径不存在，检查是否指定了默认纹理且存在
            if (!string.IsNullOrEmpty(defaultTexturePath) && ModContent.FileExists(defaultTexturePath))
            {
                return defaultTexturePath;
            }
            
            // 如果两者都不存在，返回原始默认路径（即使不存在）
            return expectedTexturePath;
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(ModItem item, params string[] fallbackPaths)
        {
            // 首先尝试使用默认的纹理路径（根据类名生成）
            Type itemType = item.GetType();
            string expectedTexturePath = itemType.FullName.Replace('.', '/') + ".png";
            
            if (ModContent.FileExists(expectedTexturePath))
            {
                return expectedTexturePath;
            }
            
            // 检查备选路径
            foreach (string path in fallbackPaths)
            {
                if (!string.IsNullOrEmpty(path) && ModContent.FileExists(path))
                {
                    return path;
                }
            }
            
            // 如果都没有找到，返回默认路径
            return expectedTexturePath;
        }
    }
    
    // 为ModItem添加扩展方法
    public static class ModItemExtensions
    {
        /// <summary>
        /// 获取基于相对路径的纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="relativePath">相对路径</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetRelativeTexturePath(this ModItem item, string relativePath)
        {
            return TexturePathHelper.GetTexturePath(item, relativePath);
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(this ModItem item, string defaultTexturePath)
        {
            return TexturePathHelper.GetTexturePathOrDefault(item, defaultTexturePath);
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(this ModItem item, params string[] fallbackPaths)
        {
            return TexturePathHelper.GetTexturePathWithFallback(item, fallbackPaths);
        }
    }
}