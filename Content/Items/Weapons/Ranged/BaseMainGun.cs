using System.Linq;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Ranged
{
    public class BaseMainGun : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Type] = true;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("基础主炮");
            Item.damage = ExpansionKele.ATKTool(17,default); // 基础伤害
            Item.DamageType = DamageClass.Ranged; // 远程伤害
            Item.width = 64; // 宽度
            Item.height = 32; // 高度
            Item.useTime = 25; // 使用时间
            Item.useAnimation = 25; // 使用动画时间
            Item.useStyle = ItemUseStyleID.Shoot; // 射击风格
            Item.noMelee = true; // 禁用近战攻击
            Item.knockBack = 4; // 击退值
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 价值
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);  // 稀有度
            Item.UseSound = SoundID.Item11; // 使用音效
            Item.autoReuse = true; // 自动重用
            Item.shoot = ModContent.ProjectileType<BaseMainGunProjectile>(); // 默认发射物（将在实际射击中替换）
            Item.shootSpeed = 10f; // 发射速度
            //Item.useAmmo = AmmoID.Rocket; // 弹药类型为火箭
        }
    //     public override void SetStaticDefaults()//允许右键重复使用
	// {
	// 	ItemID.Sets.ItemsThatAllowRepeatedRightClick[base.Item.type] = true;
	// }

        public override bool CanUseItem(Player player)
        {
            return true;
        }
        // public override bool AltFunctionUse(Player player)
        // {
        //     return true; // 允许右键使用
        // }

         public override Vector2? HoldoutOffset()
	{
		
		return new Vector2(-40f, -6f);//左上为负，建议以左为主
	}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 12); // 所需材料
            recipe.AddTile(TileID.Anvils); // 合成站
            recipe.Register();
        }
    }



    public class BaseMainGunProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16; // 宽度
            Projectile.height = 16; // 高度
            Projectile.friendly = true; // 友方子弹
            Projectile.penetrate = 1; // 穿透敌人数量
            Projectile.timeLeft = 600; // 子弹存在时间
            Projectile.damage =(int)(Projectile.damage*1f); // 固定伤害
            Projectile.knockBack = 4f; // 击退值
            Projectile.DamageType = DamageClass.Ranged; // 远程伤害
            Projectile.extraUpdates = 1; // 每帧更新两次
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
        }

        
        public override void AI()
        {
            // 添加火焰拖尾效果
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 0, default, 2f);

            // 可选：添加发光效果
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 0f) * 0.75f); // 红橙色光晕
        }

        

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= 1f;
            DoAreaOfEffect(target.Center);
        }
        
        private void DoAreaOfEffect(Vector2 center)
        {
            foreach (NPC npc in Main.npc.Where(n => n.active && !n.friendly && n.lifeMax > 5))
    {
        if (Vector2.Distance(npc.Center, center) <= 8*16)
        {
            int explosionDamage = (int)(Projectile.damage * 0.41f);
            npc.SimpleStrikeNPC(explosionDamage, 0,default,0,DamageClass.Ranged);
            npc.immune[Projectile.owner] = 10;
            // NPC.HitModifiers modifiers = new NPC.HitModifiers()
            // {


            //     //ArmorPenetration += 10f // 忽视 10 点防御
            // };

            // const int flameCount = 2;
            // for (int j = 0; j < flameCount; j++)
            // {
            //     Vector2 velocity = (Vector2.UnitX * (float)Main.rand.NextDouble() * 2 - Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(360));
            //     int fireProj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), center, velocity * 3f,
            //         ProjectileID.FireArrow, (int)(explosionDamage * 0.5f), 0f, Projectile.owner);
            //     Main.projectile[fireProj].timeLeft = 30; // 缩短时间，避免过多残留
            // }

            // // 添加火焰特效
            // for (int j = 0; j < 10; j++)
            // {
            //     Dust.NewDust(npc.position, npc.width, npc.height, DustID.Lava, Scale: 2f);
            // }
        }
    }
        }

    }
}