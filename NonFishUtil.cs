using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Tyfyter.Utils {
    
    public sealed class NonFishItem : ModItem {
        public override string Texture => "Terraria/Item_2290";
        public static event Action ResizeItemArrays;
        public static event Action ResizeOtherArrays;
        public override bool IsQuestFish() {
            ResizeItemArrays();
            ResizeItemArrays = null;
            return false;
        }
        public override void SetStaticDefaults() {
            ResizeOtherArrays();
            ResizeOtherArrays = null;
        }
    }
}
