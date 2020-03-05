using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeUI : MonoBehaviour {
    
    //private TheAnvil theAnvil;

    #region General
    public void OnClickUpgrade(BaseLandmark landmark) {
        //theAnvil = landmark as TheAnvil;
        // UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character, string>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), LandmarkManager.Instance.anvilResearchData.Keys.ToList(),
        //     "Choose Minion", "Choose Upgrade",
        //     CanChooseMinion, CanChooseUpgrade,
        //     OnHoverEnterMinion, OnHoverAbilityChoice,
        //     OnHoverExitMinion, OnHoverExitAbilityChoice,
        //     OnConfirmUpgrade, "Upgrade");
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = $"{character.name} is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.RESEARCHER)) {
                message = $"{character.name} does not have the required trait: Researcher";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnConfirmUpgrade(object minionObj, object upgradeObj) {
        Minion minion = (minionObj as Character).minion;
        string upgrade = upgradeObj as string;

        //minion.SetAssignedRegion(theAnvil.tileLocation.region);
        //theAnvil.tileLocation.region.SetAssignedMinion(minion);
        // theAnvil.SetUpgradeIdentifier(upgrade);
        // theAnvil.StartUpgradeProcess();
    }
    #endregion

    #region Minion
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.RESEARCHER);
    }
    #endregion

    // #region Upgrade
    // private bool CanChooseUpgrade(string upgrade) {
    //     if (!theAnvil.dynamicResearchData[upgrade].isResearched && PlayerManager.Instance.player.mana >= LandmarkManager.Instance.anvilResearchData[upgrade].manaCost) {
    //         if (LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch == string.Empty) {
    //             return true;
    //         } else {
    //             return theAnvil.dynamicResearchData[LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch].isResearched;
    //         }
    //     }
    //     return false;
    // }
    // private void OnHoverAbilityChoice(string abilityName) {
    //     string info = string.Empty;
    //     if (CanChooseUpgrade(abilityName)) {
    //         info = TheAnvil.GetUpgradeDescription(abilityName);
    //         info += "\nCost: " + LandmarkManager.Instance.anvilResearchData[abilityName].manaCost.ToString() + " mana";
    //         info += "\nDuration: " + LandmarkManager.Instance.anvilResearchData[abilityName].durationInHours.ToString() + " hours";
    //     } else {
    //         info = theAnvil.GetUnavailabilityDescription(abilityName);
    //         if (info != string.Empty) {
    //             info += "\n";
    //         }
    //         if (PlayerManager.Instance.player.mana >= LandmarkManager.Instance.anvilResearchData[abilityName].manaCost) {
    //             info += "Cost: " + LandmarkManager.Instance.anvilResearchData[abilityName].manaCost.ToString() + " mana";
    //         } else {
    //             info += "<color=red>Cost: " + LandmarkManager.Instance.anvilResearchData[abilityName].manaCost.ToString() + " mana</color>";
    //         }
    //         info += "\nDuration: " + LandmarkManager.Instance.anvilResearchData[abilityName].durationInHours.ToString() + " hours";
    //     }
    //     UIManager.Instance.ShowSmallInfo(info);
    // }
    // private void OnHoverExitAbilityChoice(string abilityName) {
    //     UIManager.Instance.HideSmallInfo();
    // }
    // #endregion

}
