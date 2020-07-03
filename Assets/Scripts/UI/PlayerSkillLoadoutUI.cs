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
    private List<SPELL_TYPE> loadoutChoices;

    public void Initialize() {
        spellsSkillSlotItems = new GroupedSkillSlotItems();
        afflictionsSkillSlotItems = new GroupedSkillSlotItems();
        minionsSkillSlotItems = new GroupedSkillSlotItems();
        structuresSkillSlotItems = new GroupedSkillSlotItems();
        miscsSkillSlotItems = new GroupedSkillSlotItems();
        loadoutChoices = new List<SPELL_TYPE>();

        Messenger.AddListener<SkillSlotItem, PLAYER_ARCHETYPE>(Signals.SKILL_SLOT_ITEM_CLICKED, OnClickSkillSlotItem);
        Messenger.AddListener(Signals.START_GAME_AFTER_LOADOUT_SELECT, OnStartGameAfterLoadoutSelect);

        spellsTab.isOn = true;
    }
    public void OnDestroy() {
        Messenger.RemoveListener<SkillSlotItem, PLAYER_ARCHETYPE>(Signals.SKILL_SLOT_ITEM_CLICKED, OnClickSkillSlotItem);
        Messenger.RemoveListener(Signals.START_GAME_AFTER_LOADOUT_SELECT, OnStartGameAfterLoadoutSelect);
    }

    #region General
    private void OnStartGameAfterLoadoutSelect() {
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
    private void LoadSkillDataToUI(List<SPELL_TYPE> fixedSkills, int extraSlots, GroupedSkillSlotItems groupedSkillSlotItems, List<SPELL_TYPE> extraSkills, Transform parent) {
        if(fixedSkills != null) {
            for (int i = 0; i < fixedSkills.Count; i++) {
                SPELL_TYPE fixedSkill = fixedSkills[i];
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
                skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(extraSkills[i]);
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
        if (spellsTab.isOn) {
            for (int i = 0; i < PlayerSkillManager.Instance.allSpells.Length; i++) {
                SPELL_TYPE skillType = PlayerSkillManager.Instance.allSpells[i];
                if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                    if (!loadout.spells.fixedSkills.Contains(skillType) && !spellsSkillSlotItems.HasExtraSkill(skillType)) {
                        loadoutChoices.Add(skillType);
                    }
                }
            }
        } else if (afflictionsTab.isOn) {
            for (int i = 0; i < PlayerSkillManager.Instance.allAfflictions.Length; i++) {
                SPELL_TYPE skillType = PlayerSkillManager.Instance.allAfflictions[i];
                if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                    if (!loadout.afflictions.fixedSkills.Contains(skillType) && !afflictionsSkillSlotItems.HasExtraSkill(skillType)) {
                        loadoutChoices.Add(skillType);
                    }
                }
            }
        } else if (minionsTab.isOn) {
            for (int i = 0; i < PlayerSkillManager.Instance.allMinionPlayerSkills.Length; i++) {
                SPELL_TYPE skillType = PlayerSkillManager.Instance.allMinionPlayerSkills[i];
                if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                    if (!loadout.minions.fixedSkills.Contains(skillType) && !minionsSkillSlotItems.HasExtraSkill(skillType)) {
                        loadoutChoices.Add(skillType);
                    }
                }
            }
        } else if (structuresTab.isOn) {
            for (int i = 0; i < PlayerSkillManager.Instance.allDemonicStructureSkills.Length; i++) {
                SPELL_TYPE skillType = PlayerSkillManager.Instance.allDemonicStructureSkills[i];
                if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                    if (!loadout.structures.fixedSkills.Contains(skillType) && !structuresSkillSlotItems.HasExtraSkill(skillType)) {
                        loadoutChoices.Add(skillType);
                    }
                }
            }
        } else if (miscsTab.isOn) {
            for (int i = 0; i < PlayerSkillManager.Instance.allPlayerActions.Length; i++) {
                SPELL_TYPE skillType = PlayerSkillManager.Instance.allPlayerActions[i];
                if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                    if (!loadout.miscs.fixedSkills.Contains(skillType) && !miscsSkillSlotItems.HasExtraSkill(skillType)) {
                        loadoutChoices.Add(skillType);
                    }
                }
            }
        }
        if(loadoutChoices.Count > 0) {
            objectPicker.ShowLoadoutPicker(loadoutChoices, OnConfirmSkill, OnHoverEnterSkill, OnHoverExitSkill);
        }
    }
    private void OnConfirmSkill(PlayerSkillData skillData) {
        if(pickedSlotItem) {
            pickedSlotItem.SetSkillSlotItem(loadout.archetype, skillData, false);
        }
    }
    private void OnHoverEnterSkill(PlayerSkillData skillData) {
        skillDetailsTooltip.ShowPlayerSkillDetails(skillData, objectPicker.hoverPos);
    }
    private void OnHoverExitSkill(PlayerSkillData skillData) {
        skillDetailsTooltip.HidePlayerSkillDetails();
    }
    private void OnHoverEnterSkillSlotItem(PlayerSkillData skillData) {
        if(skillData != null) {
            skillDetailsTooltip.ShowPlayerSkillDetails(skillData);
        }
    }
    private void OnHoverExitSkillSlotItem(PlayerSkillData skillData) {
        skillDetailsTooltip.HidePlayerSkillDetails();
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
    public bool HasExtraSkill(SPELL_TYPE skillType) {
        for (int i = 0; i < extraSkillSlotItems.Count; i++) {
            if(extraSkillSlotItems[i].skillData != null && extraSkillSlotItems[i].skillData.skill == skillType) {
                return true;
            }
        }
        return false;
    }
}
