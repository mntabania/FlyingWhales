using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillLoadoutUI : MonoBehaviour {
    public PlayerSkillLoadout loadout;
    public GameObject skillSlotItemPrefab;
    public PlayerSkillLoadoutObjectPicker objectPicker;
    public PlayerSkillDetailsTooltip skillDetailsTooltip;

    public ScrollRect spellsScrollRect;
    public ScrollRect afflictionsScrollRect;
    public ScrollRect minionsScrollRect;
    public ScrollRect structuresScrollRect;
    public ScrollRect miscsScrollRect;

    public Toggle spellsTab;
    public Toggle afflictionsTab;
    public Toggle minionsTab;
    public Toggle structuresTab;
    public Toggle miscsTab;

    public GroupedSkillSlotItems spellsSkillSlotItems { get; private set; }
    public GroupedSkillSlotItems afflictionsSkillSlotItems { get; private set; }
    public GroupedSkillSlotItems minionsSkillSlotItems { get; private set; }
    public GroupedSkillSlotItems structuresSkillSlotItems { get; private set; }
    public GroupedSkillSlotItems miscsSkillSlotItems { get; private set; }

    private SkillSlotItem pickedSlotItem;
    private List<PLAYER_SKILL_TYPE> loadoutChoices;

    public bool moreLoadoutOptions { get; set; }

    public void Initialize() {
        spellsSkillSlotItems = new GroupedSkillSlotItems();
        afflictionsSkillSlotItems = new GroupedSkillSlotItems();
        minionsSkillSlotItems = new GroupedSkillSlotItems();
        structuresSkillSlotItems = new GroupedSkillSlotItems();
        miscsSkillSlotItems = new GroupedSkillSlotItems();
        loadoutChoices = new List<PLAYER_SKILL_TYPE>();
        LoadSkillDataToUI(loadout.spells.fixedSkills, loadout.spells.extraSlots, spellsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraSpells(loadout.archetype), spellsScrollRect.content);
        LoadSkillDataToUI(loadout.afflictions.fixedSkills, loadout.afflictions.extraSlots, afflictionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraAfflictions(loadout.archetype), afflictionsScrollRect.content);
        LoadSkillDataToUI(loadout.minions.fixedSkills, loadout.minions.extraSlots, minionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMinions(loadout.archetype), minionsScrollRect.content);
        LoadSkillDataToUI(loadout.structures.fixedSkills, loadout.structures.extraSlots, structuresSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraStructures(loadout.archetype), structuresScrollRect.content);
        LoadSkillDataToUI(loadout.miscs.fixedSkills, loadout.miscs.extraSlots, miscsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMiscs(loadout.archetype), miscsScrollRect.content);

        Messenger.AddListener<SkillSlotItem, PLAYER_ARCHETYPE>(UISignals.SKILL_SLOT_ITEM_CLICKED, OnClickSkillSlotItem);
        Messenger.AddListener(UISignals.SAVE_LOADOUTS, SaveLoadoutSettings);

        spellsTab.isOn = true;
    }
    public void OnDestroy() {
        Messenger.RemoveListener<SkillSlotItem, PLAYER_ARCHETYPE>(UISignals.SKILL_SLOT_ITEM_CLICKED, OnClickSkillSlotItem);
        Messenger.RemoveListener(UISignals.SAVE_LOADOUTS, SaveLoadoutSettings);
    }

    #region General
    private void SaveLoadoutSettings() {
        if (SaveManager.Instance.useSaveData) {
            return; //If using a save data, do not reset player skills because we will use the ones that are saved
        }
        SaveManager.Instance.currentSaveDataPlayer.ClearLoadoutSaveData(loadout.archetype);
        SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraSpells(loadout.archetype, spellsSkillSlotItems.extraSkillSlotItems);
        SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraAfflictions(loadout.archetype, afflictionsSkillSlotItems.extraSkillSlotItems);
        SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraMinions(loadout.archetype, minionsSkillSlotItems.extraSkillSlotItems);
        SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraStructures(loadout.archetype, structuresSkillSlotItems.extraSkillSlotItems);
        SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraMiscs(loadout.archetype, miscsSkillSlotItems.extraSkillSlotItems);    
    }
    #endregion

    #region Tabs
    public void OnClickSpellsTab(bool state) {
        if (state) {
            if(spellsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && spellsSkillSlotItems.extraSkillSlotItems.Count <= 0) {
                LoadSkillDataToUI(loadout.spells.fixedSkills, loadout.spells.extraSlots, spellsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraSpells(loadout.archetype), spellsScrollRect.content);
            }
        }
    }
    public void OnClickAfflictionsTab(bool state) {
        if (state) {
            if (afflictionsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && afflictionsSkillSlotItems.extraSkillSlotItems.Count <= 0) {
                LoadSkillDataToUI(loadout.afflictions.fixedSkills, loadout.afflictions.extraSlots, afflictionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraAfflictions(loadout.archetype), afflictionsScrollRect.content);
            }
        }
    }
    public void OnClickMinionsTab(bool state) {
        if (state) {
            if (minionsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && minionsSkillSlotItems.extraSkillSlotItems.Count <= 0) {
                LoadSkillDataToUI(loadout.minions.fixedSkills, loadout.minions.extraSlots, minionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMinions(loadout.archetype), minionsScrollRect.content);
            }
        }
    }
    public void OnClickStructuresTab(bool state) {
        if (state) {
            if (structuresSkillSlotItems.fixedSkillSlotItems.Count <= 0 && structuresSkillSlotItems.extraSkillSlotItems.Count <= 0) {
                LoadSkillDataToUI(loadout.structures.fixedSkills, loadout.structures.extraSlots, structuresSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraStructures(loadout.archetype), structuresScrollRect.content);
            }
        }
    }
    public void OnClickMiscsTab(bool state) {
        if (state) {
            if (miscsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && miscsSkillSlotItems.extraSkillSlotItems.Count <= 0) {
                LoadSkillDataToUI(loadout.miscs.fixedSkills, loadout.miscs.extraSlots, miscsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMiscs(loadout.archetype), miscsScrollRect.content);
            }
        }
    }
    #endregion

    #region Load Data
    private void LoadSkillDataToUI(List<PLAYER_SKILL_TYPE> fixedSkills, int extraSlots, GroupedSkillSlotItems groupedSkillSlotItems, List<PLAYER_SKILL_TYPE> extraSkills, Transform parent) {
        if(fixedSkills != null) {
            for (int i = 0; i < fixedSkills.Count; i++) {
                PLAYER_SKILL_TYPE fixedSkill = fixedSkills[i];
                SkillSlotItem skillSlotItem = CreateNewSkillSlotItem(parent);
                skillSlotItem.SetSkillSlotItem(loadout.archetype, fixedSkill, true);
                skillSlotItem.SetOnHoverEnterAction(OnHoverEnterSkillSlotItem);
                skillSlotItem.SetOnHoverExitAction(OnHoverExitSkillSlotItem);
                //skillSlotItem.SetInteractable(false);
                groupedSkillSlotItems.AddFixedSkillSlotItem(skillSlotItem);
            }
        }
        for (int i = 0; i < extraSlots; i++) {
            PlayerSkillData skillData = null;
            if(extraSkills != null && extraSkills.Count > 0 && i < extraSkills.Count) {
                skillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(extraSkills[i]);
            }
            if (skillData != null) {
                var playerSkillData = PlayerSkillManager.Instance.GetSkillData(skillData.skill);
                if (playerSkillData == null) {
                    continue;
                }    
            }
            SkillSlotItem skillSlotItem = CreateNewSkillSlotItem(parent);
            skillSlotItem.SetSkillSlotItem(loadout.archetype, skillData, false);
            skillSlotItem.SetOnHoverEnterAction(OnHoverEnterSkillSlotItem);
            skillSlotItem.SetOnHoverExitAction(OnHoverExitSkillSlotItem);
            //skillSlotItem.SetInteractable(true);
            groupedSkillSlotItems.AddExtraSkillSlotItem(skillSlotItem);
        }
    }
    #endregion

    #region Skill Slot Item
    private SkillSlotItem CreateNewSkillSlotItem(Transform parent) {
        GameObject go = Instantiate(skillSlotItemPrefab, parent);
        go.transform.localPosition = Vector3.zero;
        return go.GetComponent<SkillSlotItem>();
    }
    private void OnClickSkillSlotItem(SkillSlotItem slotItem, PLAYER_ARCHETYPE archetype) {
        if (archetype != loadout.archetype) { return; }
        pickedSlotItem = slotItem;
        loadoutChoices.Clear();
        PLAYER_SKILL_TYPE[] availableSkills = null;
        List<PLAYER_SKILL_TYPE> fixedSkills = null;
        GroupedSkillSlotItems groupedSkillSlotItems = null;
        if (spellsTab.isOn) {
            availableSkills = loadout.availableSpells;
            if (moreLoadoutOptions) {
                availableSkills = PlayerSkillManager.Instance.allSpells;
            }
            fixedSkills = loadout.spells.fixedSkills;
            groupedSkillSlotItems = spellsSkillSlotItems;
        } else if (afflictionsTab.isOn) {
            availableSkills = loadout.availableAfflictions;
            if (moreLoadoutOptions) {
                availableSkills = PlayerSkillManager.Instance.allAfflictions;
            }
            fixedSkills = loadout.afflictions.fixedSkills;
            groupedSkillSlotItems = afflictionsSkillSlotItems;
        } else if (minionsTab.isOn) {
            availableSkills = loadout.availableMinions;
            if (moreLoadoutOptions) {
                availableSkills = PlayerSkillManager.Instance.allMinionPlayerSkills;
            }
            fixedSkills = loadout.minions.fixedSkills;
            groupedSkillSlotItems = minionsSkillSlotItems;
        } else if (structuresTab.isOn) {
            availableSkills = loadout.availableStructures;
            if (moreLoadoutOptions) {
                availableSkills = PlayerSkillManager.Instance.allDemonicStructureSkills;
            }
            fixedSkills = loadout.structures.fixedSkills;
            groupedSkillSlotItems = structuresSkillSlotItems;
        } else if (miscsTab.isOn) {
            availableSkills = loadout.availableMiscs;
            if (moreLoadoutOptions) {
                availableSkills = PlayerSkillManager.Instance.allPlayerActions;
            }
            fixedSkills = loadout.miscs.fixedSkills;
            groupedSkillSlotItems = miscsSkillSlotItems;
        }
        if(availableSkills != null) {
            for (int i = 0; i < availableSkills.Length; i++) {
                PLAYER_SKILL_TYPE skillType = availableSkills[i];
                if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                    if (!PlayerSkillManager.Instance.constantSkills.Contains(skillType) && !fixedSkills.Contains(skillType) && !groupedSkillSlotItems.HasExtraSkill(skillType)) {
                        loadoutChoices.Add(skillType);
                    }
                }
            }
        }
        if (loadoutChoices.Count > 0) {
            objectPicker.ShowLoadoutPicker(loadoutChoices, OnConfirmSkill, OnHoverEnterSkill, OnHoverExitSkill);
        }
    }
    private void OnConfirmSkill(PlayerSkillData skillData) {
        if(pickedSlotItem) {
            pickedSlotItem.SetSkillSlotItem(loadout.archetype, skillData, false);
        }
    }
    private void OnHoverEnterSkill(PlayerSkillData skillData) {
        skillDetailsTooltip.ShowPlayerSkillDetails(skillData, position: objectPicker.hoverPos);
    }
    private void OnHoverExitSkill(PlayerSkillData skillData) {
        skillDetailsTooltip.HidePlayerSkillDetails();
    }
    private void OnHoverEnterSkillSlotItem(PlayerSkillData skillData) {
        if(skillData != null) {
            if (loadout.archetype.IsScenarioArchetype()) {
                ScenarioData scenarioData = WorldSettings.Instance.GetScenarioDataByWorldType(WorldSettings.Instance.worldSettingsData.worldType);
                skillDetailsTooltip.ShowPlayerSkillDetails(skillData, scenarioData.GetLevelForPower(skillData.skill));
            } else {
                skillDetailsTooltip.ShowPlayerSkillDetails(skillData);    
            }
            
        }
    }
    private void OnHoverExitSkillSlotItem(PlayerSkillData skillData) {
        skillDetailsTooltip.HidePlayerSkillDetails();
    }
    public void ClearExtraSlotItems() {
        for (int i = 0; i < spellsSkillSlotItems.extraSkillSlotItems.Count; i++) {
            spellsSkillSlotItems.extraSkillSlotItems[i].ClearData();
        }
        for (int i = 0; i < afflictionsSkillSlotItems.extraSkillSlotItems.Count; i++) {
            afflictionsSkillSlotItems.extraSkillSlotItems[i].ClearData();
        }
        for (int i = 0; i < minionsSkillSlotItems.extraSkillSlotItems.Count; i++) {
            minionsSkillSlotItems.extraSkillSlotItems[i].ClearData();
        }
        for (int i = 0; i < structuresSkillSlotItems.extraSkillSlotItems.Count; i++) {
            structuresSkillSlotItems.extraSkillSlotItems[i].ClearData();
        }
        for (int i = 0; i < miscsSkillSlotItems.extraSkillSlotItems.Count; i++) {
            miscsSkillSlotItems.extraSkillSlotItems[i].ClearData();
        }
    }
    public void SetMoreLoadoutOptions(bool state, bool doEffect) {
        if(moreLoadoutOptions != state) {
            moreLoadoutOptions = state;
            if (doEffect) {
                if (!moreLoadoutOptions) {
                    ClearExtraSlotItems();
                }
            }
        }
    }
    #endregion

}

