// using Microsoft.Xna.Framework;
// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using System.Collections.Generic;
// using System;
// using ExpansionKele.Content.Customs;
// using Terraria.WorldBuilding;
// using ExpansionKele.Content.Buff;
// using Terraria.DataStructures;
// using Microsoft.Xna.Framework.Graphics;

// namespace ExpansionKele.Content.StarySummon{
// public class StarBlade : ModItem
// {
    

//     public override void SetDefaults()
// {
//     Item.damage = 20;
//     Item.DamageType = DamageClass.Summon;
//     Item.width = 120;
//     Item.height = 120;
//     Item.useTime = 20;
//     Item.useAnimation = 20;
//     Item.useStyle = ItemUseStyleID.Swing;
//     Item.noMelee = false;
//     Item.knockBack = 6;
//     Item.value = Item.sellPrice(0, 1, 0, 0);
//     Item.rare = ItemRarityID.Pink;
//     Item.UseSound = SoundID.Item1;
//     Item.autoReuse = true;
//     Item.shoot = ModContent.ProjectileType<StarBladeGlowProjectile>();
//     Item.shootSpeed = 0f;
//     Item.buffType = ModContent.BuffType<StarBladeBuff>();
//     Item.buffTime = 3600;
// }

// public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
// {
//     // 调试信息
//     Main.NewText("StarBlade Shoot called");

//     // 获取玩家的中心位置
//     Vector2 playerCenter = player.Center+ new Vector2(-0,-0);

//     // 创建发射的Projectile实例
//     Projectile.NewProjectile(source, playerCenter, velocity, Item.shoot, damage, knockback, player.whoAmI);

//     // 给玩家添加buff
//     player.AddBuff(Item.buffType, Item.buffTime);
    
//     return false; // 返回false表示我们已经处理了发射逻辑
// }
// public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) 
//     {
        
//         target.AddBuff(ModContent.BuffType<WhipMark>(), 600);
//     }

    


//     public override void AddRecipes()
//     {
//         CreateRecipe()
//             .AddIngredient(ItemID.AdamantiteBar, 12)
//             .AddIngredient(ItemID.SoulofLight, 10)
//             .AddTile(TileID.MythrilAnvil)
//             .Register();
//     }
// }



// public class StarBladeGlowProjectile : ModProjectile
// {
//     public override void SetStaticDefaults()
//     {
//         // Main.projFrames[Projectile.type] = 1;
//         // ProjectileID.Sets.MinionShot[Projectile.type] = true;
//     }

//     public override void SetDefaults()
// {
//     Projectile.width = 512; // 设置宽度
//     Projectile.height = 512; // 设置高度
//     Projectile.aiStyle = -1;
//     Projectile.friendly = true;
//     Projectile.hostile = false;
//     Projectile.tileCollide = false; // 允许碰撞瓦片
//     Projectile.ignoreWater = true;
//     Projectile.penetrate = -1; // 穿透所有敌人
//     Projectile.timeLeft = 2; // 设置一个较长的生存时间
//     Projectile.minion = false; // 不是小兵
//     Projectile.minionSlots = 0f;
//     Projectile.netImportant = true;
//     Projectile.alpha = 255;
// }

//     public override void AI()
//     {
//         // Player player = Main.player[Projectile.owner];
//         // if (player.dead || !player.active)
//         // {
//         //     Projectile.Kill();
//         //     return;
//         // }

//         // // 设置抛射体的透明度和颜色
        
//         // Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 1f);

//         // // 添加重力效果（可选）
//         // Projectile.velocity.Y += 0.2f;

//         // // 可以添加碰撞检测和反弹逻辑（可选）
//         // if (Projectile.velocity.Y > 16f)
//         // {
//         //     Projectile.velocity.Y = 16f;
//         // }
//     }

//     public override void PostDraw(Color lightColor)
// {
    // //Texture2D texture = ModContent.Request<Texture2D>("ExpansionKele/Content/Summon/StarBladeGlowProjectile").Value;
//     Vector2 drawOrigin = new Vector2(texture.Width, texture.Height) / 2f;
//     Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
// }
// }







// public class StarPartner : ModProjectile
// {
//     public override void SetStaticDefaults()
//     {
//         Main.projFrames[Projectile.type] = 1;
//         ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
//         ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
//     }

