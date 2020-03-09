using System.Collections.Generic;
using Pathfinding;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class AreaInnerTileMap : InnerTileMap {
        //Building spots
        public NPCSettlement npcSettlement { get; private set; }
        public TownMapSettings generatedTownMapSettings { get; private set; }
        public string usedTownCenterTemplateName { get; private set; }

        #region Map Generation
        public void Initialize(NPCSettlement npcSettlement) {
            this.npcSettlement = npcSettlement;

            //set tile map sorting orders
            TilemapRenderer ground = groundTilemap.gameObject.GetComponent<TilemapRenderer>();
            ground.sortingOrder = InnerMapManager.GroundTilemapSortingOrder;
            TilemapRenderer details = detailsTilemap.gameObject.GetComponent<TilemapRenderer>();
            details.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;

            TilemapRenderer northEdge = northEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            northEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            TilemapRenderer southEdge = southEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            southEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            TilemapRenderer westEdge = westEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            westEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
            TilemapRenderer eastEdge = eastEdgeTilemap.gameObject.GetComponent<TilemapRenderer>();
            eastEdge.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
        }
        public void LoadMap(SaveDataAreaInnerTileMap data) {
            // outsideTiles = new List<LocationGridTile>();
            // insideTiles = new List<LocationGridTile>();
            //
            // LoadBurningSources(data.burningSources);
            //
            // LoadGrid(data);
            // SplitMap(false);
            //Vector3Int startPoint = new Vector3Int(eastOutsideTiles, southOutsideTiles, 0);
            //DrawTownMap(data.generatedTownMapSettings, startPoint);
            //No need to Place Structures since structure of tile is loaded upon loading grid tile
            //AssignOuterAreas(insideTiles, outsideTiles); //no need for this because structure reference is already saved per location grid tile, and this only assigns the tile to either the wilderness or work npcSettlement structure
        }
        // private IEnumerator GenerateGrid(TownMapSettings settings) {
        //     Point determinedSize = GetWidthAndHeightForSettings(settings);
        //     yield return StartCoroutine(GenerateGrid(determinedSize.X, determinedSize.Y));
        // }
        // private void SplitMap(bool changeFloorAssets = true) {
        //     //assign outer and inner areas
        //     //outer areas should always be 7 tiles from the edge (except for the east side that is 14 tiles from the edge)
        //     //values are all +1 to accomodate walls that take 1 tile
        //     for (int x = 0; x < width; x++) {
        //         for (int y = 0; y < height; y++) {
        //             LocationGridTile currTile = map[x, y];
        //             //determine if tile is outer or inner
        //             if (x < _eastOutsideTiles || x >= width - _westOutsideTiles || y < _southOutsideTiles || y >= height - _northOutsideTiles) {
        //                 //outside
        //                 currTile.SetIsInside(false);
        //                 //GetOutsideFloorTileForArea(npcSettlement)
        //                 if (changeFloorAssets) {
        //                     groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTile(npcSettlement));
        //                 }
        //                 outsideTiles.Add(currTile);
        //             } else {
        //                 //inside
        //                 currTile.SetIsInside(true);
        //                 //GetOutsideFloorTileForArea(npcSettlement)
        //                 if (changeFloorAssets) {
        //                     groundTilemap.SetTile(currTile.localPlace, GetOutsideFloorTile(npcSettlement));
        //                 }
        //                 insideTiles.Add(currTile);
        //             }
        //         }
        //     }
        // }
        #endregion
    }

    [System.Serializable]
    public class SaveDataAreaInnerTileMap {
        public int width;
        public int height;
        public int areaID;
        public SaveDataLocationGridTile[][] map;
        public string usedTownCenterTemplateName;
        public TownMapSettings generatedTownMapSettings;
        public List<SaveDataBurningSource> burningSources;

        public void Save(AreaInnerTileMap innerMap) {
            width = innerMap.width;
            height = innerMap.height;
            areaID = innerMap.npcSettlement.id;
            usedTownCenterTemplateName = innerMap.usedTownCenterTemplateName;
            generatedTownMapSettings = innerMap.generatedTownMapSettings;

            burningSources = new List<SaveDataBurningSource>();
            for (int i = 0; i < innerMap.activeBurningSources.Count; i++) {
                BurningSource bs = innerMap.activeBurningSources[i];
                SaveDataBurningSource source = new SaveDataBurningSource();
                source.Save(bs);
                burningSources.Add(source);
            }

            map = new SaveDataLocationGridTile[width][];
            for (int x = 0; x < innerMap.map.GetLength(0); x++) {
                map[x] = new SaveDataLocationGridTile[innerMap.map.GetLength(1)];
                for (int y = 0; y < innerMap.map.GetLength(1); y++) {
                    SaveDataLocationGridTile data = new SaveDataLocationGridTile();
                    data.Save(innerMap.map[x, y]);
                    map[x][y] = data;
                }
            }
        }
        public void Load(AreaInnerTileMap innerMap) {
            // innerMap.width = width;
            // innerMap.height = height;
            // innerMap.Initialize(LandmarkManager.Instance.GetAreaByID(areaID));
            // innerMap.LoadMap(this);
        }

        public void LoadTileTraits() {
            for (int x = 0; x < map.Length; x++) {
                for (int y = 0; y < map[x].Length; y++) {
                    map[x][y].LoadTraits();
                }
            }
        }
        public void LoadObjectHereOfTiles() {
            for (int x = 0; x < map.Length; x++) {
                for (int y = 0; y < map[x].Length; y++) {
                    map[x][y].LoadObjectHere();
                }
            }
        }
    }
}