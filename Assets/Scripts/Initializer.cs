using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Tutorial;
using UnityEngine;

public class Initializer : MonoBehaviour {
    public IEnumerator InitializeDataBeforeWorldCreation() {
        LocalizationManager.Instance.Initialize();
        GameManager.Instance.Initialize();
        CharacterManager.Instance.Initialize();
        RaceManager.Instance.Initialize();
        TraitManager.Instance.Initialize();
        // SecretManager.Instance.Initialize();
        LandmarkManager.Instance.Initialize();
        //CombatManager.Instance.Initialize();
        PlayerManager.Instance.Initialize();
        TimerHubUI.Instance.Initialize();

        CameraMove.Instance.Initialize();
        InnerMapManager.Instance.Initialize();
        ObjectPoolManager.Instance.InitializeObjectPools();

        UIManager.Instance.InitializeUI();

        InteractionManager.Instance.Initialize();

        // TokenManager.Instance.Initialize();
        JobManager.Instance.Initialize();
        PlayerUI.Instance.Initialize();
        RandomNameGenerator.Initialize();
        yield return null;
    }

    public void InitializeDataAfterWorldCreation() {
        PlayerUI.Instance.InitializeAfterGameLoaded();
        LightingManager.Instance.Initialize();
        TutorialManager.Instance.Initialize();
    }
}
