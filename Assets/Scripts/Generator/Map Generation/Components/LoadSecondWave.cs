using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        yield return MapGenerator.Instance.StartCoroutine(Load(data, saveData));
    }
    #endregion

    private IEnumerator Load(MapGenerationData data, SaveDataCurrentProgress saveData) {
        //Load region references
        yield return MapGenerator.Instance.StartCoroutine(LoadRegionReferences(saveData));
        
        //Load structure references
        yield return MapGenerator.Instance.StartCoroutine(LoadStructureReferences(saveData));
        
        //Load Faction Related Extra Data
        yield return MapGenerator.Instance.StartCoroutine(LoadFactionReferences(saveData));

        //load tile second wave
        yield return MapGenerator.Instance.StartCoroutine(LoadLocationGridTileSecondWave(saveData));
        
        //Load Settlement data
        yield return MapGenerator.Instance.StartCoroutine(LoadSettlementReferences(saveData));
        // yield return MapGenerator.Instance.StartCoroutine(LoadSettlementMainStorageAndPrison(saveData));
        // yield return MapGenerator.Instance.StartCoroutine(LoadOtherSettlementData(saveData));
        
        //Load Characters

        //Load Tile Objects
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));
        
        
        //Load Structure Wall Traits
        yield return MapGenerator.Instance.StartCoroutine(LoadStructureWallTraits(saveData));
        
        //Load Area Spells Component
        yield return MapGenerator.Instance.StartCoroutine(LoadAreaSpellsComponent(saveData));

        //Always load action references before character references because there is a chance that the action is no longer viable to be done, so the action will be removed from the database
        //This means that when the character loads its references (including jobs and actions), if the action of the character is no longer in the database it will not be loaded to it and that is exactly what we want
        yield return MapGenerator.Instance.StartCoroutine(LoadActionReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadInterruptReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadAdditionalActionReferences(saveData));
        
        // yield return MapGenerator.Instance.StartCoroutine(LoadLogReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadPartyReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadPartyQuestsReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadCrimeReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadGatheringReferences(saveData));

        //Load Second Wave Job data
        yield return MapGenerator.Instance.StartCoroutine(LoadJobsSecondWave(saveData));

        //Always load characters last but before additional tile object info because of tombstone(it needs character marker before placing) so that if ever there are things like the character must be in the bed already, we know that the tile objects are already loaded
        yield return MapGenerator.Instance.StartCoroutine(LoadCharacterReferences(saveData));

        yield return MapGenerator.Instance.StartCoroutine(LoadAdditionalTileObjectInfo(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjectTraits(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjectAsDeadReference(saveData));

        //Load additional structure references
        yield return MapGenerator.Instance.StartCoroutine(LoadAdditionalStructureReferences(saveData));
        
        //Load Second wave trait data
        yield return MapGenerator.Instance.StartCoroutine(LoadTraitsSecondWave(saveData));
        
        yield return MapGenerator.Instance.StartCoroutine(LoadPlayerReferences(data, saveData));
    }

    #region Region
    private IEnumerator LoadRegionReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Region Data...");
        for (int i = 0; i < saveData.worldMapSave.regionSaves.Count; i++) {
            SaveDataRegion saveDataRegion = saveData.worldMapSave.regionSaves[i];
            Region region = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(saveDataRegion.persistentID);
            region.LoadReferences(saveDataRegion);
            yield return null;
        }
    }
    #endregion
    
    #region Structure
    private IEnumerator LoadStructureReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Structure Data...");
        for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++) {
            SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
            LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID);
            structure.LoadReferences(saveDataLocationStructure);
            yield return null;
        }
    }
    private IEnumerator LoadAdditionalStructureReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Structure Data...");
        for (int i = 0; i < saveData.worldMapSave.structureSaves.Count; i++) {
            SaveDataLocationStructure saveDataLocationStructure = saveData.worldMapSave.structureSaves[i];
            LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataLocationStructure.persistentID);
            structure.LoadAdditionalReferences(saveDataLocationStructure);
            yield return null;
        }
    }
    #endregion
    
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
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Objects...");
        int batchCount = 0;
        HashSet<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
        for (int i = 0; i < allTileObjects.Count; i++) {
            TileObject tileObject = allTileObjects.ElementAt(i);
            string persistentID = tileObject.persistentID;
            SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
            if (saveDataTileObject == null) {
                // Debug.LogWarning($"{tileObject} with persistentID {tileObject.persistentID} does not have any save data.");
                continue;
            }
            if (tileObject is ThinWall) {
                continue;
            }
            if (tileObject is GenericTileObject || !saveDataTileObject.tileLocationID.hasValue) {
                //the loaded object does not have a grid tile location, it will be loaded and in memory, but not placed in this section.
                //if it in a character's inventory then it will be referenced by the character carrying it, when that character has been loaded.
                //Also do not load generic tile objects, since they are loaded in RegionInnerMapGeneration.
                tileObject.LoadSecondWave(saveDataTileObject);
                continue;
            }
            if (tileObject is Tombstone) {
                tileObject.LoadSecondWave(saveDataTileObject);
                continue;
            }
            if (saveDataTileObject.tileLocationID.hasValue) {
                LocationGridTile gridTileLocation = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveDataTileObject.tileLocationID);
                if (tileObject is MovingTileObject) {
                    SaveDataMovingTileObject saveDataMovingTileObject = saveDataTileObject as SaveDataMovingTileObject;
                    if (!saveDataMovingTileObject.hasExpired) {
                        Assert.IsNotNull(saveDataMovingTileObject);
                        tileObject.SetGridTileLocation(gridTileLocation);
                        tileObject.OnPlacePOI();
                        tileObject.mapObjectVisual.SetWorldPosition(saveDataMovingTileObject.mapVisualWorldPosition);
                        tileObject.LoadSecondWave(saveDataTileObject);    
                    }
                } else {
                    gridTileLocation.structure.LoadPOI(tileObject, gridTileLocation);
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
                
                // if (tileObject is BlockWall && tileObject.gridTileLocation.structure is DemonicStructure demonicStructure) {
                //     //TODO: This is only a quick fix, so that loaded block walls will contribute to demonic structure damage
                //     demonicStructure.AddObjectAsDamageContributor(tileObject);
                // } else if (tileObject is Eyeball && tileObject.gridTileLocation.structure is Eye eye) {
                //     //TODO: This is only a quick fix, so that loaded eyes will contribute to demonic structure damage
                //     eye.AddObjectAsDamageContributor(tileObject);
                // } else if (tileObject is PortalTileObject && tileObject.gridTileLocation.structure is ThePortal portal) {
                //     //TODO: This is only a quick fix, so that loaded portal will contribute to demonic structure damage
                //     portal.AddObjectAsDamageContributor(tileObject);
                // }
            }
            batchCount++;
            if (batchCount == MapGenerationData.TileObjectLoadingBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    private IEnumerator LoadTileObjectTraits(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Object Traits...");
        int batchCount = 0;
        HashSet<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
        for (int i = 0; i < allTileObjects.Count; i++) {
            TileObject tileObject = allTileObjects.ElementAt(i);
            string persistentID = tileObject.persistentID;
            SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
            if (saveDataTileObject != null) {
                SaveDataTraitContainer saveDataTraitContainer = saveDataTileObject.saveDataTraitContainer;
                tileObject.traitContainer.Load(tileObject, saveDataTraitContainer);
                batchCount++;
                if (batchCount == MapGenerationData.TileObjectLoadingBatches) {
                    batchCount = 0;
                    yield return null;    
                }    
            }
        }
    }
    private IEnumerator LoadAdditionalTileObjectInfo(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Other Object info...");
        int batchCount = 0;
        HashSet<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
        for (int i = 0; i < allTileObjects.Count; i++) {
            TileObject tileObject = allTileObjects.ElementAt(i);
            string persistentID = tileObject.persistentID;
            SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, persistentID);
            if (tileObject is Tombstone tombstone) {
                if (saveDataTileObject.tileLocationID.hasValue) {
                    if (tombstone.character == null || tombstone.character.marker == null) {
                        Debug.LogWarning($"{tombstone} with persistent id {tombstone.persistentID} does not have a character inside it, but has a tile location. Not placing it to prevent errors, but this case should not happen!");
                        continue;
                    }
                    LocationGridTile gridTileLocation = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveDataTileObject.tileLocationID);
                    gridTileLocation.structure.LoadPOI(tileObject, gridTileLocation);
                    if (tileObject.mapObjectVisual != null) {
                        if (InnerMapManager.Instance.assetManager.allTileObjectSprites.ContainsKey(saveDataTileObject.spriteName)) {
                            tileObject.mapObjectVisual.SetVisual(InnerMapManager.Instance.assetManager.allTileObjectSprites[saveDataTileObject.spriteName]);
                        } else {
                            tileObject.mapObjectVisual.SetVisual(null);    
                            // Debug.Log($"Could not find asset with name {saveDataTileObject.spriteName}");
                        }
                        tileObject.mapObjectVisual.SetRotation(saveDataTileObject.rotation);    
                    }
                }
            }
            tileObject.LoadAdditionalInfo(saveDataTileObject);
            
            batchCount++;
            if (batchCount == MapGenerationData.TileObjectLoadingBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    private IEnumerator LoadTileObjectAsDeadReference(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Info...");
        HashSet<TileObject> allTileObjects = DatabaseManager.Instance.tileObjectDatabase.allTileObjectsList;
        for (int i = 0; i < allTileObjects.Count; i++) {
            TileObject tileObject = allTileObjects.ElementAt(i);
            if (tileObject.isDeadReference) {
                DatabaseManager.Instance.tileObjectDatabase.UnRegisterTileObject(tileObject, false);
                i--;
                yield return null;
            }
        }
    }
    #endregion

    #region Hex Tile
    private IEnumerator LoadAreaSpellsComponent(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Area Spells...");
        for (int i = 0; i < saveData.worldMapSave.areaSaves.Count; i++) {
            SaveDataArea saveArea = saveData.worldMapSave.areaSaves[i];
            Area area = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveArea.areaData.persistentID);
            area.spellsComponent.LoadReferences(saveArea.spellsComponent);
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
    private IEnumerator LoadAdditionalActionReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Action Data...");
        saveData.LoadAdditionalActionReferences();
        yield return null;
    }
    private IEnumerator LoadInterruptReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Action Data...");
        saveData.LoadInterruptReferences();
        yield return null;
    }
    // private IEnumerator LoadLogReferences(SaveDataCurrentProgress saveData) {
    //     LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Log Data...");
    //     saveData.LoadLogReferences();
    //     yield return null;
    // }
    private IEnumerator LoadPartyReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Party Data...");
        saveData.LoadPartyReferences();
        yield return null;
    }
    private IEnumerator LoadPartyQuestsReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Party Quest Data...");
        saveData.LoadPartyQuestReferences();
        yield return null;
    }
    private IEnumerator LoadCrimeReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Crime Data...");
        saveData.LoadCrimeReferences();
        yield return null;
    }
    private IEnumerator LoadGatheringReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Gathering Data...");
        saveData.LoadGatheringReferences();
        yield return null;
    }
    #endregion

    #region Settlements
    private IEnumerator LoadSettlementReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Settlement Data...");
        for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
            SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
            BaseSettlement baseSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID);
            baseSettlement.LoadReferences(saveDataBaseSettlement);
            
            // if (!string.IsNullOrEmpty(saveDataBaseSettlement.factionOwnerID)) {
            //     BaseSettlement baseSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID);
            //     // LandmarkManager.Instance.OwnSettlement(faction, baseSettlement);
            //     baseSettlement.LoadReferences(saveDataBaseSettlement);
            // }
        }
        yield return null;
    }
    // private IEnumerator LoadSettlementMainStorageAndPrison(SaveDataCurrentProgress saveData) {
    //     LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Additional Settlement Data...");
    //     for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
    //         SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
    //         BaseSettlement baseSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataBaseSettlement._persistentID);
    //         if (saveDataBaseSettlement is SaveDataNPCSettlement saveDataNpcSettlement && baseSettlement is NPCSettlement npcSettlement) {
    //             if (!string.IsNullOrEmpty(saveDataNpcSettlement.prisonID)) {
    //                 LocationStructure prison = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNpcSettlement.prisonID);
    //                 npcSettlement.LoadPrison(prison);
    //             }
    //             if (!string.IsNullOrEmpty(saveDataNpcSettlement.mainStorageID)) {
    //                 LocationStructure mainStorage = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNpcSettlement.mainStorageID);
    //                 npcSettlement.LoadMainStorage(mainStorage);
    //             }    
    //         }
    //     }
    //     yield return null;
    // }
    // private IEnumerator LoadOtherSettlementData(SaveDataCurrentProgress saveData) {
    //     LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Settlement Jobs...");
    //     for (int i = 0; i < saveData.worldMapSave.settlementSaves.Count; i++) {
    //         SaveDataBaseSettlement saveDataBaseSettlement = saveData.worldMapSave.settlementSaves[i];
    //         if (saveDataBaseSettlement is SaveDataNPCSettlement saveDataNpcSettlement) {
    //             NPCSettlement npcSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataNpcSettlement.persistentID) as NPCSettlement;
    //             Assert.IsNotNull(npcSettlement);
    //             npcSettlement.Initialize();
    //             npcSettlement.LoadJobs(saveDataNpcSettlement);
    //             npcSettlement.LoadRuler(saveDataNpcSettlement.rulerID);
    //             npcSettlement.LoadResidents(saveDataNpcSettlement);
    //             npcSettlement.LoadPartiesAndQuests(saveDataNpcSettlement);
    //         }
    //         yield return null;
    //     }
    // }
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
                        ThinWall structureWallObject = manMadeStructure.structureWalls[j];
                        SaveDataTileObject saveDataStructureWallObject = saveDataManMadeStructure.structureWallObjects[j];
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
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading More Trait data...");
        saveData.LoadTraitsSecondWave();
        yield return null;
    }
    #endregion

    #region Jobs
    private IEnumerator LoadJobsSecondWave(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading More Job data...");
        int batchCount = 0;
        for (int i = 0; i < DatabaseManager.Instance.jobDatabase.allJobs.Count; i++) {
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.allJobs[i];
            SaveDataJobQueueItem saveDataJobQueueItem = saveData.GetFromSaveHub<SaveDataJobQueueItem>(OBJECT_TYPE.Job, jobQueueItem.persistentID);
            if (saveDataJobQueueItem != null) {
                bool isViable = jobQueueItem.LoadSecondWave(saveDataJobQueueItem);
                //if (!isViable) {
                //    if (DatabaseManager.Instance.jobDatabase.UnRegister(jobQueueItem)) {
                //        i--;
                //    }
                //}
            }
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
            for (int j = 0; j < saveDataRegion.innerMapSave.tileSaves.Values.Count; j++) {
                SaveDataLocationGridTile saveDataLocationGridTile = saveDataRegion.innerMapSave.tileSaves.Values.ElementAt(j);
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

    #region Player
    private IEnumerator LoadPlayerReferences(MapGenerationData data, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Player Data...");
        saveData.LoadPlayerReferences();
        yield return null;
    }
    #endregion
}
