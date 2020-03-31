using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;
using Inner_Maps;
using Traits;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    public Save currentSave { get; private set; }

    private const string saveFileName = "CURRENT_SAVE_FILE";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    public void SetCurrentSave(Save save) {
        currentSave = save;
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
    public void LoadSaveData() {
        if(UtilityScripts.Utilities.DoesFileExist(UtilityScripts.Utilities.gameSavePath + saveFileName)) {
            SetCurrentSave(SaveGame.Load<Save>(UtilityScripts.Utilities.gameSavePath + saveFileName));
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
