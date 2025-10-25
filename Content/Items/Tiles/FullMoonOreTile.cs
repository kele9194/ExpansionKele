using ExpansionKele.Content.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExpansionKele.Content.Items.Tiles
{
    /// <summary>
    /// 满月矿石方块类。
    /// 表示游戏中一种可挖掘的稀有矿石，具有基础物理属性与掉落逻辑。
    /// </summary>
    public class FullMoonOreTile : ModTile
    {
        public override string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true;
            TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
            Main.tileSpelunker[Type] = true;          // 支持探险者手电筒高亮
            Main.tileOreFinderPriority[Type] = 410;   // 探矿器优先级
            Main.tileShine2[Type] = true;             // 启用闪光特效
            Main.tileShine[Type] = 900;               // 挖掘闪光亮度
            Main.tileMergeDirt[Type] = true;          // 与泥土融合贴图
            Main.tileSolid[Type] = true;              // 可站立
            Main.tileBlockLight[Type] = true;         // 阻挡光线
            Main.tileLighted[Type] = true;            // 可发光

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(200, 200, 200), name);    // 小地图颜色

            DustType = DustID.Stone;                  // 破坏尘埃类型
            HitSound = SoundID.Tink;                  // 被击打音效
            MinPick = 110;                            // 最低镐力要求（精金镐以上）
            RegisterItemDrop(ModContent.ItemType<FullMoonOre>()); // 掉落物品

            Main.tileNoFail[Type] = true;             // 不会被爆炸破坏

            // 简化TileObjectData设置，因为这是一个固体方块不需要复杂设置
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
        }

        /// <summary>
        /// 控制矿石是否可被爆炸破坏。
        /// 当前设置为不可被炸毁。
        /// </summary>
        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        /// <summary>
        /// 设置该方块发出的光照颜色。
        /// </summary>
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.9f;
            g = 0.9f;
            b = 1f;
        }

        /// <summary>
        /// 控制挖掘失败或成功时产生的尘埃数量。
        /// </summary>
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            if (fail)
                num = 1; // 镐等级不足时只产生少量尘埃
            else
                num = 3; // 成功挖掘时产生更多尘埃
        }

        // 启用Biome Sight药水效果来高亮显示此方块
        public override bool IsTileBiomeSightable(int i, int j, ref Color sightColor) {
            sightColor = Color.Cyan;
            return true;
        }
    }
}