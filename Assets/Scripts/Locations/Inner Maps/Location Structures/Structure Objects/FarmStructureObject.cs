using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class FarmStructureObject : LocationStructureObject {

        protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
            LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
            base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
            if (newTileObject.tileObjectType == TILE_OBJECT_TYPE.CORN_CROP || newTileObject.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB_CROP || 
                newTileObject.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY_CROP || newTileObject.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE_CROP ||
                newTileObject.tileObjectType == TILE_OBJECT_TYPE.POTATO_CROP) {
                Farm farm = structure as Farm;
                Assert.IsNotNull(farm);
                farm.AddFarmTile(tile);
                // structure.AddObjectAsDamageContributor(newTileObject);
                // Sprite originalSprite = InnerMapManager.Instance.GetTileObjectScriptableObject(newTileObject.tileObjectType).defaultSprite;
                // if (originalSprite != null) { newTileObject.mapVisual.SetVisual(originalSprite); }
                newTileObject.mapVisual.UpdateTileObjectVisual(newTileObject);
            }
        }
        protected override TileObject InstantiatePreplacedObject(TILE_OBJECT_TYPE p_type, LocationGridTile p_tile) {
            if (p_type == TILE_OBJECT_TYPE.CORN_CROP || p_type == TILE_OBJECT_TYPE.HYPNO_HERB_CROP || 
                p_type == TILE_OBJECT_TYPE.ICEBERRY_CROP || p_type == TILE_OBJECT_TYPE.PINEAPPLE_CROP ||
                p_type == TILE_OBJECT_TYPE.POTATO_CROP) {
                //determine what crop type should be used based on tile location
                if (p_tile.specificBiomeTileType == Biome_Tile_Type.Grassland) {
                    p_type = TILE_OBJECT_TYPE.CORN_CROP;
                } else if (p_tile.specificBiomeTileType == Biome_Tile_Type.Jungle) {
                    p_type = TILE_OBJECT_TYPE.POTATO_CROP;
                } else if (p_tile.specificBiomeTileType == Biome_Tile_Type.Desert || 
                           p_tile.specificBiomeTileType == Biome_Tile_Type.Oasis) {
                    p_type = TILE_OBJECT_TYPE.PINEAPPLE_CROP;
                } else if (p_tile.specificBiomeTileType == Biome_Tile_Type.Snow || 
                           p_tile.specificBiomeTileType == Biome_Tile_Type.Taiga || 
                           p_tile.specificBiomeTileType == Biome_Tile_Type.Tundra) {
                    p_type = TILE_OBJECT_TYPE.ICEBERRY_CROP;
                }
            }
            return base.InstantiatePreplacedObject(p_type, p_tile);
        }
    }
}