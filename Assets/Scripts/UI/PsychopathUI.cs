using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Traits;
using TMPro;

public class PsychopathUI : PopupMenuBase {
    public PsychopathRequirementsUI requirements1UI;
    //public PsychopathRequirementsUI requirements2UI;
    public Button okButton;
    public TextMeshProUGUI reqDescriptionsLabel;

    public Character character { get; private set; }
    private List<PsychopathReq> requirements;

    public void ShowPsychopathUI(Character character) {
        if(requirements == null) {
            requirements = new List<PsychopathReq>();
        } else {
            requirements.Clear();
        }
        this.character = character;
        requirements1UI.ShowRequirementsUI();
        //requirements2UI.ShowRequirementsUI();
        reqDescriptionsLabel.text = "None";
        base.Open();
    }
    public void HidePsychopathUI() {
        character = null;
        base.Close();
    }

    public void OnClickOK() {
        if (requirements.Count == 0) {
            PlayerUI.Instance.ShowGeneralConfirmation("Error", "Must have at least 1 requirement.");
            return;
        }
        Psychopath serialKillerTrait = new Psychopath();
        character.traitContainer.AddTrait(character, serialKillerTrait);
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Psychopath", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        // PlayerManager.Instance.player.ShowNotificationFrom(log);

       
        //if (requirements2UI.victimType == SERIAL_VICTIM_TYPE.GENDER || requirements2UI.victimType == SERIAL_VICTIM_TYPE.RACE) {
        //    requirements2UI.victimDescriptions.Add(requirements2UI.reqDescriptionDropdown.options[requirements2UI.reqDescriptionDropdown.value].text);
        //}
        if(requirements.Count == 2) {
            serialKillerTrait.SetVictimRequirements(requirements[0].victimType, requirements[0].victimDescriptions, requirements[1].victimType, requirements[1].victimDescriptions);
        } else {
            serialKillerTrait.SetVictimRequirements(requirements[0].victimType, requirements[0].victimDescriptions, SERIAL_VICTIM_TYPE.NONE, new List<string>());

        }
        HidePsychopathUI();
        PlayerManager.Instance.GetAfflictionData(SPELL_TYPE.PSYCHOPATHY).OnExecuteSpellActionAffliction();
    }
    public void OnClickAdd() {
        if(requirements.Count >= 2) {
            PlayerUI.Instance.ShowGeneralConfirmation("Error", "Cannot have more than 2 requirements.");
            return;
        }
        if (requirements1UI.victimType == SERIAL_VICTIM_TYPE.NONE) {
            PlayerUI.Instance.ShowGeneralConfirmation("Error", "Must have at least 1 requirement.");
            return;
        }
        for (int i = 0; i < requirements.Count; i++) {
            if(requirements[i].victimType == requirements1UI.victimType) {
                PlayerUI.Instance.ShowGeneralConfirmation("Error", "Cannot have the same requirements.");
                return;
            }
        }
        if (requirements1UI.victimType == SERIAL_VICTIM_TYPE.CLASS || requirements1UI.victimType == SERIAL_VICTIM_TYPE.TRAIT) {
            if (requirements1UI.victimDescriptions == null || requirements1UI.victimDescriptions.Count <= 0) {
                PlayerUI.Instance.ShowGeneralConfirmation("Error", "Requirements are lacking. Please check again.");
                return;
            }
        }
        if (requirements1UI.victimType == SERIAL_VICTIM_TYPE.GENDER || requirements1UI.victimType == SERIAL_VICTIM_TYPE.RACE) {
            requirements1UI.victimDescriptions.Add(requirements1UI.reqDescriptionDropdown.options[requirements1UI.reqDescriptionDropdown.value].text);
        }
        requirements.Add(new PsychopathReq(requirements1UI.victimType, requirements1UI.victimDescriptions, requirements1UI.reqDescriptionsLabel.text));
        UpdateRequirementsLabel();
    }
    public void OnClickRemove() {
        if(requirements.Count > 0) {
            requirements.RemoveAt(requirements.Count - 1);
            UpdateRequirementsLabel();
        }
    }
    private void UpdateRequirementsLabel() {
        string req = string.Empty;
        for (int i = 0; i < requirements.Count; i++) {
            PsychopathReq currReq = requirements[i];
            if(i > 0) {
                req += "\n";
            }
            req += currReq.text;
            //string desc = string.Empty;
            //if (currReq.victimDescriptions.Count > 0) {
            //    for (int j = 0; j < currReq.victimDescriptions.Count; j++) {
            //        if (j > 0) {
            //            desc += ", ";
            //        }
            //        desc += currReq.victimDescriptions[j];
            //    }
            //} else {
            //    desc = "None";
            //}
            //req += currReq.victimType.ToString() + ": " + desc;
        }
        reqDescriptionsLabel.text = req;
    }
}
