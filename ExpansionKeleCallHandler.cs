// using System;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework;
// using System.Reflection;
// using Terraria;
// using ExpansionKele.Content.StaryMelee;
// using ExpansionKele.Content.StarySniper;
// using ExpansionKele.Content.StaryMagic;

// namespace ExpansionKele
// {
//     public static class ExpansionKeleCallHandler
//     {
//         public static object HandleCall(Mod mod, params object[] args)
//         {
//             try
//             {
//                 // 确保至少有一个参数
//                 if (args == null || args.Length == 0)
//                     return null;

//                 // 第一个参数应该是操作名称
//                 string operation = args[0] as string;

//                 // 根据操作名称执行不同的功能
//                 switch (operation)
//                 {
//                     // 创建 StarySwordAbs 的派生类实例
//                     case "CreateStarySword":
//                         if (args.Length >= 3)
//                         {
//                             string modName = args[1] as string;
//                             string typeName = args[2] as string;
//                             return CreateStarySword(modName, typeName);
//                         }
//                         break;

//                     // 创建 GaSniperAbs 的派生类实例
//                     case "CreateGaSniper":
//                         if (args.Length >= 3)
//                         {
//                             string modName = args[1] as string;
//                             string typeName = args[2] as string;
//                             return CreateGaSniper(modName, typeName);
//                         }
//                         break;

//                     // 创建 AbsStarStaff 的派生类实例
//                     case "CreateStarStaff":
//                         if (args.Length >= 3)
//                         {
//                             string modName = args[1] as string;
//                             string typeName = args[2] as string;
//                             return CreateStarStaff(modName, typeName);
//                         }
//                         break;

//                     // 获取抽象类中的虚拟属性值
//                     case "GetPropertyValue":
//                         if (args.Length >= 3)
//                         {
//                             object instance = args[1];
//                             string propertyName = args[2] as string;
//                             return GetPropertyValue(instance, propertyName);
//                         }
//                         break;

//                     // 设置抽象类中的虚拟属性值
//                     case "SetPropertyValue":
//                         if (args.Length >= 4)
//                         {
//                             object instance = args[1];
//                             string propertyName = args[2] as string;
//                             object value = args[3];
//                             SetPropertyValue(instance, propertyName, value);
//                             return true;
//                         }
//                         break;
                        
//                     // 获取预定义的 ProjectileType
//                     case "GetProjectileType":
//                         if (args.Length >= 2)
//                         {
//                             string projectileName = args[1] as string;
//                             return GetProjectileType(projectileName);
//                         }
//                         break;
//                 }
//             }
//             catch (Exception e)
//             {
//                 mod.Logger.Error($"Error in ExpansionKele.Call: {e.Message}");
//             }

//             return null;
//         }

//         // 创建 StarySwordAbs 派生类的实例
//         private static ModItem CreateStarySword(string modName, string typeName)
//         {
//             var mod = ModLoader.GetMod(modName);
//             if (mod == null) return null;

//             try
//             {
//                 var type = mod.Code.GetType($"{modName}.{typeName}");
//                 if (type != null && type.IsSubclassOf(typeof(StarySwordAbs)))
//                 {
//                     return (ModItem)Activator.CreateInstance(type);
//                 }
//             }
//             catch (Exception e)
//             {
//                 ModContent.GetInstance<ExpansionKele>().Logger.Error($"Error creating StarySword: {e.Message}");
//                 return null;
//             }

//             return null;
//         }

//         // 创建 GaSniperAbs 派生类的实例
//         private static ModItem CreateGaSniper(string modName, string typeName)
//         {
//             var mod = ModLoader.GetMod(modName);
//             if (mod == null) return null;

//             try
//             {
//                 var type = mod.Code.GetType($"{modName}.{typeName}");
//                 if (type != null && type.IsSubclassOf(typeof(GaSniperAbs)))
//                 {
//                     return (ModItem)Activator.CreateInstance(type);
//                 }
//             }
//             catch (Exception e)
//             {
//                 ModContent.GetInstance<ExpansionKele>().Logger.Error($"Error creating GaSniper: {e.Message}");
//                 return null;
//             }

