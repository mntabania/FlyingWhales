using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ruinarch.Custom_UI;
using TMPro;

public class SkillSlotItem : MonoBehaviour {
    public RuinarchButton skillSlotItemButton;
    public RuinarchButton minusButton;
    public Image buttonImage;
    public Image icon;
    public Image fixedIcon;

    public Sprite optionalButtonDefault;
    public Sprite optionalButtonHighlighted;
    public Sprite optionalButtonPressed;

    public Sprite defaultButtonDefault;
    public Sprite defaultButtonHighlighted;
    public Sprite defaultButtonPressed;
    public Sprite defaultButtonDisabled;

    public TextMeshProUGUI spellText;

    public PlayerSkillData skillData { get; private set; }

    private Action<PlayerSkillData> onHoverEnter;
    private Action<PlayerSkillData> onHoverExit;
    private bool isFixed;
    private PLAYER_ARCHETYPE archetype;

    public void SetSkillSlotItem(PLAYER_ARCHETYPE archetype, PlayerSkillData skillData, bool isFixed) {
        this.skillData = skillData;
        this.isFixed = isFixed;
        this.archetype = archetype;
        UpdateSkillSlotItem();
    }
    public void SetSkillSlotItem(PLAYER_ARCHETYPE archetype, PLAYER_SKILL_TYPE skillType, bool isFixed) {
        SetSkillSlotItem(archetype, PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(skillType), isFixed);
    }
    public void SetOnHoverEnterAction(Action<PlayerSkillData> onHoverEnter) {
        this.onHoverEnter = onHoverEnter;
    }
    public void SetOnHoverExitAction(Action<PlayerSkillData> onHoverExit) {
        this.onHoverExit = onHoverExit;
    }
    private void UpdateSkillSlotItem() {
        UpdateButtonSprites();
        UpdateMinusButton();
        UpdateIcon();
        UpdateText();
        UpdateFixedIcon();
    }
    private void UpdateButtonSprites() {
        SpriteState newSpriteState = new SpriteState();
        if (!isFixed && skillData == null) {
            newSpriteState.highlightedSprite = optionalButtonHighlighted;
            newSpriteState.pressedSprite = optionalButtonPressed;
            newSpriteState.selectedSprite = optionalButtonPressed;
            newSpriteState.disabledSprite = optionalButtonDefault;
            buttonImage.sprite = optionalButtonDefault;
        } else {
            newSpriteState.highlightedSprite = defaultButtonHighlighted;
            newSpriteState.pressedSprite = defaultButtonPressed;
            newSpriteState.selectedSprite = defaultButtonPressed;
            newSpriteState.disabledSprite = defaultButtonDefault;
            buttonImage.sprite = defaultButtonDefault;
        }
        skillSlotItemButton.spriteState = newSpriteState;
    }
    private void UpdateMinusButton() {
        minusButton.gameObject.SetActive(!isFixed && skillData != null);
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
    public void ClearData() {
        SetSkillSlotItem(archetype, null, isFixed);
    }
    private void UpdateText() {
        if(skillData != null) {
            SkillData playerSkillData = PlayerSkillManager.Instance.GetSkillData(skillData.skill);
            if (playerSkillData == null) {
                Debug.LogError(skillData.skill.ToString() + " skill data is null!");
            }
            spellText.text = playerSkillData.localizedName;
        } else {
            if (!isFixed) {
                spellText.text = "Click to Assign a Skill";
            } else {
                spellText.text = "Unassigned";
            }
        }
    }
    public void OnClickThis() {
        if (isFixed) { return; }
        Messenger.Broadcast(UISignals.SKILL_SLOT_ITEM_CLICKED, this, archetype);
    }
    public void OnClickMinus() {
        if (isFixed) { return; }
        ClearData();
    }
    public void OnHoverEnter() {
        onHoverEnter(skillData);
    }
    public void OnHoverExit() {
        onHoverExit(skillData);
    }
}
