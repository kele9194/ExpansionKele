using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles;
using Terraria.Localization;
using Terraria.GameContent.ItemDropRules;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    public class Resentment : ModItem
    {
        private int resentmentTimer = 0;
        public override string LocalizationCategory => "Items.Weapons";
        public static LocalizedText DeathReason { get; private set; }

        public override void SetStaticDefaults()
        {
            DeathReason = this.GetLocalization("DeathReason");
            // DisplayName.SetDefault("怨憎");
            // Tooltip.SetDefault("手持该武器每4帧损失2hp\n弹幕命中敌人恢复10hp\n血量每减少1%，自身防御减少1%，伤害增加0.2%");
        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(150,190);                        // 基础伤害
            Item.DamageType = DamageClass.Magic;     // 魔法伤害类型
            Item.mana = 10;                          // 消耗10魔力
            Item.width = 28;                         // 物品宽高
            Item.height = 30;
            Item.useTime = 12;                       // 使用时间12帧
            Item.useAnimation = 12;                  // 动画持续时间
            Item.useStyle = ItemUseStyleID.Shoot;    // 使用样式为射击
            Item.noMelee = true;                     // 关闭近战攻击判定
            Item.knockBack = 4;                      // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this);    // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);      // 稀有度
            Item.UseSound = SoundID.Item8;           // 魔法射击音效
            Item.autoReuse = true;                   // 自动重用
            Item.shoot = ModContent.ProjectileType<ResentmentProjectile>(); // 发射自定义弹幕
            Item.shootSpeed = 10f;                   // 弹丸速度
        }
        public override void HoldItem(Player player)
        {
            // 每2帧减少1点生命值（即每4帧减少2点生命值，但每次减少1点）
            // 根据用户要求改为每2帧损失1hp，这样等价于每4帧损失2hp
            resentmentTimer++;
            if (resentmentTimer >= 2) // 每2帧触发一次
            {
                player.statLife -= 1; // 减少1点生命值
                if (player.statLife <= 0)
                {
                    // 当玩家血量降到0或以下时，触发死亡，使用本地化的死亡原因
                    player.KillMe(PlayerDeathReason.ByCustomReason(DeathReason.Format(player.name)), 9999, 0);
                }
                resentmentTimer = 0; // 重置计时器
            }
            float lifeLostPercent = 1f - (float)player.statLife / player.statLifeMax2;

            var DefensePlayer =player.GetModPlayer<CustomDamageReductionPlayer>();
            DefensePlayer.MultiPreDefenseDamageReduction(1+lifeLostPercent/2f);
            // var damagePlayer=player.GetModPlayer<ExpansionKeleDamageMulti>();
            // damagePlayer.MultiplyMultiplicativeDamageBonus(1+lifeLostPercent*1.5f);
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            float lifeLostPercent = 1f - (float)player.statLife / player.statLifeMax2;
            
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damagePlayer.MultiplyMultiplicativeDamageBonus(1 + lifeLostPercent * 1.5f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 发射自定义弹幕
            int proj=Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ResentmentProjectile>(), damage, knockback, player.whoAmI);
            return false; // 阻止发射默认弹幕
        }

        public override void AddRecipes()
        {
            
        }
    }

    public class GolemDrop : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // 检查是否是石巨人 (Golem)
            if (npc.type == NPCID.Golem)
            {
                // 添加25%概率直接掉落怨憎武器
                // 明确指定掉落数量范围（例如 1 个）
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Resentment>(), 4, 1, 1));
            }
        }
    }
}