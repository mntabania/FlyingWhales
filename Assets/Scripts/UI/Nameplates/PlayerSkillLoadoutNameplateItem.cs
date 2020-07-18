using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

//Use this nameplate item instead of enum nameplate item because enum nameplate item right now is not built to be reused, it has specific data intended for SPELL_TYPE in game scene only
//So if we want to use the SPELL_TYPE data in other things like for example in Player Skill Loadout UI, we should temporarily use this nameplate item
//This must be omitted in the future, so we must recode the enum nameplate item to accomodate other SPELL_TYPE data not being used in the Game Scene only
public class PlayerSkillLoadoutNameplateItem : MonoBehaviour {

    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI mainLbl;
    [SerializeField] private Toggle toggle;

    private Action<PlayerSkillData, bool> onToggleNameplate;
    private Action<PlayerSkillData> onHoverEnter;
    private Action<PlayerSkillData> onHoverExit;

    private PlayerSkillData skillData;

    public void SetObject(SPELL_TYPE o) {
        skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(o);
        string gameObjectName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(o.ToString());
        name = gameObjectName;
        mainLbl.text = gameObjectName;
        SetPortrait(skillData.buttonSprite);
    }
    public void SetToggleAction(Action<PlayerSkillData, bool> onToggleNameplate) {
        this.onToggleNameplate = onToggleNameplate;
    }
    public void SetOnHoverEnterAction(Action<PlayerSkillData> onHoverEnter) {
        this.onHoverEnter = onHoverEnter;
    }
    public void SetOnHoverExitAction(Action<PlayerSkillData> onHoverExit) {
        this.onHoverExit = onHoverExit;
    }
    public void SetPortrait(Sprite sprite) {
        portrait.sprite = sprite;
        portrait.gameObject.SetActive(portrait.sprite != null);
    }
    public void SetToggleGroup(ToggleGroup group) {
        toggle.group = group;
    }
    public void OnToggle(bool isOn) {
        onToggleNameplate(skillData, isOn);
    }
    public void OnHoverEnter() {
        onHoverEnter(skillData);
    }
    public void OnHoverExit() {
        onHoverExit(skillData);
    }
    public void SetInteractableState(bool state) {
        toggle.interactable = state;
    }
}
