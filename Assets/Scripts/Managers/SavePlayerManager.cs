using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;

public class SavePlayerManager : MonoBehaviour {
    private const string savedPlayerDataFileName = "SAVED_PLAYER_DATA_2";
    public SaveDataPlayer currentSaveDataPlayer { get; private set; }

    #region getters
    public bool hasSavedDataPlayer => currentSaveDataPlayer != null;
    #endregion

    #region General
    public void SetCurrentSaveDataPlayer(SaveDataPlayer save) {
        currentSaveDataPlayer = save;
    }
    #endregion

    #region Saving
    public void SavePlayerData() {
        SaveDataPlayer save = currentSaveDataPlayer;
        SaveGame.Save(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName, save);
    }
    #endregion

    #region Loading
    public void LoadSaveDataPlayer() {
        if (UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName)) {
            SaveDataPlayer saveDataPlayer = SaveGame.Load<SaveDataPlayer>(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName);
            if (saveDataPlayer != null) {
                saveDataPlayer.ProcessOnLoad();
                SetCurrentSaveDataPlayer(saveDataPlayer);
            } else {
                CreateNewSaveDataPlayer();
            }
        }
    }
    public void CreateNewSaveDataPlayer() {
        SaveDataPlayer saveDataPlayer = new SaveDataPlayer();
        saveDataPlayer.InitializeInitialData();
        SetCurrentSaveDataPlayer(saveDataPlayer);
    }
    #endregion
}
