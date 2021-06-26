using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Settings;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class BuildListUI : PopupMenuBase {

    [SerializeField] private Toggle buildToggle;
    [SerializeField] private ScrollRect buildingsScrollRect;
    [SerializeField] private GameObject spellItemPrefab;
    [SerializeField] private UIHoverPosition tooltipPosition;
    
    private List<SpellItem> buildItems = new List<SpellItem>();
    
    private void Awake() {
        buildToggle.interactable = false;
        Close();
    }
    public override void Open() {
        base.Open();
        UpdateBuildList();
        SubscribeListeners();
        buildToggle.SetIsOnWithoutNotify(true);
    }
    public override void Close() {
        buildToggle.SetIsOnWithoutNotify(false);
        UnsubscribeListeners();
        base.Close();
    }

    #region Listeners
    private void SubscribeListeners() {
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
    }
    private void UnsubscribeListeners() {
        Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
    }
    private void OnInnerMapOpened(Region region) {
        UpdateBuildList();
    }
    private void OnInnerMapClosed(Region region) {
        UpdateBuildList();
    }
    #endregion

    public void Initialize() {
        PopulateBuildingList();
        Messenger.AddListener(UISignals.UPDATE_BUILD_LIST, UpdateBuildList);
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, OnPlayerGainedDemonicStructure);
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_LOST_DEMONIC_STRUCTURE, OnPlayerLostDemonicStructure);
        buildToggle.interactable = true;
    }
    private void OnPlayerGainedDemonicStructure(PLAYER_SKILL_TYPE p_structureType) {
        CreateStructureItem(p_structureType);
    }
    private void OnPlayerLostDemonicStructure(PLAYER_SKILL_TYPE p_structureType) {
        DeleteStructureItem(p_structureType);
    }
    private void PopulateBuildingList() {
        UtilityScripts.Utilities.DestroyChildrenObjectPool(buildingsScrollRect.content);  
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills.Count; i++) {
            PLAYER_SKILL_TYPE structureSpell = PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills[i];
            CreateStructureItem(structureSpell);
        }
    }
    private void CreateStructureItem(PLAYER_SKILL_TYPE structureSpell) {
        DemonicStructurePlayerSkill demonicStructurePlayerSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureSpell);
        GameObject spellNameplate = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellItemPrefab.name, Vector3.zero, Quaternion.identity, buildingsScrollRect.content);
        SpellItem spellItem = spellNameplate.GetComponent<SpellItem>();
        spellItem.SetObject(demonicStructurePlayerSkill);
        spellItem.SetInteractableChecker(CanChooseLandmark);
        spellItem.AddHoverEnterAction(OnHoverSpellItem);
        spellItem.AddHoverExitAction(OnHoverExitSpellItem);
        spellItem.ForceUpdateInteractableState();
        buildItems.Add(spellItem);
    }
    private void DeleteStructureItem(PLAYER_SKILL_TYPE structureSpell) {
        SpellItem item = GetStructureItem(structureSpell);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item);
            buildItems.Remove(item);
        }
    }
    private SpellItem GetStructureItem(PLAYER_SKILL_TYPE structureSpell) {
        for (int i = 0; i < buildItems.Count; i++) {
            SpellItem item = buildItems[i];
            if (item.spellData.type == structureSpell) {
                return item;
            }
        }
        return null;
    }
    private void OnHoverSpellItem(SkillData spellData) {
        PlayerUI.Instance.OnHoverSpell(spellData, tooltipPosition);
    }
    private void OnHoverExitSpellItem(SkillData spellData) {
        PlayerUI.Instance.OnHoverOutSpell(spellData);
    }
    private void UpdateBuildList() {
        for (int i = 0; i < buildItems.Count; i++) {
            SpellItem item = buildItems[i];
            item.ForceUpdateInteractableState();
            // if (item.toggle.interactable) {
            //     item.transform.SetAsFirstSibling();
            // } else {
            //     item.transform.SetAsLastSibling();
            // }
        }
    }
    private bool CanChooseLandmark(SkillData p_spellData) {
        bool canChooseLandmark = p_spellData.CanPerformAbility();

        if (canChooseLandmark) {
            // if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
            //     if (p_spellData.type == PLAYER_SKILL_TYPE.WATCHER) {
            //         return TutorialManager.Instance.HasTutorialBeenCompletedInCurrentPlaythrough(TutorialManager.Tutorial.Share_An_Intel) ||
            //                TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Share_An_Intel);
            //     } else if (p_spellData.type == PLAYER_SKILL_TYPE.KENNEL) {
            //         return TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Build_A_Kennel) ||
            //                TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Build_A_Kennel);
            //     }
            // }
            // if (structureType == SPELL_TYPE.EYE && InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.EYE)) {
            //     canChooseLandmark = false; //only 1 eye per region.
            // }
            // if (structureType == SPELL_TYPE.MEDDLER && PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER)) {
            //     canChooseLandmark = false; //only 1 finger at a time.
            // }
        }
        return canChooseLandmark;
    }
}