//     public override void SetDefaults()
// {
//     Projectile.width = 32;
//     Projectile.height = 32;
//     Projectile.aiStyle = -1;
//     Projectile.friendly = true;
//     Projectile.hostile = false;
//     Projectile.tileCollide = false;
//     Projectile.ignoreWater = true;
//     Projectile.penetrate = -1;
//     Projectile.timeLeft = 18000;
//     Projectile.minion = true;
//     Projectile.minionSlots = 0f; // 不消耗仆从槽位
//     Projectile.netImportant = true;
// }

// public override void AI()
// {
//     Player player = Main.player[Projectile.owner];
//     if (player.dead || !player.active)
//     {
//         Projectile.Kill();
//         return;
//     }

//     if (player.HasBuff(ModContent.BuffType<StarBladeBuff>()))
//     {
//         Projectile.timeLeft = 2;
//     }

//     Vector2 targetPos = player.Center + new Vector2(0, -50); // 玩家上方
//     Projectile.velocity = (targetPos - Projectile.Center) * 1f;

//     // 发射追踪导弹
//     if (Main.rand.NextBool(60)) // 每60帧发射一次
//     {
//         Vector2 missileVelocity = Main.rand.NextVector2CircularEdge(1f, 1f) * 8f;
//         Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, missileVelocity, ModContent.ProjectileType<StarBladeProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
//     }
// }

//     public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
//     {
//         if (target.HasBuff(ModContent.BuffType<WhipMark>()))
//         {
//             modifiers.SourceDamage *= 1.5f;
//             modifiers.CritDamage *= 1.1f;
//         }
//     }
// }



//     public class StarBladeProjectile : ModProjectile
// {
//     public override void SetStaticDefaults()
//     {
        
//         Main.projFrames[Projectile.type] = 1;
//         ProjectileID.Sets.MinionShot[Projectile.type] = true;
//     }

//     public override void SetDefaults()
//     {
//         Projectile.width = 16;
//         Projectile.height = 16;
//         Projectile.aiStyle = -1;
//         Projectile.friendly = true;
//         Projectile.hostile = false;
//         Projectile.tileCollide = false;
//         Projectile.ignoreWater = true;
//         Projectile.penetrate = 1;
//         Projectile.timeLeft = 600;
//         Projectile.minion = true;
//         Projectile.minionSlots = 0f;
//         Projectile.netImportant = true;
//     }

//     public override void AI()
//     {
//         float maxTrackingDistance = 960;
//         float speed = 30f;
//         float turnResistance = 10f;
//         Vector2 mousePosition = Main.MouseWorld;

//         ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed, maxTrackingDistance, turnResistance, mousePosition);
//     }

//     public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//     {
//         target.immune[Projectile.owner] = 1;
        
//     }

//     public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
//     {
        
//     }
// }




//     public class StarBladeBuff : ModBuff
// {
//     public override void SetStaticDefaults()
//     {
        
//         Main.buffNoSave[Type] = true;
//         Main.buffNoTimeDisplay[Type] = true;
//     }

//     public override void Update(Player player, ref int buffIndex)
// {
//     if (player.ownedProjectileCounts[ModContent.ProjectileType<StarPartner>()] <= 0)
//     {
//         Projectile.NewProjectile(
//             player.GetSource_Buff(buffIndex), 
//             player.Center, 
//             Vector2.Zero, 
//             ModContent.ProjectileType<StarPartner>(), 
//             player.HeldItem.damage, 
//             player.HeldItem.knockBack, 
//             player.whoAmI
//         );
//     }

//     player.GetModPlayer<StarBladePlayer>().StarBladeActive = true;
// }
// }


//     public class WhipMark : ModBuff
// {
//     public override void SetStaticDefaults()
// {
//     Main.debuff[Type] = true;
//     Main.pvpBuff[Type] = true;
//     Main.buffNoSave[Type] = true;
// }

// public override void Update(NPC npc, ref int buffIndex)
// {
//     npc.GetGlobalNPC<StarBladeGlobalNPC>().WhipMarked = true;
// }   
// }


// public class StarBladeGlobalNPC : GlobalNPC
// {
//     public bool WhipMarked;

//     public override bool InstancePerEntity => true;

//     public override void ResetEffects(NPC npc)
//     {
//         WhipMarked = false;
//     }

//     public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
//     {
//         if (WhipMarked)
//         {
//             modifiers.SourceDamage *= 1.5f;
//             modifiers.CritDamage *= 1.1f;
//         }
//     }
// }



// public class StarBladePlayer : ModPlayer
// {
//     public bool StarBladeActive;

//     public override void ResetEffects()
//     {
//         StarBladeActive = false;
//     }

//     public override void UpdateEquips()
//     {
//         if (StarBladeActive)
//         {
            
//         }
//     }
// }








// }
