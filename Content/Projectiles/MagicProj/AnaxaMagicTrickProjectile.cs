using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.IO; // 添加IO命名空间用于网络同步

namespace ExpansionKele.Content.Projectiles.MagicProj
{
	public class AnaxaMagicTrickProjectile : ModProjectile
	{
		public override string LocalizationCategory => "Projectiles.MagicProj";
		
		// 网络同步用的AI索引常量
		private const int HAS_HIT_FIRST_ENEMY_INDEX = 0;
		private const int CURRENT_TARGET_INDEX = 1;
		private const int HIT_COUNT_INDEX = 2;
		
		// 记录已经被该弹幕击中的NPC ID列表（仅在拥有者客户端维护）
		private List<int> hitNPCs = new List<int>();
		
		// 当前追踪的目标
		private NPC currentTarget = null;
		
		// 是否已经击中过第一个敌人
		private bool hasHitFirstEnemy = false;
		
		// 击中计数器
		private int hitCount = 0;
		
		
		// 最大穿透数
		private const int MAX_PENETRATE = 5;
		
		// 定义要应用的减益类型数组
		private readonly int[] DebuffTypes = {
			BuffID.Ichor,        // 灵液
			BuffID.CursedInferno, // 咒火
			BuffID.Frostburn,    // 霜冻
			BuffID.Venom,     // 酸性毒液
			BuffID.Confused,     // 困惑
			BuffID.Oiled,        // 涂油
			BuffID.ShadowFlame,  // 暗影焰
		};
		
		// 减益持续时间（6秒）
		private const int DEBUFF_DURATION = 360;
		
		// 添加纹理缓存支持
		private static Asset<Texture2D> _cachedTexture;

		public override void Load()
		{
			// 预加载纹理资源
			_cachedTexture = ModContent.Request<Texture2D>(Texture);
		}

		public override void Unload()
		{
			// 清理资源引用
			_cachedTexture = null;
		}
		
