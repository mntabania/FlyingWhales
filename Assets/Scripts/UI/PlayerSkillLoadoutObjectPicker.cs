using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayerSkillLoadoutObjectPicker : MonoBehaviour {

    [Header("Object Picker")]
    [SerializeField] private ScrollRect objectPickerScrollView;
    [SerializeField] private GameObject playerskillLoadoutItemPrefab;
    [SerializeField] private GameObject cover;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private ToggleGroup toggleGroup;
    public UIHoverPosition hoverPos;

    private System.Action<PlayerSkillData> onConfirmAction;
    private PlayerSkillData pickedSkill;

    public void ShowLoadoutPicker(PLAYER_SKILL_TYPE[] items, Action<PlayerSkillData> onConfirmAction, Action<PlayerSkillData> onHoverEnterAction, Action<PlayerSkillData> onHoverExitAction) {
        UtilityScripts.Utilities.DestroyChildren(objectPickerScrollView.content);
        this.onConfirmAction = onConfirmAction;
        for (int i = 0; i < items.Length; i++) {
            PLAYER_SKILL_TYPE skillType = items[i];
            if (!PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                //If there is no data yet, the skill must not be part of the choices
                continue;
            }
            if (WorldSettings.Instance.worldSettingsData.disabledSpells.Contains(skillType)) {
                //if picked world settings has this spell disabled then do not show it
                continue;
            }
            GameObject go = Instantiate(playerskillLoadoutItemPrefab, objectPickerScrollView.content);
            PlayerSkillLoadoutNameplateItem item = go.GetComponent<PlayerSkillLoadoutNameplateItem>();
            item.SetObject(skillType);
            item.SetToggleGroup(toggleGroup);
            item.SetToggleAction(OnPickSkill);
            item.SetOnHoverEnterAction(onHoverEnterAction);
            item.SetOnHoverExitAction(onHoverExitAction);
        }
        UpdateConfirmBtnState();
        Open();
    }
    public void ShowLoadoutPicker(List<PLAYER_SKILL_TYPE> items, Action<PlayerSkillData> onConfirmAction, Action<PlayerSkillData> onHoverEnterAction, Action<PlayerSkillData> onHoverExitAction) {
        UtilityScripts.Utilities.DestroyChildren(objectPickerScrollView.content);
        this.onConfirmAction = onConfirmAction;
        for (int i = 0; i < items.Count; i++) {
            PLAYER_SKILL_TYPE skillType = items[i];
            if (!PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(skillType)) {
                //If there is no data yet, the skill must not be part of the choices
                continue;
            }
            if (WorldSettings.Instance.worldSettingsData.disabledSpells.Contains(skillType)) {
                //if picked world settings has this spell disabled then do not show it
                continue;
            }
            GameObject go = Instantiate(playerskillLoadoutItemPrefab, objectPickerScrollView.content);
            PlayerSkillLoadoutNameplateItem item = go.GetComponent<PlayerSkillLoadoutNameplateItem>();
            item.SetObject(skillType);
            item.SetToggleGroup(toggleGroup);
            item.SetToggleAction(OnPickSkill);
            item.SetOnHoverEnterAction(onHoverEnterAction);
            item.SetOnHoverExitAction(onHoverExitAction);
            if (skillType == PLAYER_SKILL_TYPE.OSTRACIZER) {
                //disable meddler and ostracizer
                item.SetInteractableState(false);
                item.transform.SetAsLastSibling();
            } else {
                item.SetInteractableState(true);
                item.transform.SetAsFirstSibling();
            }
        }
        UpdateConfirmBtnState();
        Open();
    }
    private void OnPickSkill(PlayerSkillData skillData, bool isOn) {
        if (isOn) {
            pickedSkill = skillData;
            OnClickConfirm();
        }
        //else {
        //    if (pickedSkill == skillData) {
        //        pickedSkill = null;
        //    }
        //}
        //UpdateConfirmBtnState();
    }
    private void UpdateConfirmBtnState() {
        confirmBtn.interactable = pickedSkill != null;
    }
    public void Open() {
        gameObject.SetActive(true);
    }
    public void Close() {
        gameObject.SetActive(false);
    }
    public void OnClickConfirm() {
        onConfirmAction(pickedSkill);
        Close();
    }
}