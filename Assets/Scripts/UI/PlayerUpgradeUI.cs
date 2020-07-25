using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgradeUI : MonoBehaviour {
    
    //private TheAnvil theAnvil;


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
