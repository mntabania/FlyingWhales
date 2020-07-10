using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Traits;
using TMPro;
using Ruinarch.Custom_UI;

public class PsychopathUI : PopupMenuBase {
    public PsychopathPicker psychopathPicker;
    public RuinarchButton confirmButton;

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

    private SERIAL_VICTIM_TYPE victimType1;
    private SERIAL_VICTIM_TYPE victimType2;

    private string victimDescription1;
    private string victimDescription2;

    private List<string> serialVictimTypeAsStrings = UtilityScripts.Utilities.GetEnumChoices<SERIAL_VICTIM_TYPE>();
    private List<string> criteriaGenders = new List<string>() { "Male", "Female" };
    private List<string> criteriaRaces = new List<string>() { "Humans", "Elves" };
    private List<string> criteriaClasses = new List<string>() { "Peasant", "Miner", "Craftsman", "Archer", "Stalker", "Hunter", "Druid", "Shaman"
        , "Mage", "Knight", "Barbarian", "Marauder", "Noble" };
    private List<string> criteriaTraits = new List<string>() { "Accident Prone", "Agoraphobic", "Alcoholic", "Ambitious", "Authoritative", "Cannibal"
        , "Chaste", "Coward", "Diplomatic", "Healing Expert", "Evil"
        , "Fast", "Fireproof", "Glutton", "Hothead", "Inspiring", "Kleptomaniac"
        , "Lazy", "Lustful", "Lycanthrope", "Music Hater", "Music Lover", "Narcoleptic"
        , "Nocturnal", "Optimist", "Pessimist", "Psychopath", "Purifier", "Pyrophobic"
        , "Robust", "Suspicious", "Treacherous", "Ugly", "Unfaithful", "Vampiric", "Vigilant" };

    public Character character { get; private set; }

    //private void Start() {
    //    serialVictimTypeAsStrings = UtilityScripts.Utilities.GetEnumChoices<SERIAL_VICTIM_TYPE>();

    //}

    public void ShowPsychopathUI(Character character) {
        this.character = character;
        SetVictimType1(SERIAL_VICTIM_TYPE.None);
        SetVictimType2(SERIAL_VICTIM_TYPE.None);
        base.Open();
    }
    public void HidePsychopathUI() {
        character = null;
        base.Close();
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
            victimType1Text.text = string.Empty;
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
            victimType2Text.text = string.Empty;
            clearButtonVictim2.gameObject.SetActive(false);
        }
        SetVictim2Description(string.Empty);
        UpdateVictimDescription2ButtonState();
        UpdateConfirmButtonState();
    }
    private void SetVictim1Description(string str) {
        victimDescription1 = str;
        victimDescription1Text.text = str;
        UpdateConfirmButtonState();
    }
    private void SetVictim2Description(string str) {
        victimDescription2 = str;
        victimDescription2Text.text = str;
        UpdateConfirmButtonState();
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

    public void OnClickVictimType1() {
        psychopathPicker.ShowPicker(serialVictimTypeAsStrings, OnConfirmVictimType1, null, null);
    }
    private void OnConfirmVictimType1(string str) {
        SERIAL_VICTIM_TYPE type = (SERIAL_VICTIM_TYPE) System.Enum.Parse(typeof(SERIAL_VICTIM_TYPE), str);
        SetVictimType1(type);
    }
    public void OnClickVictimType2() {
        psychopathPicker.ShowPicker(serialVictimTypeAsStrings, OnConfirmVictimType2, null, null);
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
    public void ClearVictim1() {
        SetVictimType1(SERIAL_VICTIM_TYPE.None);
    }
    public void ClearVictim2() {
        SetVictimType2(SERIAL_VICTIM_TYPE.None);
    }
    public void OnClickConfirm() {
        Psychopath psychopathTrait = new Psychopath();
        character.traitContainer.AddTrait(character, psychopathTrait);
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Psychopath", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
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
        psychopathTrait.SetVictimRequirements(victimType1, victimDescription1, victimType2, victimDescription2);

        HidePsychopathUI();
        PlayerSkillManager.Instance.GetAfflictionData(SPELL_TYPE.PSYCHOPATHY).OnExecuteSpellActionAffliction();
    }
}
