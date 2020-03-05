using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;
using System.Linq;
using Traits;

public class TheProfaneUI : MonoBehaviour {
    //private TheProfane profane { get; set; }
    private Character chosenCultist;

    public void OnClickCorrupt(BaseLandmark landmark) {
        //profane = landmark as TheProfane;
        DualObjectPickerTabSetting[] tabs = new DualObjectPickerTabSetting[] {
            //convert
            new DualObjectPickerTabSetting() {
                name = "Convert",
                onToggleTabAction = OnClickConvert
            },
            //instruct
            new DualObjectPickerTabSetting() {
                name = "Instruct",
                onToggleTabAction = OnClickInstruct
            }
        };
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(tabs);
    }


    private void OnClickConvert(bool isOn) {
        if (isOn) {
            List<Character> convertibleCharacters = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => !x.returnedToLife && !x.isDead && x.traitContainer.HasTrait("Evil", "Treacherous") && !x.traitContainer.HasTrait("Blessed") && !x.traitContainer.HasTrait("Cultist"))); //"Disillusioned", 
            UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character, Character>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), convertibleCharacters,
                "Choose a Minion", "Choose a character to turn to Cultist",
                null, CanBeConvertedToCultist,
                null, ShowConvertCharacterTooltip,
                null, (character) => UIManager.Instance.HideSmallInfo(),
                OnConfirmConvert, "Convert");
        }
        
    }
    private void ShowConvertCharacterTooltip(Character character) {
        // int manaCost = profane.GetConvertToCultistCost(character);
        // if (CanBeConvertedToCultist(character)) {
        //     UIManager.Instance.ShowSmallInfo("Conversion Cost: " + manaCost.ToString() + " mana");
        // } else {
        //     string message = "Cannot convert character: ";
        //     if (PlayerManager.Instance.player.mana < manaCost) {
        //         message += "\n\t- Insufficient Mana. Cost is " + manaCost.ToString();
        //     }
        //     if (character.faction.leader != null && character.traitContainer.HasTrait("Treacherous")) {
        //         Character factionLeader = character.faction.leader as Character;
        //         if (!character.relationshipContainer.IsEnemiesWith(factionLeader)) {
        //             message += "\n\t- Treacherous characters must be enemies with their faction leader to be converted to a cultist.";
        //         }
        //         
        //     }
        //     UIManager.Instance.ShowSmallInfo(message);
        // }
    }
    private void OnConfirmConvert(object minionObj, object convertCharacter) {
        Minion minion = (minionObj as Character).minion;
        Character target = convertCharacter as Character;
        OnChooseCharacterToConvert(target);
    }
    private void OnClickInstruct(bool isOn) {
        if (isOn) {
            List<Character> cultists = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => !x.returnedToLife && !x.isDead && x.traitContainer.HasTrait("Cultist") && x.minion == null));
            UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character>(cultists, "Choose cultist to instruct", CanDoActionsToCharacter, null, null, OnChooseCultistToInstruct, OnConfirmInstruct, "Instruct");
        }
    }
    private void OnConfirmInstruct(object cultistObj, object instructObj) {
        Character chosenCultist = cultistObj as Character;
        string chosenAction = instructObj as string;

        // if (profane.isInCooldown) {
        //     PlayerUI.Instance.ShowGeneralConfirmation("In Cooldown", "The profane is currently on cooldown. Action will not proceed.");
        // } else {
        //     string message = "Are you sure you want to ";
        //     if (chosenAction == "Corrupt") {
        //         message += chosenAction + " " + chosenCultist.name + "?";
        //     } else if (chosenAction == "Sabotage Faction Quest") {
        //         message += "instruct " + chosenCultist.name + " to sabotage " + UtilityScripts.Utilities.GetPronounString(chosenCultist.gender, PRONOUN_TYPE.POSSESSIVE, false) + " factions quest?";
        //     } else if (chosenAction == "Destroy Supply" || chosenAction == "Destroy Food") {
        //         message += "instruct " + chosenCultist.name + " to " + chosenAction + "?";
        //     }
        //     //show confirmation.
        //     UIManager.Instance.ShowYesNoConfirmation("Confirm Action", message, onClickYesAction: () => profane.DoAction(chosenCultist, chosenAction), showCover: true, layer: 25);
        // }

    }
    private void OnChooseCultistToInstruct(object cultistObj) {
        Character cultist = cultistObj as Character;
        chosenCultist = cultist;
        List<string> actions = GetPossibleActionsForCharacter(cultist);

        UIManager.Instance.dualObjectPicker.PopulateColumn(actions, null, ShowActionTooltip, HideActionTooltip, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Ability");
    }
    private void OnChooseCharacterToConvert(Character character) {
        //string chosenAction = "Convert to cultist";
        // if (profane.isInCooldown) {
        //     PlayerUI.Instance.ShowGeneralConfirmation("In Cooldown", "The profane is currently on cooldown. Action will not proceed.");
        // } else {
        //     //show confirmation.
        //     UIManager.Instance.ShowYesNoConfirmation("Confirm Action", "Are you sure you want to convert " + character.name + " into a cultist? ", onClickYesAction: () => profane.DoAction(character, chosenAction), showCover: true, layer: 25);
        // }
    }
    private bool CanBeConvertedToCultist(Character character) {
        // int manaCost = profane.GetConvertToCultistCost(character);
        // if (PlayerManager.Instance.player.mana < manaCost) {
        //     return false;
        // }
        // if (character.traitContainer.HasTrait("Evil")) {
        //     return true;
        // } else if (character.traitContainer.HasTrait("Disillusioned")) {
        //     return true;
        // } else if (character.faction.leader != null && character.traitContainer.HasTrait("Treacherous")) {
        //     Character factionLeader = character.faction.leader as Character;
        //     return character.relationshipContainer.IsEnemiesWith(factionLeader);
        // }
        return false;
    }
    private bool CanDoActionsToCharacter(Character character) {
        return !character.currentParty.icon.isTravellingOutside && character.isAtHomeRegion;
    }
    private void ShowActionTooltip(string action) {
        // if (action == "Corrupt") {
        //     UIManager.Instance.ShowSmallInfo("Turn this character into a minion. This character will become a demon of " + (chosenCultist.traitContainer.GetNormalTrait<Cultist>("Cultist")).minionData.className);
        // }
    }
    private void HideActionTooltip(string action) {
        UIManager.Instance.HideSmallInfo();
    }
    private List<string> GetPossibleActionsForCharacter(Character character) {
        List<string> actions = new List<string>();
        if (character.minion == null) {
            actions.Add("Corrupt");
        }
        if (character.homeSettlement != null && character.homeRegion.IsFactionHere(character.faction) && character.faction.activeQuest is DivineInterventionQuest &&
            !(character.faction.activeQuest as DivineInterventionQuest).HasJob(JOB_TYPE.CORRUPT_CULTIST_SABOTAGE_FACTION)) {
            //only allow creation of sabotage faction quest if there is no job of that type yet.
            actions.Add("Sabotage Faction Quest");
        }
        if (character.homeSettlement != null) {
            actions.Add("Destroy Supply");
            actions.Add("Destroy Food");
        }
        return actions;
    }
}
