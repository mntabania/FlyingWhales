using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Inner_Maps {
    public abstract class BaseIsland {
        
        public HashSet<LocationGridTile> tiles { get; }
        public List<LocationGridTile> borderTiles { get; }
        public List<Area> occupiedAreas { get; }
        public Color color;
        
        public BaseIsland() {
            tiles = new HashSet<LocationGridTile>();
            borderTiles = new List<LocationGridTile>();
            occupiedAreas = new List<Area>();
            color = Random.ColorHSV();
        }

        public virtual void AddTile(LocationGridTile tile, MapGenerationData mapGenerationData) {
            tiles.Add(tile);
            AddOccupiedArea(tile.area);
            if (ShouldTileBeABorderTile(tile)) {
                AddBorderTile(tile, mapGenerationData);
            }
        }
        public virtual void RemoveTile(LocationGridTile tile, MapGenerationData mapGenerationData) {
            tiles.Remove(tile);
            RemoveBorderTile(tile, mapGenerationData);
        }
        public void RemoveAllTiles() {
            tiles.Clear();
            borderTiles.Clear();
            occupiedAreas.Clear();
        }
	
        public void MergeWithIsland(BaseIsland otherIsland, MapGenerationData mapGenerationData) {
            for (int i = 0; i < otherIsland.tiles.Count; i++) {
                LocationGridTile tileInOtherIsland = otherIsland.tiles.ElementAt(i);
                AddTile(tileInOtherIsland, mapGenerationData);
            }
            otherIsland.RemoveAllTiles();
        }

        public bool IsAdjacentToIsland(BaseIsland otherIsland) {
            for (int i = 0; i < borderTiles.Count; i++) {
                LocationGridTile borderTile = borderTiles[i];
                for (int j = 0; j < borderTile.neighbourList.Count; j++) {
                    LocationGridTile neighbour = borderTile.neighbourList[j];
                    if (otherIsland.tiles.Contains(neighbour)) {
                        //this island has a tile that has a neighbour that is part of the given island.
                        return true;
                    }
                }
            }
            return false;
        }
        public T GetFirstAdjacentIsland<T>(List<T> p_choices) where T : BaseIsland {
            for (int i = 0; i < p_choices.Count; i++) {
                T island = p_choices[i];
                if (island != this && IsAdjacentToIsland(island)) {
                    return island;
                }
            }
            return null;
        }
        // public List<T> GetAdjacentIslands<T>(T[,] islandMap) where T : BaseIsland {
        //     for (int i = 0; i < borderTiles.Count; i++) {
        //         LocationGridTile borderTile = borderTiles[i];
        //         for (int j = 0; j < borderTile.neighbourList.Count; j++) {
        //             LocationGridTile neighbour = borderTile.neighbourList[j];
        //             T islandOfNeighbour = islandMap[neighbour.localPlace.x]
        //         }
        //     }
        // }

        #region Border Tiles
        private bool ShouldTileBeABorderTile(LocationGridTile p_tile) {
            for (int i = 0; i < p_tile.neighbourList.Count; i++) {
                LocationGridTile neighbour = p_tile.neighbourList[i];
                if (!tiles.Contains(neighbour)) {
                    //given tile has a neighbour that is not part of this island. Given tile should be a border tile.
                    return true;
                }
            }
            return false;
        }
        protected virtual void AddBorderTile(LocationGridTile p_tile, MapGenerationData mapGenerationData) {
            borderTiles.Add(p_tile);
            RevalidateBorderTilesNexTo(p_tile, mapGenerationData);
        }
        protected virtual bool RemoveBorderTile(LocationGridTile p_tile, MapGenerationData mapGenerationData) {
            return borderTiles.Remove(p_tile);
        }
        private void RevalidateBorderTilesNexTo(LocationGridTile p_tile, MapGenerationData mapGenerationData) {
            for (int i = 0; i < p_tile.neighbourList.Count; i++) {
                LocationGridTile neighbour = p_tile.neighbourList[i];
                if (borderTiles.Contains(neighbour)) {
                    if (!ShouldTileBeABorderTile(neighbour)) {
                        RemoveBorderTile(neighbour, mapGenerationData);
                    }
                }
            }
        }
        #endregion

        #region Occupied Areas
        public void AddOccupiedArea(Area p_area) {
            if (!occupiedAreas.Contains(p_area)) {
                occupiedAreas.Add(p_area);
            }
        }
        #endregion
    }
}