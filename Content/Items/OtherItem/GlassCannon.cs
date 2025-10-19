// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using ExpansionKele.Content.Customs;

// namespace ExpansionKele.Content.Items
// {
//     public class GlassCannon : ModItem
//     {
//         public override void SetStaticDefaults()
//         {
//             //DisplayName.SetDefault("玻璃大炮");
//             //Tooltip.SetDefault("拥有邪神的力量，盔甲破碎了\n再次使用时失去邪神的关注");
//         }

//         public override void SetDefaults()
//         {
//             Item.width = 20;
//             Item.height = 20;
//             Item.maxStack = 1; // 不能堆叠
//             Item.value = Item.sellPrice(0, 1, 0, 0); // 价值
//             Item.rare = ItemRarityID.Red; // 稀有度
//             Item.useStyle = ItemUseStyleID.HoldUp; // 使用方式
//             Item.useTime = 30; // 使用时间
//             Item.useAnimation = 30; // 动画时间
//             Item.consumable = false; // 不消耗
//             Item.UseSound= SoundID.Item11;
//         }

//         public override void AddRecipes()
//         {
//             Recipe recipe = Recipe.Create(ModContent.ItemType<GlassCannon>());
//             recipe.AddIngredient(ItemID.IronBar, 15); // 15个铁
//             recipe.AddTile(TileID.Anvils); // 铁砧
//             recipe.Register();
//         }

//         public override bool? UseItem(Player player)
//         {
//             if (player.GetModPlayer<YourModPlayer>().hasGlassCannonEffect)
//             {
//                 player.GetModPlayer<YourModPlayer>().hasGlassCannonEffect = false;
//                 player.GetModPlayer<YourModPlayer>().ResetStats();
//                 Main.NewText(player.name + " 失去了邪神的关注", 255, 0, 0);
//             }
//             else
//             {
//                 player.GetModPlayer<YourModPlayer>().hasGlassCannonEffect = true;
//                 player.GetModPlayer<YourModPlayer>().ApplyStats();
//                 Main.NewText(player.name + " 拥有了邪神的力量，代价是盔甲破碎了", 255, 0, 0);
//             }
//             return true;
//         }
//     }

//     public class YourModPlayer : ModPlayer
//     {
//         float multiplier=0.82f;
//         float critBonus=0.18f;

//         float armorPenetration=15;
//         public bool hasGlassCannonEffect;

//         public override void ResetEffects()
//         {
//             hasGlassCannonEffect = false;
//             ResetStats();
//         }

//         public void ApplyStats()
//         {
//             for (int i = 10; i < 18; i++) // 10-17 是盔甲栏的索引
//             {
//                 Player.armor[i].SetDefaults(0);
//             }
//             Player.GetDamage<MeleeDamageClass>() += damageMultiplier.GetDamageMultiplier(multiplier,Player.GetTotalDamage<MeleeDamageClass>());
//             Player.GetDamage<MeleeDamageClass>() += damageMultiplier.GetDamageMultiplier(multiplier,Player.GetTotalDamage<RangedDamageClass>());
//             Player.GetDamage<MeleeDamageClass>() += damageMultiplier.GetDamageMultiplier(multiplier,Player.GetTotalDamage<MagicDamageClass>());
//             Player.GetDamage<MeleeDamageClass>() += damageMultiplier.GetDamageMultiplier(multiplier,Player.GetTotalDamage<SummonDamageClass>());
//             Player.GetDamage<MeleeDamageClass>() += damageMultiplier.GetDamageMultiplier(multiplier+critBonus,Player.GetTotalDamage<ThrowingDamageClass>());
//             Player.GetCritChance<GenericDamageClass>() +=critBonus;
//             Player.GetArmorPenetration<GenericDamageClass>() += armorPenetration;
            
//         }

//         public void ResetStats()
//         {
        
        
//         }
//     }
// }