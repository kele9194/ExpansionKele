using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExpansionKele.Content.Items.Placeables
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
            // 基础属性
            Main.tileSolid[Type] = true;              // 可站立
            Main.tileBlockLight[Type] = true;         // 阻挡光线
            Main.tileLighted[Type] = true;            // 可发光
            DustType = DustID.Stone;                  // 破坏尘埃类型
            HitSound = SoundID.Tink;                  // 被击打音效
            MinPick = 110;                            // 最低镐力要求（精金镐以上）
            RegisterItemDrop(ModContent.ItemType<FullMoonOre>()); // 掉落物品
            AddMapEntry(new Color(200, 200, 200), Language.GetText("FullMoonOre"));    // 小地图颜色
            Main.tileNoFail[Type] = true;             // 不会被爆炸破坏
            TileID.Sets.Ore[Type] = true;             // 标记为矿石
            Main.tileMergeDirt[Type] = true;          // 与泥土融合贴图
            Main.tileSpelunker[Type] = true;          // 支持探险者手电筒高亮
            Main.tileOreFinderPriority[Type] = 200;   // 探矿器优先级
            Main.tileShine[Type] = 900;               // 挖掘闪光亮度
            Main.tileShine2[Type] = true;             // 启用闪光特效

            Main.tileFrameImportant[Type] = true;

             Main.tileNoAttach[Type] = true; 

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;

            

            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = default;
            TileObjectData.newTile.AnchorTop = default;
            TileObjectData.newTile.AnchorLeft = default;
            TileObjectData.newTile.AnchorRight = default;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;

            // 支持多种样式（例如9种）
            TileObjectData.newTile.StyleHorizontal = true; // 横向排列
            TileObjectData.newTile.StyleWrapLimit = 3;     // 每行最多3个样式
            TileObjectData.newTile.StyleMultiplier = 3;    // 总共3行 => 3x3=9种样式
            TileObjectData.newTile.RandomStyleRange = 9;   // 随机选择0~8样式

			TileObjectData.addTile(Type);

            //SetupMultiStyleSupport();
        }

        /// <summary>
        /// 启用多样式 Tile 支持（如多纹理、随机外观）
        /// </summary>
        private void SetupMultiStyleSupport()
        {
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidBottom, 1, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;

            // 支持多种样式（例如9种）
            TileObjectData.newTile.StyleHorizontal = true; // 横向排列
            TileObjectData.newTile.StyleWrapLimit = 3;     // 每行最多3个样式
            TileObjectData.newTile.StyleMultiplier = 3;    // 总共3行 => 3x3=9种样式
            TileObjectData.newTile.RandomStyleRange = 9;   // 随机选择0~8样式

            TileObjectData.newTile.AnchorBottom = default; // 表示不依赖任何底部支撑
            TileObjectData.newTile.AnchorTop = default;
            TileObjectData.newTile.AnchorLeft = default;
            TileObjectData.newTile.AnchorRight =default;   

            TileObjectData.addTile(Type); // 必须调用 addTile 才能生效
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
    }
}