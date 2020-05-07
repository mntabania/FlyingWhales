using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;
using Inner_Maps;
using Traits;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    private const string saveDataPlayerFileName = "SAVED_PLAYER_DATA";

    public SaveDataPlayer currentSaveDataPlayer { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

#if UNITY_EDITOR
            EditorApplication.quitting += OnEditorQuit;
#endif
        } else {
            Destroy(this.gameObject);
        }
    }
    private void OnApplicationQuit() {
        Save();
    }
    private void OnEditorQuit() {
        Save();
    }

    //public void SetCurrentSave(Save save) {
    //    currentSave = save;
    //}
    public void SetCurrentSaveDataPlayer(SaveDataPlayer save) {
        currentSaveDataPlayer = save;
    }
    public void SaveCurrentStateOfWorld() {
//        Save save = new Save((int)GridMap.Instance.width, (int)GridMap.Instance.height, GridMap.Instance._borderThickness);
//        save.SaveHextiles(GridMap.Instance.normalHexTiles);
//        // save.SaveOuterHextiles(GridMap.Instance.outerGridList);
//        save.SaveRegions(GridMap.Instance.allRegions);
//        // save.SavePlayerArea(PlayerManager.Instance.player.playerSettlement);
//        save.SaveNonPlayerAreas();
//        save.SaveFactions(FactionManager.Instance.allFactions);
//        save.SaveCharacters(CharacterManager.Instance.allCharacters);
//        save.SavePlayer(PlayerManager.Instance.player);
//        save.SaveTileObjects(InnerMapManager.Instance.allTileObjects);
//        // save.SaveSpecialObjects(TokenManager.Instance.specialObjects);
////        save.SaveAreaMaps(InnerMapManager.Instance.innerMaps); TODO: Saving for new generic inner map
//        save.SaveCurrentDate();
//        save.SaveNotifications();

//        SaveGame.Save<Save>(UtilityScripts.Utilities.gameSavePath + saveFileName, save);
    }
    public void Save() {
        //PlayerManager.Instance.player.SaveSummons();
        //PlayerManager.Instance.player.SaveTileObjects();
        SaveDataPlayer save = currentSaveDataPlayer;
        SaveGame.Save(UtilityScripts.Utilities.gameSavePath + saveDataPlayerFileName, save);
    }
    public void LoadSaveDataPlayer() {
        //if(UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + saveFileName)) {
        //    SetCurrentSave(SaveGame.Load<Save>(UtilityScripts.Utilities.gameSavePath + saveFileName));
        //}
        if (WorldConfigManager.Instance.isDemoWorld) {
            currentSaveDataPlayer = new SaveDataPlayer();
            currentSaveDataPlayer.InitializeInitialData();
        } else {
            if (UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + saveDataPlayerFileName)) {
                SetCurrentSaveDataPlayer(SaveGame.Load<SaveDataPlayer>(UtilityScripts.Utilities.gameSavePath + saveDataPlayerFileName));
            }
            if(currentSaveDataPlayer == null) {
                currentSaveDataPlayer = new SaveDataPlayer();
                currentSaveDataPlayer.InitializeInitialData();
            }    
        }
        
    }

    public static SaveDataTrait ConvertTraitToSaveDataTrait(Trait trait) {
        if (trait.isNotSavable) {
            return null;
        }
        SaveDataTrait saveDataTrait = null;
        System.Type type = System.Type.GetType($"SaveData{trait.name}");
        if (type != null) {
            saveDataTrait = System.Activator.CreateInstance(type) as SaveDataTrait;
        } else {
            saveDataTrait = new SaveDataTrait();
        }
        return saveDataTrait;
    }
}
