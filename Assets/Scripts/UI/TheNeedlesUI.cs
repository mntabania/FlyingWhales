using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheNeedlesUI : MonoBehaviour {

    [Header("General")]
    public Button convertBtn;
    public Image cooldownProgress;

    //public TheNeedles needles { get; private set; }

    #region General
    public void OnClickConvert(BaseLandmark landmark) {
        //needles = landmark as TheNeedles;
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker<Character>(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), "Choose Minion to Convert to Mana", CanChooseMinion, OnHoverEnterMinion, OnHoverExitMinion, null, OnConfirmConvert, "Convert", column2Optional: true);
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = $"{character.name} is already doing something else.";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnConfirmConvert(object minionObj, object obj) {
        Minion minion = (minionObj as Character).minion;
        //needles.tileLocation.region.SetAssignedMinion(minion);
        //minion.SetAssignedRegion(needles.tileLocation.region);
        // needles.Activate();
    }
    #endregion

    #region Minion
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    #endregion

}
