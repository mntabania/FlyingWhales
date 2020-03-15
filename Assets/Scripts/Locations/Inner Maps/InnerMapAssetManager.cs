using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class InnerMapAssetManager : MonoBehaviour {

        [Header("Grassland Tiles")]
        public TileBase outsideTile;
        public TileBase grassTile;
        public TileBase soilTile;
        public TileBase stoneTile;
        public TileBase shrubTile;
        public TileBase herbPlantTile;
        public TileBase flowerTile;
        public TileBase rockTile;
        public TileBase randomGarbTile;

        [Header("Snow Tiles")]
        public TileBase snowOutsideTile;
        public TileBase snowTile;
        public TileBase tundraTile;
        public TileBase snowDirt;
        public TileBase snowFlowerTile;
        public TileBase snowGarbTile;
        
        [Header("Desert Tiles")]
        public TileBase desertOutsideTile;
        public TileBase desertGrassTile;
        public TileBase desertSandTile;
        public TileBase desertStoneGroundTile;
        public TileBase desertFlowerTile;
        public TileBase desertGarbTile;
        public TileBase desertRockTile;
        
        [Header("Inside Detail Tiles")]
        public TileBase crateBarrelTile;

        [Header("Seamless Edges")]
        public SeamlessEdgeAssetsDictionary edgeAssets; //0-north, 1-south, 2-west, 3-east

        [Header("Water Tiles")] 
        public TileBase waterTle;
        public TileBase shoreTile;

        [Header("Cave Tiles")] 
        public TileBase caveWallTile;
        public TileBase caveGroundTile;
        
        [Header("Monster Lair Tiles")]
        public TileBase monsterLairWallTile;
        public TileBase monsterLairGroundTile;
        
        [Header("Corrupted Tiles")] 
        public TileBase corruptedTile;
        public TileBase corruptedDetailTile;
        public Sprite[] corruptedTreeAssets;
        public Sprite[] corruptedBigTreeAssets;

        [Header("Demon Tiles")] 
        public TileBase demonicWallTile;
        
        [Header("Structure Floor Tiles")] 
        public TileBase woodFloorTile;
        public TileBase stoneFloorTile;

        [Header("Other Tiles")] 
        public TileBase poisonRuleTile;
        
        [Header("Tile Objects")]
        public TileObjectAssetDictionary tileObjectTiles;
        public TileObjectAssetDictionary corruptedTileObjectAssets;

        public TileBase GetOutsideFloorTile(Region location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return snowOutsideTile;
                case BIOMES.DESERT:
                    return desertOutsideTile;
                default:
                    return outsideTile;
            }
        }
        public TileBase GetWallAssetBasedOnWallType(WALL_TYPE wallType) {
            switch (wallType) {
                case WALL_TYPE.Stone:
                    return caveWallTile;
                case WALL_TYPE.Flesh:
                    return monsterLairWallTile;
                case WALL_TYPE.Demon_Stone:
                    return demonicWallTile;
                default:
                    return null;
            }
        }
        public TileBase GetFlowerTile(Region location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return snowFlowerTile;
                case BIOMES.DESERT:
                    return desertFlowerTile;
                default:
                    return flowerTile;
            }
        }
        public TileBase GetGarbTile(Region location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.SNOW:
                case BIOMES.TUNDRA:
                    return snowGarbTile;
                case BIOMES.DESERT:
                    return desertGarbTile;
                default:
                    return randomGarbTile;
            }
        }
        public TileBase GetRockTile(Region location) {
            switch (location.coreTile.biomeType) {
                case BIOMES.DESERT:
                    return desertRockTile;
                default:
                    return rockTile;
            }
        }
    }
}