using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ruinarch.Custom_UI;
using TMPro;

public class SkillSlotItem : MonoBehaviour {
    public RuinarchButton button;
    public Image icon;
    public Image fixedIcon;
    public TextMeshProUGUI title;

    public PlayerSkillData skillData { get; private set; }

    private Action<PlayerSkillData> onHoverEnter;
    private Action<PlayerSkillData> onHoverExit;
    private bool isFixed;

    public void SetSkillSlotItem(PlayerSkillData skillData, bool isFixed) {
        this.skillData = skillData;
        this.isFixed = isFixed;
        UpdateSkillSlotItem();
    }
    public void SetSkillSlotItem(SPELL_TYPE skillType, bool isFixed) {
        SetSkillSlotItem(PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType), isFixed);
    }
    public void SetOnHoverEnterAction(Action<PlayerSkillData> onHoverEnter) {
        this.onHoverEnter = onHoverEnter;
    }
    public void SetOnHoverExitAction(Action<PlayerSkillData> onHoverExit) {
        this.onHoverExit = onHoverExit;
    }
    public void SetInteractable(bool state) {
        button.interactable = state;
        UpdateText();
    }

    private void UpdateSkillSlotItem() {
        UpdateIcon();
        UpdateText();
        UpdateFixedIcon();
    }
    private void UpdateIcon() {
        if(skillData != null) {
            icon.sprite = skillData.buttonSprite;
            icon.gameObject.SetActive(true);
        } else {
            icon.gameObject.SetActive(false);
        }
    }
    private void UpdateFixedIcon() {
        fixedIcon.gameObject.SetActive(isFixed);
    }
    private void UpdateText() {
        if(skillData != null) {
            title.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(skillData.skill.ToString());
        } else {
            if (button.IsInteractable()) {
                title.text = "Click to Assign";
            } else {
                title.text = "Unassigned";
            }
        }
    }
    public void OnClickThis() {
        if (isFixed) { return; }
        Messenger.Broadcast(Signals.SKILL_SLOT_ITEM_CLICKED, this);
    }
    public void OnHoverEnter() {
        onHoverEnter(skillData);
    }
    public void OnHoverExit() {
        onHoverExit(skillData);
    }
}
