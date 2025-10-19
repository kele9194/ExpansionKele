using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;

namespace ExpansionKele.Content.Customs
{
    public static class ProjectileHelper
    {
        public static void FindAndMoveTowardsTarget(Terraria.Projectile projectile, float speed, float maxTrackingDistance, float turnResistance)
        {
            // 寻找最近的空中目标
            NPC target = FindNearestAerialTarget(projectile.Center, maxTrackingDistance);
            if (target != null)
            {
                Vector2 direction = target.Center - projectile.Center;
                float distance = direction.Length();
                direction.Normalize();

                // 平滑追踪目标
                projectile.velocity = (projectile.velocity * (turnResistance - 1) + direction * speed) / turnResistance;

                // 更新导弹的方向
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                // 如果没有找到目标，导弹继续沿当前方向飞行
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public static void FindAndMoveTowardsTarget(Terraria.Projectile projectile, float speed, float maxTrackingDistance, float turnResistance, Vector2 mousePosition)
        {
            // 寻找鼠标所指方向的目标
            NPC target = FindNearestAerialTargetTowardsMouse(projectile.Center, maxTrackingDistance, mousePosition);
            

            if (target != null)
            {
                Vector2 direction = target.Center - projectile.Center;
                float distance = direction.Length();
                direction.Normalize();

                // 平滑追踪目标
                projectile.velocity = (projectile.velocity * (turnResistance - 1) + direction * speed) / turnResistance;

                // 更新导弹的方向
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                // 如果没有找到目标，导弹继续沿当前方向飞行
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        private static NPC FindNearestAerialTarget(Vector2 position, float maxDistance)
{
    NPC nearestTarget = null;
    float minDistance = float.MaxValue;

    for (int i = 0; i < Main.maxNPCs; i++)
    {
        NPC npc = Main.npc[i];
        if (npc.active && !npc.friendly && (npc.type != NPCID.TargetDummy) && !npc.immortal && !npc.dontTakeDamage)
        {
            float distance = Vector2.Distance(npc.Center, position);
            if (distance < minDistance && distance <= maxDistance)
            {
                minDistance = distance;
                nearestTarget = npc;
            }
        }
    }

    return nearestTarget;
}

private static float AngleBetween(Vector2 vector1, Vector2 vector2)
{
    float angle1 = vector1.ToRotation();
    float angle2 = vector2.ToRotation();
    return Math.Abs(angle1 - angle2);
}

private static NPC FindNearestAerialTargetTowardsMouse(Vector2 position, float maxDistance, Vector2 mousePosition)
{
    NPC nearestTarget = null;
    float minAngleDifference = float.MaxValue;

    for (int i = 0; i < Main.maxNPCs; i++)
    {
        NPC npc = Main.npc[i];
        if (npc.active && !npc.friendly && (npc.type != NPCID.TargetDummy) && !npc.immortal && !npc.dontTakeDamage)
        {
            float distance = Vector2.Distance(npc.Center, position);
            if (distance <= maxDistance)
            {
                Vector2 directionToNpc = npc.Center - position;
                Vector2 directionToMouse = mousePosition - position;
                float angleDifference = AngleBetween(directionToNpc, directionToMouse);

                if (angleDifference < minAngleDifference)
                {
                    minAngleDifference = angleDifference;
                    nearestTarget = npc;
                }
            }
        }
    }

    return nearestTarget;
}

private static NPC FindNearestAerialTargetByHealth(Vector2 position, float maxDistance)
{
    NPC nearestTarget = null;
    int maxHealth = int.MinValue;

    for (int i = 0; i < Main.maxNPCs; i++)
    {
        NPC npc = Main.npc[i];
        if (npc.active && !npc.friendly && (npc.type != NPCID.TargetDummy) && !npc.immortal && !npc.dontTakeDamage)
        {
            float distance = Vector2.Distance(npc.Center, position);
            if (distance <= maxDistance && npc.life > maxHealth)
            {
                maxHealth = npc.life;
                nearestTarget = npc;
            }
        }
    }

    return nearestTarget;
}
    }
}