//             return null;
//         }

//         // 创建 AbsStarStaff 派生类的实例
//         private static ModItem CreateStarStaff(string modName, string typeName)
//         {
//             var mod = ModLoader.GetMod(modName);
//             if (mod == null) return null;

//             try
//             {
//                 var type = mod.Code.GetType($"{modName}.{typeName}");
//                 if (type != null && type.IsSubclassOf(typeof(AbsStarStaff)))
//                 {
//                     return (ModItem)Activator.CreateInstance(type);
//                 }
//             }
//             catch (Exception e)
//             {
//                 ModContent.GetInstance<ExpansionKele>().Logger.Error($"Error creating StarStaff: {e.Message}");
//                 return null;
//             }

//             return null;
//         }

//         // 获取对象属性值
//         private static object GetPropertyValue(object instance, string propertyName)
//         {
//             if (instance == null) return null;

//             var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
//             if (property != null && property.CanRead)
//             {
//                 return property.GetValue(instance);
//             }

//             var field = instance.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
//             if (field != null)
//             {
//                 return field.GetValue(instance);
//             }

//             return null;
//         }

//         // 设置对象属性值
//         private static void SetPropertyValue(object instance, string propertyName, object value)
//         {
//             if (instance == null) return;

//             var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
//             if (property != null && property.CanWrite)
//             {
//                 property.SetValue(instance, value);
//                 return;
//             }

//             var field = instance.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
//             if (field != null)
//             {
//                 field.SetValue(instance, value);
//             }
//         }
        
//         // 获取预定义的 ProjectileType
//         private static int GetProjectileType(string projectileName)
//         {
//             switch (projectileName)
//             {
//                 case "AAMissile":
//                     return ModContent.ProjectileType<Content.Projectiles.AAMissile>();
//                 case "ColaProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.ColaProjectile>();
//                 case "ColaProjectileLower":
//                     return ModContent.ProjectileType<Content.Projectiles.ColaProjectileLower>();
//                 // case "FrostRayProjectile":
//                 //     return ModContent.ProjectileType<Content.Weapons.FrostRayProjectile>();
//                 case "FullMoonArrowProj":
//                     return ModContent.ProjectileType<Content.Projectiles.FullMoonArrowProj>();
//                 case "FullMoonEchoProj":
//                     return ModContent.ProjectileType<Content.Projectiles.FullMoonEchoProj>();
//                 case "FullMoonProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.FullMoonProjectile>();
//                 case "IronCurtainCannonLaser":
//                     return ModContent.ProjectileType<Content.Projectiles.IronCurtainCannonLaser>();
//                 case "IronCurtainCannonProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.IronCurtainCannonProjectile>();
//                 case "LifePercentageProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.LifePercentageProjectile>();
//                 case "MagicBlueProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.MagicBlueProjectile>();
//                 case "MagicCyanProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.MagicCyanProjectile>();
//                 case "MagicPurpleProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.MagicPurpleProjectile>();
//                 case "MagicRedProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.MagicRedProjectile>();
//                 case "MagicStarProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.MagicStarProjectile>();
//                 case "NeutronProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.NeutronProjectile>();
//                 case "SharkyBullet":
//                     return ModContent.ProjectileType<Content.Projectiles.SharkyBullet>();
//                 case "SharkyBulletPlus":
//                     return ModContent.ProjectileType<Content.Projectiles.SharkyBulletPlus>();
//                 case "SpectralCurtainCannonProj":
//                     return ModContent.ProjectileType<Content.Projectiles.SpectralCurtainCannonProj>();
//                 case "StingerProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.StingerProjectile>();
//                 case "VortexMainProjectile":
//                     return ModContent.ProjectileType<Content.Projectiles.VortexMainProjectile>();
//                 default:
//                     return 0;
//             }
//         }
//     }
// }