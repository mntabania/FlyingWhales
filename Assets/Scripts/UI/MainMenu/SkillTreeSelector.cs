using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Quests;
using Ruinarch;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class SkillTreeSelector : MonoBehaviour {

    [SerializeField] private HorizontalScrollSnap _horizontalScrollSnap;
    [SerializeField] private Button continueBtn;

    [SerializeField] private Toggle[] archetypeToggles;

    public PlayerSkillLoadoutUI[] playerLoadoutUI;

    public void Initialize() {
        this.gameObject.SetActive(true);
        _horizontalScrollSnap.Awake();
        for (int i = 0; i < playerLoadoutUI.Length; i++) {
            playerLoadoutUI[i].Initialize();
        }
        // if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
        //     //if second world then disable ravager and lich builds, and go to puppet master build
        //     _horizontalScrollSnap.RemoveAllChildren(out var childrenRemoved);
        //     for (int i = 0; i < archetypeToggles.Length; i++) {
        //         Toggle toggle = archetypeToggles[i];
        //         PlayerSkillLoadoutUI loadoutUI = playerLoadoutUI[i];
        //         if (toggle.gameObject.name == "Second World") {
        //             toggle.isOn = true;
        //             _horizontalScrollSnap.AddChild(loadoutUI.gameObject);
        //         } else {
        //             toggle.gameObject.SetActive(false);
        //             loadoutUI.gameObject.SetActive(false);
        //         }
        //     }
        // } else {
            //disable other non main loadouts
            //Second World
            for (int i = 0; i < archetypeToggles.Length; i++) {
                Toggle toggle = archetypeToggles[i];
                PlayerSkillLoadoutUI loadoutUI = playerLoadoutUI[i];
                if (toggle.gameObject.name == "Second World") {
                    toggle.gameObject.SetActive(false);
                    loadoutUI.gameObject.SetActive(false);
                    _horizontalScrollSnap.RemoveChild(i, out var removed);
                }
            }
        // }
        this.gameObject.SetActive(false);
    }
    public void Show() {
        this.gameObject.SetActive(true);
        _horizontalScrollSnap.GoToScreen(0);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void OnClickContinue() {
        continueBtn.interactable = false;
        PlayerSkillManager.Instance.SetSelectedArchetype(GetSelectedArchetype());
        Messenger.Broadcast(Signals.START_GAME_AFTER_LOADOUT_SELECT);
        GameManager.Instance.StartProgression();
        UIManager.Instance.initialWorldSetupMenu.Hide();
        
        WorldMapCameraMove.Instance.CenterCameraOn(WorldConfigManager.Instance.mapGenerationData.portal.gameObject);
        InnerMapManager.Instance.TryShowLocationMap(WorldConfigManager.Instance.mapGenerationData.portal.region);
        InnerMapCameraMove.Instance.CenterCameraOnTile(WorldConfigManager.Instance.mapGenerationData.portal);
    }
    public void SkipLoadout(PLAYER_ARCHETYPE archetype) {
        PlayerSkillManager.Instance.SetSelectedArchetype(archetype);
        Messenger.Broadcast(Signals.START_GAME_AFTER_LOADOUT_SELECT);
        GameManager.Instance.StartProgression();
        UIManager.Instance.initialWorldSetupMenu.Hide();

        WorldMapCameraMove.Instance.CenterCameraOn(WorldConfigManager.Instance.mapGenerationData.portal.gameObject);
        InnerMapManager.Instance.TryShowLocationMap(WorldConfigManager.Instance.mapGenerationData.portal.region);
        InnerMapCameraMove.Instance.CenterCameraOnTile(WorldConfigManager.Instance.mapGenerationData.portal);
    }

    private PLAYER_ARCHETYPE GetSelectedArchetype() {
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
            return PLAYER_ARCHETYPE.Tutorial;
        } else {
            for (int i = 0; i < archetypeToggles.Length; i++) {
                if (archetypeToggles[i].isOn) {
                    return (PLAYER_ARCHETYPE) System.Enum.Parse(typeof(PLAYER_ARCHETYPE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(archetypeToggles[i].gameObject.name));
                }
            }
            return PLAYER_ARCHETYPE.Normal;
        }
    }

    private void OnScreenResolutionChanged() {
        StartCoroutine(_horizontalScrollSnap.UpdateLayoutCoroutine());
    }
    
    // #region Monobehaviours
    // private Vector2Int currentResolution;
    // private void Start () {
    //     currentResolution = new Vector2Int(Screen.width, Screen.height);
    // }
    //  
    // private void Update () {
    //     if (currentResolution.x != Screen.width || currentResolution.y != Screen.height) {
    //         //Do stuff
    //         currentResolution = new Vector2Int(Screen.width, Screen.height);
    //         OnScreenResolutionChanged();
    //     }
    // }
    // #endregion
}
