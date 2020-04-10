using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResearchAbilityUI : PopupMenuBase {
    [Header("General")]
    public Button okBtn;
    public ToggleGroup toggleGroup;

    [Header("Ability 1")]
    public Toggle ability1Toggle;
    public Image ability1Icon;
    public TextMeshProUGUI ability1Text;
    //private INTERVENTION_ABILITY ability1;

    [Header("Ability 2")]
    public Toggle ability2Toggle;
    public Image ability2Icon;
    public TextMeshProUGUI ability2Text;
    //private INTERVENTION_ABILITY ability2;

    [Header("Ability 3")]
    public Toggle ability3Toggle;
    public Image ability3Icon;
    public TextMeshProUGUI ability3Text;
    //private INTERVENTION_ABILITY ability3;

    //private INTERVENTION_ABILITY chosenAbility;

    public void ShowResearchUI() {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowResearchUI());
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        ability1Toggle.isOn = false;
        ability2Toggle.isOn = false;
        ability3Toggle.isOn = false;
        okBtn.interactable = false;

        base.Open();
    }
    public void SetAbility1(SPELL_TYPE ability) {
        //ability1 = ability;
        string name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString());
        ability1Icon.sprite = PlayerManager.Instance.GetJobActionSprite(name);
        string text = name;
        text += $"\n{PlayerSkillManager.Instance.allSpellsData[ability].description}";
        //text += "\nTier: " + PlayerManager.Instance.GetInterventionAbilityTier(ability);
        ability1Text.text = text;
    }
    public void SetAbility2(SPELL_TYPE ability) {
        //ability2 = ability;
        string name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString());
        ability2Icon.sprite = PlayerManager.Instance.GetJobActionSprite(name);
        string text = name;
        text += $"\n{PlayerSkillManager.Instance.allSpellsData[ability].description}";
        //text += "\nTier: " + PlayerManager.Instance.GetInterventionAbilityTier(ability);
        ability2Text.text = text;
    }
    public void SetAbility3(SPELL_TYPE ability) {
        //ability3 = ability;
        string name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString());
        ability3Icon.sprite = PlayerManager.Instance.GetJobActionSprite(name);
        string text = name;
        text += $"\n{PlayerSkillManager.Instance.allSpellsData[ability].description}";
        //text += "\nTier: " + PlayerManager.Instance.GetInterventionAbilityTier(ability);
        ability3Text.text = text;
    }

    public void OnClickAbility1(bool state) {
        if (!state) { return; }
        //chosenAbility = ability1;
        okBtn.interactable = true;
    }
    public void OnClickAbility2(bool state) {
        if (!state) { return; }
        //chosenAbility = ability2;
        okBtn.interactable = true;
    }
    public void OnClickAbility3(bool state) {
        if (!state) { return; }
        //chosenAbility = ability3;
        okBtn.interactable = true;
    }
    public override void Close() {
        base.Close();
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            //if (PlayerManager.Instance.player.isNotFirstResearch) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
            //}
        }
        //PlayerManager.Instance.player.NewCycleForNewInterventionAbility(chosenAbility);
    }
}
