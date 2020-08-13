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
    private const string savedPlayerDataFileName = "SAVED_PLAYER_DATA";
    private const string savedCurrentProgressFileName = "SAVED_CURRENT_PROGRESS";

    public SaveDataPlayer currentSaveDataPlayer { get; private set; }

    [Header("For Testing")] 
    [SerializeField] private bool alwaysResetSpecialPopupsOnStartup;
    [SerializeField] private bool alwaysResetBonusTutorialsOnStartup;

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
        SavePlayerData();
    }
    private void OnEditorQuit() {
        SavePlayerData();
    }

    #region General
    public void SetCurrentSaveDataPlayer(SaveDataPlayer save) {
        currentSaveDataPlayer = save;
    }
    public static SaveDataTrait ConvertTraitToSaveDataTrait(Trait trait) {
        if (trait.isNotSavable) {
            return null;
        }
        SaveDataTrait saveDataTrait = null;
        System.Type type = System.Type.GetType($"SaveData{trait.name}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        if (type != null) {
            saveDataTrait = System.Activator.CreateInstance(type) as SaveDataTrait;
        } else {
            saveDataTrait = new SaveDataTrait();
        }
        return saveDataTrait;
    }
    #endregion

    #region Saving
    public void SaveCurrentProgress() {
        SaveDataCurrentProgress saveData = new SaveDataCurrentProgress();
        saveData.SaveDate();
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

        SaveGame.Save(UtilityScripts.Utilities.gameSavePath + savedCurrentProgressFileName, saveData);
    }
    public void SavePlayerData() {
        //PlayerManager.Instance.player.SaveSummons();
        //PlayerManager.Instance.player.SaveTileObjects();
        SaveDataPlayer save = currentSaveDataPlayer;
        SaveGame.Save(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName, save);
    }
    #endregion

    #region Loading
    public void LoadCurrentProgress() {
        //TODO
    }
    public void LoadPlayerData() {
        //if(UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + saveFileName)) {
        //    SetCurrentSave(SaveGame.Load<Save>(UtilityScripts.Utilities.gameSavePath + saveFileName));
        //}
        if (WorldConfigManager.Instance.isTutorialWorld) {
            currentSaveDataPlayer = new SaveDataPlayer();
            currentSaveDataPlayer.InitializeInitialData();
        } else {
            if (UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName)) {
                SetCurrentSaveDataPlayer(SaveGame.Load<SaveDataPlayer>(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName));
            }
            if (currentSaveDataPlayer == null) {
                currentSaveDataPlayer = new SaveDataPlayer();
                currentSaveDataPlayer.InitializeInitialData();
            }
        }
        if (alwaysResetBonusTutorialsOnStartup) {
            currentSaveDataPlayer.ResetBonusTutorialProgress();
        }
        if (alwaysResetSpecialPopupsOnStartup) {
            currentSaveDataPlayer.ResetSpecialPopupsProgress();
        }
    }
    #endregion  
}
