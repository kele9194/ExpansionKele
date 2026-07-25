using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 炮手刻印芯片：右键使用，将手持武器刻印为炮手（狙击 · 炮手）
    /// </summary>
    public class ArtilleristChip : SniperChipBase
    {
        
        protected override SniperSubclass ChipSubclass => SniperSubclass.Artillerist;
        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.Bomb,1)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
    
    }