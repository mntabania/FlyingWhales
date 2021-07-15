using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using Managers;
using Quests;
using Tutorial;
using UnityEngine;

public class Initializer : MonoBehaviour {
    public IEnumerator InitializeDataBeforeWorldCreationEnumerator() {
        //BaseSettlement.onSettlementBuilt = null; //TODO: Make this better
        LocalizationManager.Instance.Initialize();
        GameManager.Instance.Initialize();
        SaveManager.Instance.PrepareTempDirectory();
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
        //TimerHubUI.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
        yield return null;
        // ObjectPoolManager.Instance.InitializeObjectPools();
        UIManager.Instance.InitializeUI();
        InteractionManager.Instance.Initialize();
        yield return null;
        JobManager.Instance.Initialize();
        PlayerUI.Instance.Initialize();
        WorldEventManager.Instance.Initialize();
        yield return null;
        PlayerSkillManager.Instance.ResetSpellsInUse();
        yield return null;
        PlayerSkillManager.Instance.ResetSummonPlayerSkills();
        yield return null;
        CombatManager.Instance.Initialize();
    }
    public void InitializeDataBeforeWorldCreationMainThread() {
        LocalizationManager.Instance.Initialize();
        GameManager.Instance.Initialize();
        SaveManager.Instance.PrepareTempDirectory();
        DatabaseManager.Instance.Initialize();
        CharacterManager.Instance.Initialize();
        TraitManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
        UIManager.Instance.InitializeUI();
        PlayerUI.Instance.Initialize();
        WorldEventManager.Instance.Initialize();
    }
    public void InitializeDataBeforeWorldCreationOtherThread(object state) {
        LoadThreadQueueItem threadItem = state as LoadThreadQueueItem;
        RaceManager.Instance.Initialize();
        LandmarkManager.Instance.Initialize();
        CrimeManager.Instance.Initialize();
        InteractionManager.Instance.Initialize();
        JobManager.Instance.Initialize();
        PlayerSkillManager.Instance.ResetSpellsInUse();
        PlayerSkillManager.Instance.ResetSummonPlayerSkills();
        CombatManager.Instance.Initialize();
        threadItem.isDone = true;
    }

    public void InitializeDataAfterWorldCreation() {
        PlayerUI.Instance.InitializeAfterGameLoaded();
        FactionInfoHubUI.Instance.InitializeAfterGameLoaded();
        LightingManager.Instance.Initialize();
        QuestManager.Instance.InitializeAfterGameLoaded();
        AudioManager.Instance.OnGameLoaded();
    }
    public void InitializeDataAfterLoadoutSelection() {
        if (!SaveManager.Instance.useSaveData) {
            //Do not load player data if save data is used because we will use the data of the saved one
            PlayerManager.Instance.player.LoadPlayerData(SaveManager.Instance.currentSaveDataPlayer);
        }
        UIManager.Instance.InitializeAfterLoadOutPicked();
        PlayerUI.Instance.InitializeAfterLoadOutPicked();
        if (SaveManager.Instance.useSaveData) {
            PlayerManager.Instance.player.playerSkillComponent.OnLoadSaveData();
        }
        TutorialManager.Instance.Initialize();
        QuestManager.Instance.InitializeAfterLoadoutPicked();
        if (!SaveManager.Instance.useSaveData) {
            MapGenerationFinalization.ItemGenerationAfterPickingLoadout();    
        }
        AudioManager.Instance.OnLoadoutSelected();
    }
    
}
