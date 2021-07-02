using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Traits;
using TMPro;
using Ruinarch.Custom_UI;

public class PsychopathUI : MonoBehaviour {
    public PsychopathPicker psychopathPicker;
    public RuinarchButton confirmButton;

    public GameObject requirement1GO;
    public GameObject requirement2GO;
    public GameObject conjunctionGO;
    public GameObject andGO;

    public RuinarchButton victimType1Button;
    public RuinarchButton victimType2Button;
    public RuinarchButton victimDescription1Button;
    public RuinarchButton victimDescription2Button;
    public RuinarchButton clearButtonVictim1;
    public RuinarchButton clearButtonVictim2;

    public TextMeshProUGUI victimType1Text;
    public TextMeshProUGUI victimType2Text;
    public TextMeshProUGUI victimDescription1Text;
    public TextMeshProUGUI victimDescription2Text;
    public TextMeshProUGUI conjunctionText;

    private SERIAL_VICTIM_TYPE victimType1;
    private SERIAL_VICTIM_TYPE victimType2;

    private string victimDescription1;
    private string victimDescription2;

    private string conjunction;

    private int psychopathyAfflictionLevel;

    private List<string> serialVictimType1AsStrings = new List<string>();
    private List<string> serialVictimType2AsStrings = new List<string>();
    private List<string> criteriaGenders = new List<string>() { "Male", "Female" };
    private List<string> criteriaRaces = new List<string>() { "Humans", "Elves" };
    private List<string> criteriaClasses = new List<string>() { "Farmer", "Miner", "Crafter", "Archer", "Stalker", "Hunter", "Druid", "Shaman"
        , "Mage", "Knight", "Barbarian", "Marauder", "Noble" };
    private List<string> criteriaTraits = new List<string>() { 
        "Accident Prone", "Agoraphobic", "Alcoholic", "Ambitious", "Authoritative"
        , "Blessed"
        , "Cannibal", "Chaste", "Coward", "Diplomatic", "Evil"
        , "Fast", "Fire Resistant", "Glutton", "Hothead", "Inspiring", "Kleptomaniac"
        , "Lazy", "Lustful", "Lycanthrope", "Music Hater", "Music Lover", "Narcoleptic"
        , "Nocturnal", "Optimist", "Pessimist", "Psychopath", "Purifier", "Pyrophobic"
        , "Robust", "Suspicious", "Treacherous", "Unattractive", "Unfaithful", "Vampire", "Vigilant"};
    private List<string> criteriaConjunctions = new List<string>() { "And", "Or" };

    public Character character { get; private set; }

