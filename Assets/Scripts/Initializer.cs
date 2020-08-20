using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Managers;
using Quests;
using Tutorial;
using UnityEngine;

public class Initializer : MonoBehaviour {
    public IEnumerator InitializeDataBeforeWorldCreation() {
        LocalizationManager.Instance.Initialize();
        GameManager.Instance.Initialize();
        CharacterManager.Instance.Initialize();
        RaceManager.Instance.Initialize();
        TraitManager.Instance.Initialize();
        LandmarkManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
        CrimeManager.Instance.Initialize();
        TimerHubUI.Instance.Initialize();

        WorldMapCameraMove.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();

        UIManager.Instance.InitializeUI();

        InteractionManager.Instance.Initialize();

        JobManager.Instance.Initialize();
        PlayerUI.Instance.Initialize();
        RandomNameGenerator.Initialize();
        WorldEventManager.Instance.Initialize();
        yield return null;
    }

    public void InitializeDataAfterWorldCreation() {
        PlayerUI.Instance.InitializeAfterGameLoaded();
        LightingManager.Instance.Initialize();
        QuestManager.Instance.InitializeAfterGameLoaded();
        AudioManager.Instance.OnGameLoaded();
    }
    public void InitializeDataAfterLoadoutSelection() {
        if (!SaveManager.Instance.useSaveData) {
            //Do not load player data if save data is used because we will use the data of the saved one
            PlayerManager.Instance.player.LoadPlayerData(SaveManager.Instance.currentSaveDataPlayer);
        }
        PlayerUI.Instance.InitializeAfterLoadOutPicked();
        if (SaveManager.Instance.useSaveData) {
            PlayerUI.Instance.OnLoadSaveData();
        }
        TutorialManager.Instance.Initialize();
        QuestManager.Instance.InitializeAfterLoadoutPicked();
    }
}
