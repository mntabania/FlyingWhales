using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class ChangeCombatModeData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.CHANGE_COMBAT_MODE;
    public override string name { get { return "Change Combat Mode"; } }
    public override string description { get { return "Change Combat Mode"; } }

    public ChangeCombatModeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            UIManager.Instance.characterInfoUI.ShowSwitchCombatModeUI();
        }
    }
    public override string GetLabelName(IPlayerActionTarget target) {
        if(target is Character character) {
            return "Combat Mode: " + UtilityScripts.Utilities.NotNormalizedConversionEnumToString(character.combatComponent.combatMode.ToString());
        }
        return base.GetLabelName(target);
    }
    #endregion
}