		public override void SetStaticDefaults()
		{
			// 设置尾迹缓存长度和模式
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = MAX_PENETRATE; // 穿透5个敌人
			Projectile.timeLeft = 600;
			Projectile.light = 0.6f;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 50; // extraUpdates为50
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10; // 局部无敌帧为10
            Projectile.netUpdate = true;
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
		{
			// 初始化网络同步数据
			if (Main.myPlayer == Projectile.owner)
			{
				Projectile.ai[HAS_HIT_FIRST_ENEMY_INDEX] = 0f; // false
				Projectile.ai[CURRENT_TARGET_INDEX] = -1f; // 无目标
				Projectile.ai[HIT_COUNT_INDEX] = 0f;
				Projectile.netUpdate = true;
			}
		}

		public override void AI()
		{
			// 同步网络数据
			SyncNetworkData();
			
			// 添加粒子效果
			if (Main.rand.NextBool(3))
			{
				// 添加额外的青色发光粒子
				Dust glowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
					DustID.Electric, 0f, 0f, 100, Color.Cyan, 1f);
				glowDust.noGravity = true;
				glowDust.velocity *= 0.2f;
			}

			// 设置旋转角度
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			
			// 只有拥有者客户端执行核心逻辑
			if (Main.myPlayer == Projectile.owner)
			{
				// 检测并处理与NPC的碰撞
				CheckNPCCollision();
				
				// 如果还没有击中第一个敌人，保持直线飞行
				if (!hasHitFirstEnemy)
				{
					UpdateNetworkData(); // 更新网络数据
					return;
				}
				
				// 已经击中第一个敌人，开始搜寻新的目标
				if (currentTarget == null || !IsValidTarget(currentTarget))
				{
					currentTarget = FindNextTarget();
					if (currentTarget == null)
					{
						// 没有找到合适的敌人，销毁弹幕
						Projectile.Kill();
						return;
					}
				}
				
				// 向目标移动（直线飞行）
				Vector2 direction = currentTarget.Center - Projectile.Center;
				direction.Normalize();
				Projectile.velocity = direction * Projectile.velocity.Length();
				
				// 更新网络数据
				UpdateNetworkData();
			}
			else
			{
				// 非拥有者客户端根据网络数据更新视觉效果
				if (hasHitFirstEnemy && currentTarget != null && currentTarget.active)
				{
					// 简单的视觉同步 - 跟随目标大致方向
					Vector2 direction = currentTarget.Center - Projectile.Center;
					if (direction.Length() > 10f)
					{
						direction.Normalize();
						Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), 0.1f);
					}
				}
			}
		}
		
		// 网络数据同步方法
		private void SyncNetworkData()
		{
			bool newHasHitFirst = Projectile.ai[HAS_HIT_FIRST_ENEMY_INDEX] > 0f;
			int newTargetIndex = (int)Projectile.ai[CURRENT_TARGET_INDEX];
			int newHitCount = (int)Projectile.ai[HIT_COUNT_INDEX];
			
			// 只有当数据真正改变时才更新本地变量
			if (newHasHitFirst != hasHitFirstEnemy || newHitCount != hitCount)
			{
				hasHitFirstEnemy = newHasHitFirst;
				hitCount = newHitCount;
				
				// 更新目标引用
				if (newTargetIndex >= 0 && newTargetIndex < Main.maxNPCs && Main.npc[newTargetIndex].active)
				{
					currentTarget = Main.npc[newTargetIndex];
				}
				else
				{
					currentTarget = null;
				}
			}
		}
		
		// 更新网络数据
		private void UpdateNetworkData()
		{
			Projectile.ai[HAS_HIT_FIRST_ENEMY_INDEX] = hasHitFirstEnemy ? 1f : 0f;
			Projectile.ai[CURRENT_TARGET_INDEX] = currentTarget?.whoAmI ?? -1f;
			Projectile.ai[HIT_COUNT_INDEX] = hitCount;
		}
		
		// 新增：检查与NPC的碰撞并处理击中逻辑
		private void CheckNPCCollision()
		{
			// 遍历所有活跃的NPC
			foreach (NPC npc in Main.ActiveNPCs)
			{
				// 检查碰撞条件
				if (npc.active && 
					!npc.friendly && 
					!npc.dontTakeDamage && 
					!npc.immortal &&
					npc.life > 0 &&
					Projectile.getRect().Intersects(npc.getRect()))
				{	
					// 处击中逻辑
					HandleNPCImpact(npc);
				}
			}
		}
		
		// 新增：处理NPC击中逻辑
		private void HandleNPCImpact(NPC target)
		{
			// 记录击中的NPC
			if (!hitNPCs.Contains(target.whoAmI))
			{
				hitNPCs.Add(target.whoAmI);
				hitCount++;
			}
			
			// 标记已击中第一个敌人
			if (!hasHitFirstEnemy)
			{
				hasHitFirstEnemy = true;
			}
			
			// 清除当前目标，下次AI会寻找新目标
			currentTarget = null;
			
			
			// 触发网络同步
			Projectile.netUpdate = true;
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
			}
			
			// 达到最大穿透数时销毁
			if (hitCount >= MAX_PENETRATE)
			{
				Projectile.Kill();
			}
		}
		
		// 应用随机减益的方法
		private void ApplyRandomDebuff(NPC target)
		{
			// 获取目标当前拥有的减益
			List<int> currentDebuffs = new List<int>();
			for (int i = 0; i < target.buffType.Length; i++)
			{
				if (target.buffType[i] > 0 && target.buffTime[i] > 0)
				{
					// 检查是否是我们定义的减益类型之一
					foreach (int debuffType in DebuffTypes)
					{
						if (target.buffType[i] == debuffType)
						{
							currentDebuffs.Add(debuffType);
							break;
						}
					}
				}
			}
			
			// 如果已经拥有全部减益，则找到剩余时间最少的减益进行刷新
			if (currentDebuffs.Count >= DebuffTypes.Length)
			{
				int minTime = int.MaxValue;
				int debuffToRefresh = -1;
				int buffIndexToRefresh = -1;
				
				// 找到剩余时间最少的减益
				for (int i = 0; i < target.buffType.Length; i++)
				{
					if (target.buffType[i] > 0 && target.buffTime[i] > 0)
					{
						foreach (int debuffType in DebuffTypes)
						{
							if (target.buffType[i] == debuffType && target.buffTime[i] < minTime)
							{
								minTime = target.buffTime[i];
								debuffToRefresh = debuffType;
								buffIndexToRefresh = i;
								break;
							}
						}
					}
				}
				
				// 刷新该减益
				if (buffIndexToRefresh >= 0)
				{
					target.DelBuff(buffIndexToRefresh);
					target.AddBuff(debuffToRefresh, DEBUFF_DURATION);
				}
			}
			else
			{
				// 优先选择目标尚未拥有的减益
				List<int> availableDebuffs = new List<int>();
				foreach (int debuffType in DebuffTypes)
				{
					if (!currentDebuffs.Contains(debuffType))
					{
						availableDebuffs.Add(debuffType);
					}
				}
				
				// 如果有可用的减益，则随机选择一个应用
				if (availableDebuffs.Count > 0)
				{
					int randomDebuff = availableDebuffs[Main.rand.Next(availableDebuffs.Count)];
					target.AddBuff(randomDebuff, DEBUFF_DURATION);
				}
				else
				{
					// 理论上不应该到达这里，但作为保险
					int randomDebuff = DebuffTypes[Main.rand.Next(DebuffTypes.Length)];
					target.AddBuff(randomDebuff, DEBUFF_DURATION);
				}
			}
		}
		
		// 检查目标是否有效
		private bool IsValidTarget(NPC npc)
		{
			return npc != null && 
				   npc.active && 
				   !npc.friendly && 
				   !npc.dontTakeDamage && 
				   !npc.immortal &&
				   npc.life > 0 &&
				   Vector2.Distance(Projectile.Center, npc.Center) <= 600f;
		}
		
		// 寻找下一个目标
				// 寻找下一个目标
		private NPC FindNextTarget()
		{
			NPC bestTarget = null;
			float bestDistance = 600f;
			
			// 优先寻找未被该弹幕击中的敌人
			foreach (var npc in Main.ActiveNPCs)
			{
				if (IsValidTarget(npc) && !hitNPCs.Contains(npc.whoAmI))
				{
					float distance = Vector2.Distance(Projectile.Center, npc.Center);
					if (distance < bestDistance)
					{
						bestDistance = distance;
						bestTarget = npc;
					}
				}
			}
			
			// 如果没有未击中的敌人，则随机选择一个敌人
			if (bestTarget == null)
			{
				List<NPC> validTargets = new List<NPC>();
				foreach (var npc in Main.ActiveNPCs)
				{
					if (IsValidTarget(npc))
					{
						validTargets.Add(npc);
					}
				}
				
				// 如果有有效的敌人，随机选择一个
				if (validTargets.Count > 0)
				{
					bestTarget = validTargets[Main.rand.Next(validTargets.Count)];
				}
			}
			
			return bestTarget;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 如果击中未标记的敌人（未在此弹幕的击中列表中），则伤害增加 30%
            if (!hitNPCs.Contains(target.whoAmI))
            {
                Projectile.damage = (int)(Projectile.damage * 1.3f);
            }
            // 应用随机减益效果
            ApplyRandomDebuff(target);
        }

        public override void OnKill(int timeLeft)
		{
			// 击中时产生粒子效果
			// for (int i = 0; i < 12; i++)
			// {
			// 	// 添加爆炸效果
			// 	Dust explosionDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
			// 		DustID.Electric, 0f, 0f, 100, Color.Purple, 0.5f);
			// 	explosionDust.noGravity = true;
			// 	explosionDust.velocity *= 4f;
			// }
		}
		
		public override bool PreDraw(ref Color lightColor)
		{
			// 获取纹理
			Texture2D texture = _cachedTexture.Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			
			// 紫色主色调
			Color mainColor = new Color(180, 100, 255, 100);
			
			// 绘制尾迹效果
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin;
				float scale = Projectile.scale * (1f - k / (float)Projectile.oldPos.Length) * 0.8f;
				Color trailColor = mainColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.6f;
				Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, drawOrigin, scale, SpriteEffects.None, 0);
			}
			
			// 绘制主弹幕
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			
			// 添加发光效果
			Lighting.AddLight(Projectile.Center, mainColor.ToVector3() * 0.5f);
			
			return false;
		}
		
		// 网络接收处理
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			// 接收额外的AI数据（如果需要存储更复杂的状态）
			hasHitFirstEnemy = reader.ReadBoolean();
			int targetIndex = reader.ReadInt32();
			hitCount = reader.ReadInt32();
			
			// 更新目标引用
			if (targetIndex >= 0 && targetIndex < Main.maxNPCs && Main.npc[targetIndex].active)
			{
				currentTarget = Main.npc[targetIndex];
			}
			else
			{
				currentTarget = null;
			}
		}
		
		public override void SendExtraAI(BinaryWriter writer)
		{
			// 发送额外的AI数据
			writer.Write(hasHitFirstEnemy);
			writer.Write(currentTarget?.whoAmI ?? -1);
			writer.Write(hitCount);
		}
	}
}