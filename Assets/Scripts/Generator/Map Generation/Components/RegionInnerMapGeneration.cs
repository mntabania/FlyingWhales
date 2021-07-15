using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class RegionInnerMapGeneration : MapGenerationComponent {
    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Map...");
        Region region = DatabaseManager.Instance.regionDatabase.mainRegion;
        yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionMap(region, this, data));
        //for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //    Region region = GridMap.Instance.allRegions[i];
        //    yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateRegionMap(region, this, data));
        //}
    }

    #region Scenario Maps
    public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Generating Map...");
        Region region = DatabaseManager.Instance.regionDatabase.mainRegion;
        yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateScenarioMap(region, this, data, scenarioMapData));
        //for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //    Region region = GridMap.Instance.allRegions[i];
        //    yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.GenerateScenarioMap(region, this, data, scenarioMapData));
        //}
    }
    #endregion

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Inner Maps...");
        //for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
        //    Region region = GridMap.Instance.allRegions[i];
        //    region.CreateStructureList();
        //}
        Region location = DatabaseManager.Instance.regionDatabase.mainRegion;
        location.CreateStructureList();

        //load all structure instances.
        for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++) {
            SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
            LocationStructure createdStructure = saveDataLocationStructure.InitialLoad(location);
            if (createdStructure != null && !createdStructure.hasBeenDestroyed) {
                if (createdStructure is Wilderness wilderness) {
                    location.LoadWilderness(wilderness);
                }
                //only add undestroyed structures to location.
                location.AddStructure(createdStructure);
                if (!string.IsNullOrEmpty(saveDataLocationStructure.settlementLocationID)) {
                    BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataLocationStructure.settlementLocationID);
                    settlement.AddStructure(createdStructure);
                }
            }
            saveDataLocationStructure.Load();
        }

        //create inner maps
        SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSave;
        yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.LoadRegionMap(location, this, saveDataRegion.innerMapSave, saveData));
        //for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
        //    SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
        //    Region location = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(saveDataRegion.persistentID);
        //    yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.LoadRegionMap(location, this, saveDataRegion.innerMapSave, saveData));
        //    // yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveDataRegion, location));
        //}

        LevelLoaderManager.Instance.UpdateLoadingInfo($"Loading Structures...");
        
        //place structures
        for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++) {
            SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
            if (saveDataLocationStructure.hasBeenDestroyed) {
                continue; //do not place already destroyed structures.
            }
            //Region region = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(saveDataLocationStructure.regionLocationID);
            yield return MapGenerator.Instance.StartCoroutine(AssignTilesToStructures(saveDataLocationStructure, location));
            yield return MapGenerator.Instance.StartCoroutine(LoadStructureObject(saveDataLocationStructure, location));    
        }

        Dictionary<string, TileBase> tileAssetDB = InnerMapManager.Instance.assetManager.GetFloorAndWallTileAssetDB();
        yield return MapGenerator.Instance.StartCoroutine(location.innerMap.LoadTileVisuals(this, saveDataRegion.innerMapSave, tileAssetDB));
        //for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
        //    SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
        //    Region region = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(saveDataRegion.persistentID);
        //    yield return MapGenerator.Instance.StartCoroutine(location.innerMap.LoadTileVisuals(this, saveDataRegion.innerMapSave, tileAssetDB));
        //}
        tileAssetDB.Clear();
        
        // //add wilderness to all village settlements
        // //NOTE: This is needed because some actions reference the settlement and check which structures of the settlement are valid targets (i.e Bury Character)
        // for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++) {
        //     NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
        //     if (settlement.locationType == LOCATION_TYPE.SETTLEMENT) {
        //         settlement.AddStructure(settlement.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));        
        //     }
        // }
        
        for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
            InnerTileMap innerTileMap = InnerMapManager.Instance.innerMaps[i];
            innerTileMap.groundTilemap.RefreshAllTiles();
            yield return null;
        }
        
    }
    private IEnumerator AssignTilesToStructures(SaveDataLocationStructure saveDataLocationStructure, Region region) {
        if (saveDataLocationStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID);
            for (int j = 0; j < saveDataLocationStructure.tileCoordinates.Length; j++) {
                Point point = saveDataLocationStructure.tileCoordinates[j];
                LocationGridTile tile = region.innerMap.map[point.X, point.Y];
                tile.SetStructure(structure);
            }
        }
        yield return null;
        
    }
    private IEnumerator LoadStructureObject(SaveDataLocationStructure saveDataLocationStructure, Region region) {
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

            if (!string.IsNullOrEmpty(saveDataLocationStructure.occupiedAreaID)) {
                Area occupiedArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataLocationStructure.occupiedAreaID);
                structure.SetOccupiedArea(occupiedArea);
            }
            
            structureObject.OnLoadStructureObjectPlaced(region.innerMap, structure, saveDataLocationStructure);
            structure.CreateRoomsBasedOnStructureObject(structureObject);
            structure.OnDoneLoadStructure();

            if (manMadeStructure.structureWalls != null) {
                //load wall data
                Assert.IsTrue(manMadeStructure.structureWalls.Count == saveDataManMadeStructure.structureWallObjects.Length, $"Structure walls of {structure} is inconsistent with save data!");
                for (int j = 0; j < manMadeStructure.structureWalls.Count; j++) {
                    ThinWall structureWallObject = manMadeStructure.structureWalls[j];
                    SaveDataTileObject saveDataStructureWallObject = saveDataManMadeStructure.structureWallObjects[j];
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

            if (!string.IsNullOrEmpty(saveDataLocationStructure.occupiedAreaID)) {
                Area occupiedArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataLocationStructure.occupiedAreaID);
                structure.SetOccupiedArea(occupiedArea);
            }
            
            structureObject.OnLoadStructureObjectPlaced(region.innerMap, structure, saveDataLocationStructure);
            structure.CreateRoomsBasedOnStructureObject(structureObject);
            structure.OnDoneLoadStructure();

            yield return null;
        } else if (structure is NaturalStructure naturalStructure && saveDataLocationStructure is SaveDataNaturalStructure saveDataNaturalStructure) {
            //natural structures
            if (!string.IsNullOrEmpty(saveDataLocationStructure.occupiedAreaID)) {
                Area occupiedArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataLocationStructure.occupiedAreaID);
                structure.SetOccupiedArea(occupiedArea);
            }
            if (naturalStructure is AnimalDen animalDen && saveDataNaturalStructure is SaveDataAnimalDen saveDataAnimalDen) {
                GameObject structurePrefab = ObjectPoolManager.Instance.InstantiateObjectFromPool(saveDataAnimalDen.structureTemplateName, saveDataAnimalDen.structureObjectWorldPosition,
                    Quaternion.identity, region.innerMap.structureParent, true);
                LocationStructureObject structureObject = structurePrefab.GetComponent<LocationStructureObject>();

                structureObject.RefreshAllTilemaps();
                List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(region.innerMap);
                structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                animalDen.SetStructureObject(structureObject);

                structureObject.OnLoadStructureObjectPlaced(region.innerMap, structure, saveDataLocationStructure);
                structure.CreateRoomsBasedOnStructureObject(structureObject);
                structure.OnDoneLoadStructure();
            }
            
        }
    }
    #endregion
}