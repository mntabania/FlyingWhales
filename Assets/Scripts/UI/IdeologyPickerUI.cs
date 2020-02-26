using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IdeologyPickerUI : MonoBehaviour {
    public TMP_Dropdown dropdown;
    public Button editButton;
    public TextMeshProUGUI requirementsText;
    //public GameObject exclusiveIdeologyHolder;

    //Exclusive Requirements
    private EXCLUSIVE_IDEOLOGY_CATEGORIES exclusiveCategory;
    private string exclusiveRequirement;

    private int categoryIndex;

    public void Initialize(int categoryIndex) {
        this.categoryIndex = categoryIndex;
        PopulateIdeologyDropdown();
        ResetRequirementsText();
    }
    private void PopulateIdeologyDropdown() {
        dropdown.ClearOptions();
        FACTION_IDEOLOGY[] categorizedIdeologies = FactionManager.Instance.categorizedFactionIdeologies[categoryIndex];
        //string[] ideologies = System.Enum.GetNames(typeof(FACTION_IDEOLOGY));
        for (int i = 0; i < categorizedIdeologies.Length; i++) {
            dropdown.options.Add(new TMP_Dropdown.OptionData(System.Enum.GetName(typeof(FACTION_IDEOLOGY), categorizedIdeologies[i])));
        }
        dropdown.RefreshShownValue();
        dropdown.value = 0;
    }
    public void OnChangeIdeology(int index) {
        UIManager.Instance.regionInfoInfoUi.fingersUI.ShowAppropriateIdeologyContent(GetIdeologyDropdownValue(), categoryIndex);
    }
    public void OnClickEdit() {
        FACTION_IDEOLOGY currentIdeology = GetIdeologyDropdownValue();
        if(currentIdeology == FACTION_IDEOLOGY.EXCLUSIVE) {
            UIManager.Instance.regionInfoInfoUi.fingersUI.UpdateExclusiveIdeologyContent(exclusiveCategory, exclusiveRequirement, categoryIndex);
        }
    }
    private FACTION_IDEOLOGY GetIdeologyDropdownValue() {
        return (FACTION_IDEOLOGY) System.Enum.Parse(typeof(FACTION_IDEOLOGY), dropdown.options[dropdown.value].text);
    }
    private void ResetRequirementsText() {
        SetRequirementsText("None");
    }
    private void SetRequirementsText(string text) {
        requirementsText.text = text;
        UpdateEditButton();
    }
    private void UpdateEditButton() {
        if(requirementsText.text == "None") {
            editButton.gameObject.SetActive(false);
        } else {
            editButton.gameObject.SetActive(true);
        }
    }
    //public bool IsComplete() {
    //    if(GetIdeologyDropdownValue() == FACTION_IDEOLOGY.EXCLUSIVE) {
    //        if (exclusiveCategory == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE && exclusiveRequirement == "NONE") { //This means that the race choice is NONE
    //            PlayerUI.Instance.ShowGeneralConfirmation("No Race Requirement Error!", "Please select a race for the requirement.");
    //            return false;
    //        }
    //    }
    //}

    #region Exclusive Ideology
    public void SetExclusiveRequirements(EXCLUSIVE_IDEOLOGY_CATEGORIES exclusiveCategory, string exclusiveRequirement) {
        this.exclusiveCategory = exclusiveCategory;
        this.exclusiveRequirement = exclusiveRequirement;
        SetRequirementsText(
            $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(exclusiveCategory.ToString())} - {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(exclusiveRequirement)}");
    }
    #endregion
}
