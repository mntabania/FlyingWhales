using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
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
        yield return MapGenerator.Instance.StartCoroutine(LoadPlayerReferences(saveData));
        //Load Faction Related Extra Data
        yield return MapGenerator.Instance.StartCoroutine(LoadFactionReferences(saveData));

        //Load Characters

        //Load Tile Objects
        yield return MapGenerator.Instance.StartCoroutine(LoadTileObjects(saveData));
        
        //Load Hex tile Spells Component
        yield return MapGenerator.Instance.StartCoroutine(LoadHexTileSpellsComponent(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadActionReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadInterruptReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadLogReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadPartyReferences(saveData));
        yield return MapGenerator.Instance.StartCoroutine(LoadCrimeReferences(saveData));
    }

    #region Player
    private IEnumerator LoadPlayerReferences(SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo("Loading Player Data...");
        saveData.LoadPlayerReferences();
        yield return null;
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
                continue;
            }
            LocationGridTile gridTileLocation = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(saveDataTileObject.tileLocationID);
            if (tileObject is MovingTileObject) {
                SaveDataMovingTileObject saveDataMovingTileObject = saveDataTileObject as SaveDataMovingTileObject;
                Assert.IsNotNull(saveDataMovingTileObject);
                tileObject.SetGridTileLocation(gridTileLocation);
                tileObject.OnPlacePOI();
                tileObject.mapObjectVisual.SetWorldPosition(saveDataMovingTileObject.mapVisualWorldPosition);
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
            }
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
}
