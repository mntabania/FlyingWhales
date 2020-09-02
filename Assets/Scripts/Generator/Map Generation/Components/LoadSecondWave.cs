using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.Assertions;

public class LoadSecondWave : MapGenerationComponent {

    public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
        yield return null;
    }

    #region Saved World
    public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
        yield return MapGenerator.Instance.StartCoroutine(Load(saveData));
    }
    #endregion

    private IEnumerator Load(SaveDataCurrentProgress saveData) {
        //Load Faction Related Extra Data
        yield return MapGenerator.Instance.StartCoroutine(LoadFactionReferences(saveData));

        //load tile second wave
        yield return MapGenerator.Instance.StartCoroutine(LoadLocationGridTileSecondWave(saveData));
        
        //Load Settlement data
        yield return MapGenerator.Instance.StartCoroutine(LoadSettlementOwners(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadSettlementMainStorageAndPrison(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadSettlementJobs(saveData));
        
        //Load Characters

        //Load Tile Objects
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjectTraits(saveData));
        
        //Load Structure Wall Traits
        yield return MapGenerator.Instance.StartCoroutine(LoadStructureWallTraits(saveData));
        
        //Load Hex tile Spells Component
        yield return MapGenerator.Instance.StartCoroutine(LoadHexTileSpellsComponent(saveData));

        yield return MapGenerator.Instance.StartCoroutine(LoadActionReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadInterruptReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadLogReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadPartyReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadCrimeReferences(saveData));
        
        //Load Second wave trait data
        yield return MapGenerator.Instance.StartCoroutine(LoadTraitsSecondWave(saveData));
        
        //Load Second Wave Job data
        yield return MapGenerator.Instance.StartCoroutine(LoadJobsSecondWave(saveData));

        yield return MapGenerator.Instance.StartCoroutine(LoadCharacterReferences(saveData));
    }

    #region Faction
    private IEnumerator LoadFactionReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Faction Data...");
        saveData.LoadFactionReferences();
        yield return null;
    }
    #endregion

    #region Character
    private IEnumerator LoadCharacterReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Character Data...");
        saveData.LoadCharacterReferences();
        yield return null;
    }
    #endregion

    #region Tile Objects
    private IEnumerator LoadTileObjects(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading objects...");
        int batchCount = 0;
        for (int i = 0; i < DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList[i];
            string persistentID = tileObject.persistentID;
            SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
            if (tileObject is GenericTileObject || string.IsNullOrEmpty(saveDataTileObject.tileLocationID)) {
                //the loaded object does not have a grid tile location, it will be loaded and in memory, but not placed in this section.
                //if it in a character's inventory then it will be referenced by the character carrying it, when that character has been loaded.
                //Also do not load generic tile objects, since they are loaded in RegionInnerMapGeneration.
                tileObject.LoadSecondWave(saveDataTileObject);
                continue;
            }
            LocationGridTile gridTileLocation = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(saveDataTileObject.tileLocationID);
            if (tileObject is MovingTileObject) {
                SaveDataMovingTileObject saveDataMovingTileObject = saveDataTileObject as SaveDataMovingTileObject;
                Assert.IsNotNull(saveDataMovingTileObject);
                tileObject.SetGridTileLocation(gridTileLocation);
                tileObject.OnPlacePOI();
                tileObject.mapObjectVisual.SetWorldPosition(saveDataMovingTileObject.mapVisualWorldPosition);
                tileObject.LoadSecondWave(saveDataTileObject);
            } else if (tileObject is Tombstone) {
                //TODO:
            } else {
                gridTileLocation.structure.AddPOI(tileObject, gridTileLocation);
                if (tileObject.mapObjectVisual != null) {
                    if (InnerMapManager.Instance.assetManager.allTileObjectSprites.ContainsKey(saveDataTileObject.spriteName)) {
                        tileObject.mapObjectVisual.SetVisual(InnerMapManager.Instance.assetManager.allTileObjectSprites[saveDataTileObject.spriteName]);
                        if (tileObject is Table) {
                            tileObject.RevalidateTileObjectSlots();
                        }
                    } else {
                        tileObject.mapObjectVisual.SetVisual(null);    
                        // Debug.Log($"Could not find asset with name {saveDataTileObject.spriteName}");
                    }
                    tileObject.mapObjectVisual.SetRotation(saveDataTileObject.rotation);    
                }    
                tileObject.LoadSecondWave(saveDataTileObject);
            }
            batchCount++;
            if (batchCount == MapGenerationData.TileObjectLoadingBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    private IEnumerator LoadTileObjectTraits(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading object traits...");
        int batchCount = 0;
        for (int i = 0; i < DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList[i];
            string persistentID = tileObject.persistentID;
            SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
            SaveDataTraitContainer saveDataTraitContainer = saveDataTileObject.saveDataTraitContainer;
            tileObject.traitContainer.Load(tileObject, saveDataTraitContainer);
            batchCount++;
            if (batchCount == MapGenerationData.TileObjectLoadingBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    #endregion

    #region Hex Tile
    private IEnumerator LoadHexTileSpellsComponent(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Area Spells...");
        for (int i = 0; i < saveData.worldMapSave.hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = saveData.worldMapSave.hextileSaves[i];
            HexTile hexTile = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(saveDataHextile.persistentID);
            hexTile.spellsComponent.Load(saveDataHextile.saveDataHexTileSpellsComponent);
        }
        yield return null;
    }
    #endregion

    #region Others
    private IEnumerator LoadActionReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Action Data...");
        saveData.LoadActionReferences();
        yield return null;
    }
    private IEnumerator LoadInterruptReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Action Data...");
        saveData.LoadInterruptReferences();
        yield return null;
    }
    private IEnumerator LoadLogReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Log Data...");
        saveData.LoadLogReferences();
        yield return null;
    }
    private IEnumerator LoadPartyReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Party Data...");
        saveData.LoadPartyReferences();
        yield return null;
    }
    private IEnumerator LoadCrimeReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Crime Data...");
        saveData.LoadCrimeReferences();
        yield return null;
    }
    #endregion

    #region Settlements
        private IEnumerator LoadSettlementOwners(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Settlement Data...");
        for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
            SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
            if (!string.IsNullOrEmpty(saveDataBaseSettlement.factionOwnerID)) {
                BaseSettlement baseSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID);
                Faction faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(saveDataBaseSettlement.factionOwnerID);
                LandmarkManager.Instance.OwnSettlement(faction, baseSettlement);    
            }
        }
        yield return null;
    }
    private IEnumerator LoadSettlementMainStorageAndPrison(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Settlement Data...");
        for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
            SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
            BaseSettlement baseSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID);
            if (saveDataBaseSettlement is SaveDataNPCSettlement saveDataNpcSettlement && baseSettlement is NPCSettlement npcSettlement) {
                if (!string.IsNullOrEmpty(saveDataNpcSettlement.prisonID)) {
                    LocationStructure prison = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNpcSettlement.prisonID);
                    npcSettlement.LoadPrison(prison);
                }
                if (!string.IsNullOrEmpty(saveDataNpcSettlement.mainStorageID)) {
                    LocationStructure mainStorage = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNpcSettlement.mainStorageID);
                    npcSettlement.LoadMainStorage(mainStorage);
                }    
            }
        }
        yield return null;
    }
    private IEnumerator LoadSettlementJobs(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Settlement Jobs...");
        for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
            SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
            if (saveDataBaseSettlement is SaveDataNPCSettlement saveDataNpcSettlement) {
                NPCSettlement npcSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataNpcSettlement.persistentID) as NPCSettlement;
                Assert.IsNotNull(npcSettlement);
                npcSettlement.LoadJobs(saveDataNpcSettlement.jobIDs);
            }
            yield return null;
        }
    }
    #endregion

    #region Structure Walls
    private IEnumerator LoadStructureWallTraits(SaveDataCurrentProgress saveData) {
        for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++) {
            SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
            if (saveDataLocationStructure is SaveDataManMadeStructure saveDataManMadeStructure) {
                LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID);
                ManMadeStructure manMadeStructure = structure as ManMadeStructure;
                Assert.IsNotNull(manMadeStructure);
                if (manMadeStructure.structureWalls != null) {
                    for (int j = 0; j < manMadeStructure.structureWalls.Count; j++) {
                        StructureWallObject structureWallObject = manMadeStructure.structureWalls[j];
                        SaveDataStructureWallObject saveDataStructureWallObject = saveDataManMadeStructure.structureWallObjects[j];
                        structureWallObject.traitContainer.Load(structureWallObject, saveDataStructureWallObject.saveDataTraitContainer);
                    }    
                }
                yield return null;
            }
        }
    }
    #endregion

    #region Traits
    private IEnumerator LoadTraitsSecondWave(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading more trait data...");
        saveData.LoadTraitsSecondWave();
        yield return null;
    }
    #endregion

    #region Jobs
    private IEnumerator LoadJobsSecondWave(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading more job data...");
        int batchCount = 0;
        for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++) {
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
            SaveDataJobQueueItem saveDataJobQueueItem = saveData.GetFromSaveHub<SaveDataJobQueueItem>(OBJECT_TYPE.Job, jobQueueItem.persistentID);
            jobQueueItem.LoadSecondWave(saveDataJobQueueItem);
            batchCount++;
            if (batchCount == MapGenerationData.JobLoadingBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    #endregion

    #region Location Grid Tile
    private IEnumerator LoadLocationGridTileSecondWave(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Map Data...");
        int batchCount = 0;
        for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
            SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
            for (int j = 0; j < saveDataRegion.innerMapSave.tileSaves.Count; j++) {
                SaveDataLocationGridTile saveDataLocationGridTile = saveDataRegion.innerMapSave.tileSaves[j];
                LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(saveDataLocationGridTile.persistentID);
                tile.LoadSecondWave(saveDataLocationGridTile);
                batchCount++;
                if (batchCount == MapGenerationData.LocationGridTileSecondaryWaveBatches) {
                    batchCount = 0;
                    yield return null;    
                }
            }
        }
        yield return null;
    }
    #endregion
}
