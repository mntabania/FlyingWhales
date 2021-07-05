using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Quests;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class SkillTreeSelector : MonoBehaviour {

    [SerializeField] private HorizontalScrollSnap _horizontalScrollSnap;
    [SerializeField] private Button continueBtn;
    [SerializeField] private RuinarchToggle moreLoadoutOptionsToggle;
    
    [SerializeField] private Toggle[] archetypeToggles;
    [SerializeField] private PlayerSkillLoadoutUI[] playerLoadoutUI;

    public void Initialize() {
        this.gameObject.SetActive(true);
        _horizontalScrollSnap.Awake();
        for (int i = 0; i < playerLoadoutUI.Length; i++) {
            playerLoadoutUI[i].Initialize();
            playerLoadoutUI[i].SetMoreLoadoutOptions(false, false); //SaveManager.Instance.currentSaveDataPlayer.moreLoadoutOptions
        }
        if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.forcedArchetypes != null) {
            //world settings has a forced archetype, disable all other archetypes except forced archetype
            for (int i = 0; i < archetypeToggles.Length; i++) {
                Toggle toggle = archetypeToggles[i];
                PlayerSkillLoadoutUI loadoutUI = playerLoadoutUI[i];
                if (!WorldSettings.Instance.worldSettingsData.playerSkillSettings.forcedArchetypes.Contains(loadoutUI.loadout.archetype)) {
                    toggle.gameObject.SetActive(false);
                    loadoutUI.gameObject.SetActive(false);
                    _horizontalScrollSnap.RemoveChild(loadoutUI.transform.GetSiblingIndex(), out var removed);
                    toggle.SetIsOnWithoutNotify(false);
                }
            }
        } else {
            //only enable the main archetypes
            for (int i = 0; i < archetypeToggles.Length; i++) {
                Toggle toggle = archetypeToggles[i];
                PlayerSkillLoadoutUI loadoutUI = playerLoadoutUI[i];
                if (!loadoutUI.loadout.archetype.IsMainArchetype()) {
                    toggle.gameObject.SetActive(false);
                    loadoutUI.gameObject.SetActive(false);
                    _horizontalScrollSnap.RemoveChild(loadoutUI.transform.GetSiblingIndex(), out var removed);
                    toggle.SetIsOnWithoutNotify(false);
                }
            }
        }
        moreLoadoutOptionsToggle.SetIsOnWithoutNotify(false);//
        this.gameObject.SetActive(false);
    }
    public void Show() {
        this.gameObject.SetActive(true);
        _horizontalScrollSnap.GoToScreen(0);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void OnToggleMoreLoadoutOptions(bool state) {
        MoreLoadoutOptions(state);
    }

    private void MoreLoadoutOptions(bool state) {
        for (int i = 0; i < playerLoadoutUI.Length; i++) {
            playerLoadoutUI[i].SetMoreLoadoutOptions(state, true);
        }
    }

    public void OnClickContinue() {
        continueBtn.interactable = false;
        PLAYER_ARCHETYPE selectedArchetype = GetSelectedArchetype();
        PlayerSkillManager.Instance.SetSelectedArchetype(selectedArchetype);
        if (selectedArchetype == PLAYER_ARCHETYPE.Lich) {
            // //add 1 charge of skeleton marauder to lich
            // SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(RACE.SKELETON, "Marauder");
            // PlayerManager.Instance.player.playerSkillComponent.AddCharges(summonPlayerSkill.type, 1);
            //Set undead faction as friendly with player faction
            PlayerManager.Instance.player.playerFaction.SetRelationshipFor(FactionManager.Instance.undeadFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
        }
        SaveManager.Instance.currentSaveDataPlayer.SetMoreLoadoutOptions(moreLoadoutOptionsToggle.isOn);
        BroadcastLoadoutSelectedSignals();
        // PlagueDisease.Instance.OnLoadoutPicked();
        UIManager.Instance.initialWorldSetupMenu.Hide();
        InnerMapManager.Instance.TryShowLocationMap(GridMap.Instance.mainRegion);
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        portal.CenterOnStructure();
        
        if (WorldConfigManager.Instance.mapGenerationData.isGeneratingTileObjects) {
            //tile object generation has not finished yet. Wait for it to finish, then start progression
            UIManager.Instance.ShowWaitForTileObjectGenerationToFinishWindow();
        } else {
            GameManager.Instance.StartProgression();
        }
    }
    public void LoadLoadout(PLAYER_ARCHETYPE archetype) {
        PlayerSkillManager.Instance.SetSelectedArchetype(archetype);
        BroadcastLoadoutSelectedSignals();
        GameManager.Instance.LoadProgression();
        UIManager.Instance.initialWorldSetupMenu.Hide();

        InnerMapManager.Instance.TryShowLocationMap(GridMap.Instance.mainRegion);
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        portal.CenterOnStructure();
    }
    private void BroadcastLoadoutSelectedSignals() {
        Messenger.Broadcast(UISignals.SAVE_LOADOUTS);
        Messenger.Broadcast(UISignals.START_GAME_AFTER_LOADOUT_SELECT);
    }

    private PLAYER_ARCHETYPE GetSelectedArchetype() {
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
            return PLAYER_ARCHETYPE.Tutorial;
        } else if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled) {
            return PLAYER_ARCHETYPE.Ravager;
        } else {
            for (int i = 0; i < archetypeToggles.Length; i++) {
                Toggle archetypeToggle = archetypeToggles[i];
                if (archetypeToggle.gameObject.activeInHierarchy && archetypeToggle.isOn) {
                    return (PLAYER_ARCHETYPE) System.Enum.Parse(typeof(PLAYER_ARCHETYPE), UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(archetypeToggle.gameObject.name));
                }
            }
            return PLAYER_ARCHETYPE.Normal;
        }
    }

    private void OnScreenResolutionChanged() {
        StartCoroutine(_horizontalScrollSnap.UpdateLayoutCoroutine());
    }
    
    public void OnHoverMoreLoadoutOptions() {
        string text = "Unlock all available optional abilities regardless of Archetype.";
        text += $"\n<color=red>Warning: Toggling this off will reset all optional loadouts.</color>";
        UIManager.Instance.ShowSmallInfo(text);
    }
    public void OnHoverOutMoreLoadoutOptions() {
        UIManager.Instance.HideSmallInfo();
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
