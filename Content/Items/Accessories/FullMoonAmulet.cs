using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.Accessories
{
    public class FullMoonAmulet : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var damageplayer=player.GetModPlayer<ExpansionKeleDamageMulti>();
            // 乘算增伤 -25% (应该是+25%吧？因为-25%是减伤)
            damageplayer.MultiplyMultiplicativeDamageBonus(0.75f);
            // +5穿甲
            player.GetArmorPenetration(DamageClass.Generic) += 5;
            
            // 获取玩家的特殊能力组件
            player.GetModPlayer<FullMoonAmuletPlayer>().hasFullMoonAmulet = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FullMoonBar>(3)
                .AddIngredient<SigwutBar>(3)
                .AddIngredient(ItemID.SharkToothNecklace)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }

    public class FullMoonAmuletPlayer : ModPlayer
    {
        // 冷却时间常量 (60秒 = 3600 ticks)
        private const int COOLDOWN_TIME = 2400;
        
        public bool hasFullMoonAmulet = false;
        
        // 三个血条段的冷却时间 (单位为tick，60 tick = 1秒)
        public int[] healthSegmentCooldown = new int[3];
        
        // 每个血条段的持续伤害减免计数器
        public int[] segmentDamageTaken = new int[3];
        
        public override void ResetEffects()
        {
            hasFullMoonAmulet = false;
        }
        
        // ... existing code ...
        // ... existing code ...
        public override void PreUpdate()
        {
            if (hasFullMoonAmulet)
            {
                // 更新冷却时间，按照严格的顺序：2->1->0
                // 只有当高阶段冷却完毕后，低阶段才能开始冷却
                // 每次只有一个阶段在冷却
                
                // 按照优先级检查冷却
                for (int i = 2; i >= 0; i--)
                {
                    // 如果当前阶段需要冷却
                    if (healthSegmentCooldown[i] > 0)
                    {
                        // 冷却该阶段
                        healthSegmentCooldown[i]--;
                        // 一次只冷却一个阶段，所以break
                        break;
                    }
                }
            }
        }
// ... existing code ...
// ... existing code ...
        
        // ... existing code ...
        // ... existing code ...
        // ... existing code ...
        // ... existing code ...
        // ... existing code ...
        // ... existing code ...
// ... existing code ...
// ... existing code ...
        // PostHurt: 在伤害处理完成后调用
        // 可以根据实际受到的伤害来恢复血量
        // ... existing code ...
        // PostHurt: 在伤害处理完成后调用
        // 通过现在的血量和受到的伤害算出之前的血量，比较阶段差异来触发保护机制
        public override void PostHurt(Player.HurtInfo info)
        {
            if (!hasFullMoonAmulet) return;

            // 计算血量分段大小
            int segmentSize = Player.statLifeMax2 / 3;
            
            // 根据当前血量计算当前阶段
            int currentSegment = Player.statLife / segmentSize;
            
            // 获取实际受到的伤害值
            int damage = info.Damage;
            
            // 推算受伤前的血量和阶段
            int previousLife = Player.statLife + damage;
            int previousSegment = previousLife / segmentSize;
            
            // 确保阶段在有效范围内
            if (currentSegment >= 3) currentSegment = 2;
            if (currentSegment < 0) currentSegment = 0;
            if (previousSegment >= 3) previousSegment = 2;
            if (previousSegment < 0) previousSegment = 0;
            
            // Main.NewText($"[PostHurt] 受伤前阶段: {previousSegment}, 受伤后阶段: {currentSegment}, 受到伤害: {damage}", 255, 255, 255);

            // 检查受伤前的阶段是否在冷却中
            if (healthSegmentCooldown[previousSegment] > 0)
            {
                // 冷却中，不触发保护机制
                return;
            }

            // 如果受伤前和受伤后的阶段不一致，触发保护机制
            if (previousSegment != currentSegment)
            {
                // 触发受伤前阶段的冷却
                healthSegmentCooldown[previousSegment] = COOLDOWN_TIME;
                
                // 根据受伤前的阶段进行不同处理，将生命值恢复到该阶段的下限
                switch (previousSegment)
                {
                    case 0: // 第一阶段 (0 - statLifeMax2/3)
                        // 恢复生命值到第一阶段下限（即segmentSize）
                        Player.statLife = 1;
                        // Main.NewText("[PostHurt] 血量恢复至第1阶段下限", 255, 255, 255);
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item91, Player.position);
                        break;

                    case 1: // 第二阶段 (statLifeMax2/3 - 2*statLifeMax2/3)
                        // 恢复生命值到第二阶段下限（即segmentSize * 2）
                        Player.statLife = segmentSize * 1;
                        // Main.NewText("[PostHurt] 血量恢复至第2阶段下限", 255, 255, 255);
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item91, Player.position);
                        break;

                    case 2: // 第三阶段 (2*statLifeMax2/3 - statLifeMax2)
                        // 恢复生命值到第三阶段下限（即最大生命值）
                        Player.statLife = segmentSize * 2;
                        // Main.NewText("[PostHurt] 血量恢复至第3阶段下限", 255, 255, 255);
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item91, Player.position);
                        break;
                }
            }
        }
// ... existing code ...

        
        // ... existing code ...
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            // 如果未装备望月护符则使用默认行为
            if (!hasFullMoonAmulet)
                return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
            
            // 计算血量分段
            int segmentSize = Player.statLifeMax2 / 3;
            int currentSegment = Player.statLife / segmentSize;
            
            
            // 确保在有效范围内
            if (currentSegment >= 3) currentSegment = 2;
            if (currentSegment < 0) currentSegment = 0;
            // Main.NewText("当前血量段：" + currentSegment, 255, 255, 255);
            
            // 检查当前段是否在冷却中
            if (healthSegmentCooldown[currentSegment] > 0)
            {
                // 冷却中，使用默认死亡处理
                return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
            }
            else
            { 
            
            // 根据当前所处的血量阶段进行不同处理
            switch (currentSegment)
            {
                
                case 0: // 第一阶段 (0 - statLifeMax2/3)
                    // 设置冷却时间
                    healthSegmentCooldown[0] = COOLDOWN_TIME;
                    // 播放音效
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item91, Player.position);
                    // 将血量恢复到1点并阻止死亡
                    // Main.NewText("生命值已恢复至1点！", 0, 255, 255);
                    Player.statLife = 1;
                    return false; // 阻止死亡
                    
                case 1: // 第二阶段 (statLifeMax2/3 - 2*statLifeMax2/3)
                    // 设置冷却时间
                    healthSegmentCooldown[1] = COOLDOWN_TIME;
                    // 播放音效
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item91, Player.position);
                    // 将血量恢复到第一阶段上限并阻止死亡
                    Player.statLife = segmentSize;
                    // Main.NewText($"生命值已恢复至1/3,{segmentSize}",0, 255, 255);
                    return false; // 阻止死亡
                    
                case 2: // 第三阶段 (2*statLifeMax2/3 - statLifeMax2)
                    // 设置冷却时间
                    healthSegmentCooldown[2] = COOLDOWN_TIME;
                    // 播放音效
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item91, Player.position);
                    // Main.NewText($"生命值已恢复至2/3,{segmentSize*2}", 0, 255, 255);
                    // 将血量恢复到第二阶段上限并阻止死亡
                    Player.statLife = segmentSize * 2;
                    return false; // 阻止死亡
            }
            }
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
            
            // 默认情况使用基础
        }
// ... existing code ...
    }
}