    public void ShowPsychopathUI(Character character) {
        psychopathyAfflictionLevel = PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PSYCHOPATHY).currentLevel;
        this.character = character;
        UpdateUIBasedOnPsychopathyAfflictionLevel();
        ClearVictim1();
        ClearVictim2();
        ClearConjunction();
        gameObject.SetActive(true);
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
    }
    public void HidePsychopathUI() {
        character = null;
        gameObject.SetActive(false);
        if (!PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown and object picker is not open, unpause game
        }
    }
    private void UpdateUIBasedOnPsychopathyAfflictionLevel() {
        requirement1GO.SetActive(true);
        requirement2GO.SetActive(psychopathyAfflictionLevel >= 2);
        conjunctionGO.SetActive(psychopathyAfflictionLevel >= 3);
        andGO.SetActive(!conjunctionGO.activeSelf);
    }
    private void UpdateConfirmButtonState() {
        confirmButton.interactable = (victimType1 != SERIAL_VICTIM_TYPE.None && victimDescription1 != string.Empty) || (victimType2 != SERIAL_VICTIM_TYPE.None && victimDescription2 != string.Empty);
    }
    private void SetVictimType1(SERIAL_VICTIM_TYPE type) {
        victimType1 = type;
        if(type != SERIAL_VICTIM_TYPE.None) {
            victimType1Text.text = type.ToString();
            clearButtonVictim1.gameObject.SetActive(true);
        } else {
            victimType1Text.text = "<i>Type</i>";
            clearButtonVictim1.gameObject.SetActive(false);
        }
        SetVictim1Description(string.Empty);
        UpdateVictimDescription1ButtonState();
        UpdateConfirmButtonState();
    }
    private void SetVictimType2(SERIAL_VICTIM_TYPE type) {
        victimType2 = type;
        if (type != SERIAL_VICTIM_TYPE.None) {
            victimType2Text.text = type.ToString();
            clearButtonVictim2.gameObject.SetActive(true);
        } else {
            victimType2Text.text = "<i>Type</i>";
            clearButtonVictim2.gameObject.SetActive(false);
        }
        SetVictim2Description(string.Empty);
        UpdateVictimDescription2ButtonState();
        UpdateConfirmButtonState();
    }
    private void SetVictim1Description(string str) {
        victimDescription1 = str;
        victimDescription1Text.text = string.IsNullOrEmpty(str) ? "<i>Criteria</i>" : str;
        UpdateConfirmButtonState();
    }
    private void SetVictim2Description(string str) {
        victimDescription2 = str;
        victimDescription2Text.text = string.IsNullOrEmpty(str) ? "<i>Criteria</i>" : str;
        UpdateConfirmButtonState();
    }
    private void SetConjunction(string p_str) {
        conjunction = p_str;
        conjunctionText.text = conjunction;
    }
    private void UpdateVictimDescription1ButtonState() {
        victimDescription1Button.interactable = victimType1 != SERIAL_VICTIM_TYPE.None;
    }
    private void UpdateVictimDescription2ButtonState() {
        victimDescription2Button.interactable = victimType2 != SERIAL_VICTIM_TYPE.None;
    }
    private List<string> GetListOfVictimDescriptionsBasedOnType(SERIAL_VICTIM_TYPE type) {
        if(type == SERIAL_VICTIM_TYPE.Gender) {
            return criteriaGenders;
        } else if (type == SERIAL_VICTIM_TYPE.Class) {
            return criteriaClasses;
        } else if (type == SERIAL_VICTIM_TYPE.Race) {
            return criteriaRaces;
        } else if (type == SERIAL_VICTIM_TYPE.Trait) {
            return criteriaTraits;
        }
        return null;
    }
    private void UpdateSerialVictimTypesAsStrings(List<string> list) {
        list.Clear();
        if (psychopathyAfflictionLevel == 0) {
            list.Add("Trait");
        } else if (psychopathyAfflictionLevel == 1) {
            list.Add("Trait");
            list.Add("Class");
        } else if (psychopathyAfflictionLevel == 2) {
            list.Add("Trait");
            list.Add("Class");
            list.Add("Gender");
        } else if (psychopathyAfflictionLevel == 3) {
            list.Add("Trait");
            list.Add("Class");
            list.Add("Gender");
            list.Add("Race");
        }
    }
    public void OnClickVictimType1() {
        UpdateSerialVictimTypesAsStrings(serialVictimType1AsStrings);
        serialVictimType1AsStrings.Remove(victimType2Text.text);
        psychopathPicker.ShowPicker(serialVictimType1AsStrings, OnConfirmVictimType1, null, null);
    }
    private void OnConfirmVictimType1(string str) {
        SERIAL_VICTIM_TYPE type = (SERIAL_VICTIM_TYPE) System.Enum.Parse(typeof(SERIAL_VICTIM_TYPE), str);
        SetVictimType1(type);
    }
    public void OnClickVictimType2() {
        UpdateSerialVictimTypesAsStrings(serialVictimType2AsStrings);
        serialVictimType2AsStrings.Remove(victimType1Text.text);
        psychopathPicker.ShowPicker(serialVictimType2AsStrings, OnConfirmVictimType2, null, null);
    }
    private void OnConfirmVictimType2(string str) {
        SERIAL_VICTIM_TYPE type = (SERIAL_VICTIM_TYPE) System.Enum.Parse(typeof(SERIAL_VICTIM_TYPE), str);
        SetVictimType2(type);
    }
    public void OnClickVictimDescription1() {
        List<string> descriptions = GetListOfVictimDescriptionsBasedOnType(victimType1);
        if(descriptions != null) {
            psychopathPicker.ShowPicker(descriptions, OnConfirmVictimDescription1, null, null);
        }
    }
    private void OnConfirmVictimDescription1(string str) {
        SetVictim1Description(str);
    }
    public void OnClickVictimDescription2() {
        List<string> descriptions = GetListOfVictimDescriptionsBasedOnType(victimType2);
        if (descriptions != null) {
            psychopathPicker.ShowPicker(descriptions, OnConfirmVictimDescription2, null, null);
        }
    }
    private void OnConfirmVictimDescription2(string str) {
        SetVictim2Description(str);
    }
    public void OnClickConjunction() {
        psychopathPicker.ShowPicker(criteriaConjunctions, OnConfirmConjunction, null, null);
    }
    private void OnConfirmConjunction(string str) {
        SetConjunction(str);
    }
    public void ClearVictim1() {
        SetVictimType1(SERIAL_VICTIM_TYPE.None);
    }
    public void ClearVictim2() {
        SetVictimType2(SERIAL_VICTIM_TYPE.None);
    }
    public void ClearConjunction() {
        SetConjunction(criteriaConjunctions[0]);
    }
    public void OnClickConfirm() {
        Psychopath psychopathTrait = TraitManager.Instance.CreateNewInstancedTraitClass<Psychopath>("Psychopath");
        character.traitContainer.AddTrait(character, psychopathTrait);
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "player_afflicted", null, LOG_TAG.Life_Changes);
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Psychopath", LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase(true);
        // PlayerManager.Instance.player.ShowNotificationFrom(log);
        if(victimType1 == SERIAL_VICTIM_TYPE.None) {
            victimDescription1 = string.Empty;
        }
        if (victimType2 == SERIAL_VICTIM_TYPE.None) {
            victimDescription2 = string.Empty;
        }
        if (victimDescription1 == string.Empty) {
            victimType1 = SERIAL_VICTIM_TYPE.None;
        }
        if (victimDescription2 == string.Empty) {
            victimType2 = SERIAL_VICTIM_TYPE.None;
        }
        psychopathTrait.SetVictimRequirements(victimType1, victimDescription1, victimType2, victimDescription2, conjunction);

        HidePsychopathUI();
        PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.PSYCHOPATHY).OnExecutePlayerSkill();
    }
}
