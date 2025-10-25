using System;
using System.IO;
using System.Reflection;
using Terraria.ModLoader;

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
        /// 
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
    }
}