public class GroupedSkillSlotItems {
    public List<SkillSlotItem> fixedSkillSlotItems { get; private set; }
    public List<SkillSlotItem> extraSkillSlotItems { get; private set; }

    public GroupedSkillSlotItems() {
        fixedSkillSlotItems = new List<SkillSlotItem>();
        extraSkillSlotItems = new List<SkillSlotItem>();
    }

    public void AddFixedSkillSlotItem(SkillSlotItem skillSlotItem) {
        fixedSkillSlotItems.Add(skillSlotItem);
    }
    public bool RemoveFixedSkillSlotItem(SkillSlotItem skillSlotItem) {
        return fixedSkillSlotItems.Remove(skillSlotItem);
    }
    public void ClearFixedSkillSlotItem() {
        fixedSkillSlotItems.Clear();
    }

    public void AddExtraSkillSlotItem(SkillSlotItem skillSlotItem) {
        extraSkillSlotItems.Add(skillSlotItem);
    }
    public bool RemoveExtraSkillSlotItem(SkillSlotItem skillSlotItem) {
        return extraSkillSlotItems.Remove(skillSlotItem);
    }
    public void ClearExtraSkillSlotItem() {
        extraSkillSlotItems.Clear();
    }
    public bool HasExtraSkill(PLAYER_SKILL_TYPE skillType) {
        for (int i = 0; i < extraSkillSlotItems.Count; i++) {
            if(extraSkillSlotItems[i].skillData != null && extraSkillSlotItems[i].skillData.skill == skillType) {
                return true;
            }
        }
        return false;
    }
}
