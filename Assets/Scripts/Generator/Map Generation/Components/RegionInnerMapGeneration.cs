using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class RegionInnerMapGeneration : MapGenerationComponent {
    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Generating inner maps...");
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionMap(region, this));
        }
    }

    #region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
        yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
    }
    #endregion

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading inner maps...");
        Dictionary<string, TileBase> tileAssetDB = InnerMapManager.Instance.assetManager.GetFloorAndWallTileAssetDB();
        for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
            SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
            Region location = GridMap.Instance.GetRegionByID(saveDataRegion.id);
            saveDataRegion.InitialLoadStructures(location);
            yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.LoadRegionMap(location, this, saveDataRegion.innerMapSave, tileAssetDB));
            yield return MapGenerator.Instance.StartCoroutine(AssignTilesToStructures(saveDataRegion.structureSaveData, location));
            yield return MapGenerator.Instance.StartCoroutine(LoadStructureObjects(saveDataRegion.structureSaveData, location));
            yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveDataRegion, location));
        }
    }
    private IEnumerator AssignTilesToStructures(SaveDataLocationStructure[] structureSaves, Region region) {
        LevelLoaderManager.Instance.UpdateLoadingInfo($"Loading {region.name} structures...");
        for (int i = 0; i < structureSaves.Length; i++) {
            SaveDataLocationStructure saveDataLocationStructure = structureSaves[i];
            if (saveDataLocationStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                //skip assigning to region since all tiles are assigned to wilderness initially
                continue;
            }
            LocationStructure structure = region.GetStructureByID(saveDataLocationStructure.structureType, saveDataLocationStructure.id);
            for (int j = 0; j < saveDataLocationStructure.tileCoordinates.Length; j++) {
                Point point = saveDataLocationStructure.tileCoordinates[j];
                LocationGridTile tile = region.innerMap.map[point.X, point.Y];
                tile.SetStructure(structure);
            }
            yield return null;
        }
    }
    private IEnumerator LoadStructureObjects(SaveDataLocationStructure[] structureSaves, Region region) {
        for (int i = 0; i < structureSaves.Length; i++) {
            SaveDataLocationStructure saveDataLocationStructure = structureSaves[i];
            LocationStructure structure = region.GetStructureByID(saveDataLocationStructure.structureType, saveDataLocationStructure.id);

            if (structure is ManMadeStructure manMadeStructure && saveDataLocationStructure is SaveDataManMadeStructure saveDataManMadeStructure) {
                //Man Made Structure
                GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(saveDataManMadeStructure.structureTemplateName, saveDataManMadeStructure.structureObjectWorldPosition,
                    Quaternion.identity, region.innerMap.structureParent, true);
                LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();

                structureObject.RefreshAllTilemaps();
                List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(region.innerMap);
                structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                manMadeStructure.SetStructureObject(structureObject);

                if (saveDataLocationStructure.occupiedHexTileID != -1) {
                    HexTile occupiedHexTile =  GridMap.Instance.GetHexTile(saveDataLocationStructure.occupiedHexTileID);
                    structure.SetOccupiedHexTile(occupiedHexTile.innerMapHexTile);
                }
                
                structureObject.OnLoadStructureObjectPlaced(region.innerMap, structure);
                structure.CreateRoomsBasedOnStructureObject(structureObject);
                structure.OnDoneLoadStructure();

                if (manMadeStructure.structureWalls != null) {
                    //load wall data
                    Assert.IsTrue(manMadeStructure.structureWalls.Count == saveDataManMadeStructure.structureWallObjects.Length, $"Structure walls of {structure} is inconsistent with save data!");
                    for (int j = 0; j < manMadeStructure.structureWalls.Count; j++) {
                        StructureWallObject structureWallObject = manMadeStructure.structureWalls[j];
                        SaveDataStructureWallObject saveDataStructureWallObject = saveDataManMadeStructure.structureWallObjects[j];
                        structureWallObject.LoadDataFromSave(saveDataStructureWallObject);
                    }    
                }
                yield return null;
            } else if (structure is DemonicStructure demonicStructure && saveDataLocationStructure is SaveDataDemonicStructure saveDataDemonicStructure) {
                //Demonic Structure
                GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(saveDataDemonicStructure.structureTemplateName, saveDataDemonicStructure.structureObjectWorldPosition,
                    Quaternion.identity, region.innerMap.structureParent, true);
                LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();

                structureObject.RefreshAllTilemaps();
                List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(region.innerMap);
                structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                demonicStructure.SetStructureObject(structureObject);

                if (saveDataLocationStructure.occupiedHexTileID != -1) {
                    HexTile occupiedHexTile = GridMap.Instance.GetHexTile(saveDataLocationStructure.occupiedHexTileID);
                    structure.SetOccupiedHexTile(occupiedHexTile.innerMapHexTile);
                }
                
                structureObject.OnLoadStructureObjectPlaced(region.innerMap, structure);
                structure.CreateRoomsBasedOnStructureObject(structureObject);
                structure.OnDoneLoadStructure();

                yield return null;
            }
        }
    }
    private IEnumerator LoadTileObjects(SaveDataRegion saveDataRegion, Region region) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading objects...");
        int batchCount = 0;
        for (int i = 0; i < saveDataRegion.tileObjectSaves.Count; i++) {
            SaveDataTileObject saveDataTileObject = saveDataRegion.tileObjectSaves[i];
            TileObject tileObject = saveDataTileObject.Load();
            LocationGridTile gridTileLocation = region.innerMap.map[saveDataTileObject.tileLocation.X, saveDataTileObject.tileLocation.Y];
            gridTileLocation.structure.AddPOI(tileObject, gridTileLocation);
            if (InnerMapManager.Instance.assetManager.allTileObjectSprites.ContainsKey(saveDataTileObject.spriteName)) {
                tileObject.mapObjectVisual.SetVisual(InnerMapManager.Instance.assetManager.allTileObjectSprites[saveDataTileObject.spriteName]);    
            } else {
                tileObject.mapObjectVisual.SetVisual(null);
                // Debug.Log($"Could not find asset with name {saveDataTileObject.spriteName}");
            }
            tileObject.mapObjectVisual.SetRotation(saveDataTileObject.rotation);
            batchCount++;
            if (batchCount == MapGenerationData.TileObjectLoadingBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    #endregion
}