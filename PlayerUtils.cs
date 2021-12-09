using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria.GameInput;

namespace Tyfyter.Utils {
	public static class PlayerUtils {
		public static bool HasAnyBuff(this Player player, HashSet<int> types) {
			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (player.buffTime[i] >= 1 && types.Contains(player.buffType[i])) {
					return true;
				}
			}
			return false;
		}
	}
}