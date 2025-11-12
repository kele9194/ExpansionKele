using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class SolarRelic : ModItem
    {
        public static float damageMulti=0.9f;
        public static float damageConv=0.1f;
        public override string LocalizationCategory=>"Items.OtherItem";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("日魂圣物");
            // Tooltip.SetDefault("放入背包后激活：\n降低5%造成的伤害，但额外造成基于你10%伤害的无视防御伤害");
            Item.ResearchUnlockCount = 1; // 研究所需数量
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 5, 0, 0); // 5金币
            Item.rare = ItemRarityID.Yellow; // 黄色稀有度
        }

        public override void UpdateInventory(Player player)
        {
            // 当物品在背包中且被收藏时应用效果
            if (Item.favorited)
            {
                ApplySolarRelicEffect(player);
            }
        }


        private void ApplySolarRelicEffect(Player player)
        {
            // 获取玩家的乘算伤害修改器组件
            var damageMultiPlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            
            // 将伤害降低到原来的90%
            damageMultiPlayer.MultiplyMultiplicativeDamageBonus(damageMulti);
            
            // 添加特殊效果标记，用于在ModifyHitNPC中处理额外的真实伤害
            var solarRelicPlayer = player.GetModPlayer<SolarRelicPlayer>();
            solarRelicPlayer.SolarRelicActive = true;
        }

        public override void AddRecipes()
        {
            // 创建配方
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarTabletFragment, 5); // 5个日耀碎片
            recipe.AddIngredient(ItemID.BrokenHeroSword, 1); // 3个月亮碎片
            recipe.AddTile(TileID.MythrilAnvil); // 秘银砧/山铜砧
            recipe.Register();
        }
    }

public class SolarRelicPlayer : ModPlayer
    {
        public bool SolarRelicActive = false;
        public float OriginalDamage = 0f;
        public int LastHitSourceDamage = 0;

        public override void ResetEffects()
        {
            SolarRelicActive = false;
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            OriginalDamage = damage.ApplyTo(item.damage);
        }

        // 新增方法：捕获原始伤害信息
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 保存源伤害，这是处理前的原始伤害
            LastHitSourceDamage = (int)modifiers.SourceDamage.ApplyTo(OriginalDamage);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (SolarRelicActive)
            {
                // 使用HitInfo中的源伤害或者我们自己保存的源伤害来计算真实伤害，并整合暴击伤害倍率
                int sourceDamage = hit.SourceDamage > 0 ? hit.SourceDamage : LastHitSourceDamage;
                float critDamageMultiplier = hit.Crit ? 2.0f : 1.0f; 
                int trueDamage = (int)(sourceDamage * SolarRelic.damageConv * critDamageMultiplier+0.5f);

                if (trueDamage >= 0)
                {
                    // 对目标造成真实伤害（无视防御）
                    target.life -= trueDamage;
                    CombatText.NewText(target.getRect(), Color.White, trueDamage.ToString(), false, true);

                    // 触发击中效果
                    if (target.life > 0)
                    {
                        target.netUpdate = true;
                    }
                    // 当目标生命值降到0或以下时，处理死亡逻辑
                    else
                    {
                        // 使用原版击杀逻辑
                        target.StrikeNPC(new NPC.HitInfo() { Damage = trueDamage, HitDirection = 0, Knockback = 0f, Crit = false ,InstantKill=true}, false);
                    }
// ... existing code ...
                }
            }
        }
    }
}