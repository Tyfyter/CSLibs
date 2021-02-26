using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Tyfyter.Utils.ID;
using static Tyfyter.Utils.StructureUtils.StructureTilePlacementType;

namespace Tyfyter.Utils {
    public static class StructureUtils {
        public class Structure {
            readonly string[] _map;
            readonly Dictionary<char, StructureTile> _key;
            public Queue<int> createdChests = new Queue<int>();
            public Structure(string[] map, Dictionary<char, StructureTile> key) {
                _map = map;
                _key = key;
            }
            public Structure(string[] map, params (char key, StructureTile value)[] key) {
                _map = map;
                _key = new Dictionary<char, StructureTile>(key.Length);
                foreach(var val in key)_key.Add(val.key, val.value);
            }
            /// <summary>
            /// places the structure
            /// </summary>
            /// <returns>the amount of blocks changed</returns>
            public int Place(int i, int j) {
                char[] line;
                StructureTile c;
                Tile currentTile;
                int i1,j1;
                Stack<(int x, int y, StructureTile tile)> changedTiles = new Stack<(int x, int y, StructureTile tile)>();
                Stack<(int x, int y, StructureTile tile)> changedMultiTiles = new Stack<(int x, int y, StructureTile tile)>();
                for(int j0 = 0; j0 < _map.Length; j0++) {
                    j1 = j + j0;
                    line = _map[j0].ToCharArray();
                    for(int i0 = 0; i0 < line.Length; i0++) {
                        i1 = i + i0;
                        c = _key[line[i0]];
                        if(c.placementType==Nothing)continue;
                        if(i1<0||i1>Main.maxTilesX||j1<0||j1>Main.maxTilesY) {
                            goto invalidPosition;
                        }
                        currentTile = Main.tile[i1, j1];
                        if(currentTile is null)goto invalidPosition;
                        switch(c.placementType & (OldHandling|Deactivate)) {
                            case RequiredTile:
                            if(currentTile.active())goto invalidPosition;
                            break;
                            case RequiredTile|Deactivate:
                            case ReplaceOld:
                            case ReplaceOld|Deactivate:
                            if(currentTile.active()&&(!TileID.Sets.CanBeClearedDuringGeneration[currentTile.type]||Main.tileContainer[currentTile.type]))goto invalidPosition;
                            break;
                            case OptionalTile:
                            if(currentTile.active())continue;
                            break;
                            case OptionalTile|Deactivate:
                            if(!currentTile.active()||!TileID.Sets.CanBeClearedDuringGeneration[currentTile.type]||Main.tileContainer[currentTile.type])continue;
                            break;
                        }
                        if((c.placementType&MultiTile) == 0) {
                            changedTiles.Push((i1,j1,c));
                        } else {
                            changedMultiTiles.Push((i1,j1,c));
                        }
                        continue;
                        invalidPosition:
                        if((c.placementType&OldHandling)!=OptionalTile){
                            return 0;
                        }
                    }
                }
                int changedTilesCount = 0;
                (int x, int y, StructureTile tile) currentChange;
                while(changedTiles.Count>0) {
                    currentChange = changedTiles.Pop();
                    currentTile = Main.tile[currentChange.x, currentChange.y];
                    c = currentChange.tile;
                    if((c.placementType & Deactivate) == 0) {
                        currentTile.active(true);
                        currentTile.ResetToType(c.type);
                        if(c.slopeType <= 4) currentTile.slope(c.slopeType);
                        if(c.slopeType == 5) currentTile.halfBrick();
                        _setLiquid(currentTile, c.placementType);
                    } else {
                        currentTile.active(false);
                        _setLiquid(currentTile, c.placementType);
                    }
					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendTileSquare(-1, currentChange.x, currentChange.y, 1);
                    } else {
						WorldGen.SquareTileFrame(currentChange.x, currentChange.y);
                    }
                    changedTilesCount++;
                }
                while(changedMultiTiles.Count>0) {
                    currentChange = changedMultiTiles.Pop();
                    currentTile = Main.tile[currentChange.x, currentChange.y];
                    c = currentChange.tile;
                    if(TileObject.CanPlace(currentChange.x, currentChange.y, c.type, c.style, 1, out TileObject tileObject)) {
                        TileObject.Place(tileObject);
                        if(Main.tileContainer[c.type])createdChests.Enqueue(Chest.CreateChest(currentChange.x, currentChange.y-1));
                    }
                }
                return changedTilesCount;
            }
            static void _setLiquid(Tile tile, StructureTilePlacementType liquid) {
                switch(liquid&(Honey|NoLiquid)) {
                    case Water:
                    tile.liquidType(Tile.Liquid_Water);
                    tile.liquid = 255;
                    break;
                    case Lava:
                    tile.liquidType(Tile.Liquid_Lava);
                    tile.liquid = 255;
                    break;
                    case Honey:
                    tile.liquidType(Tile.Liquid_Honey);
                    tile.liquid = 255;
                    break;
                    case NoLiquid:
                    tile.liquid = 0;
                    break;
                }
            }
        }
        [Flags]
        public enum StructureTilePlacementType {
            RequiredTile = 0,
            ReplaceOld = 1,
            OptionalTile = 2,
            OldHandling = 3,

            MultiTile = 4,
            Deactivate = 8,
            Water = 16,
            Lava = 16|64,
            Honey = 16|128,
            NoLiquid = 64,

            Nothing = 128
        }
        public struct StructureTile {
            public readonly ushort type;
            public readonly StructureTilePlacementType placementType;
            public readonly byte slopeType;
            public readonly int style;
            public StructureTile(ushort type) {
                this.type = type;
                placementType = RequiredTile;
                slopeType = SlopeID.None;
                style = 0;
            }
            public StructureTile(ushort type, StructureTilePlacementType placementType) {
                this.type = type;
                this.placementType = placementType;
                slopeType = SlopeID.None;
                style = 0;
            }
            /// <param name="slopeType">5 is half-brick</param>
            public StructureTile(ushort type, StructureTilePlacementType placementType, byte slopeType) {
                this.type = type;
                this.placementType = placementType;
                this.slopeType = slopeType;
                style = 0;
            }
            /// <param name="slopeType">5 is half-brick</param>
            public StructureTile(ushort type, StructureTilePlacementType placementType, byte slopeType, int style) {
                this.type = type;
                this.placementType = placementType;
                this.slopeType = slopeType;
                this.style = style;
            }
        }
	}
}