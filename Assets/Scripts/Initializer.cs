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
        DatabaseManager.Instance.Initialize();
        yield return null;
        CharacterManager.Instance.Initialize();
        RaceManager.Instance.Initialize();
        TraitManager.Instance.Initialize();
        yield return null;
        LandmarkManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
        CrimeManager.Instance.Initialize();
        yield return null;
        TimerHubUI.Instance.Initialize();
        WorldMapCameraMove.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
        yield return null;
        ObjectPoolManager.Instance.InitializeObjectPools();
        UIManager.Instance.InitializeUI();
        InteractionManager.Instance.Initialize();
        yield return null;
        JobManager.Instance.Initialize();
        PlayerUI.Instance.Initialize();
        RandomNameGenerator.Initialize();
        WorldEventManager.Instance.Initialize();
        yield return null;
        PlayerSkillManager.Instance.ResetSpellsInUse();
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
            PlayerManager.Instance.player.playerSkillComponent.OnLoadSaveData();
        }
        TutorialManager.Instance.Initialize();
        QuestManager.Instance.InitializeAfterLoadoutPicked();
        if (WorldSettings.Instance.worldSettingsData.IsScenarioMap() && !SaveManager.Instance.useSaveData) {
            MapGenerationFinalization.ScenarioItemGenerationAfterPickingLoadout();    
        }
    }
    
}
