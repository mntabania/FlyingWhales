using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;
using System.IO;

public class SavePlayerManager : MonoBehaviour {
    public const string savedPlayerDataFileName = "SAVED_PLAYER_DATA_2";
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

    public string GetSaveContent() {
        string content = string.Empty;
        if (UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName)){
            string path = UtilityScripts.Utilities.gameSavePath + savedPlayerDataFileName;

            //Read the text from directly from the test.txt file
            StreamReader reader = new StreamReader(path);
            content = reader.ReadToEnd();
            reader.Close();
        }
        Debug.Log(content);
        return content;
    }
}
