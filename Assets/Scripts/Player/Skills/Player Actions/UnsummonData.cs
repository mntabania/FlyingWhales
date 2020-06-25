using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class UnsummonData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.UNSUMMON;
    public override string name => "Unsummon";
    public override string description => $"Unsummon a minion.";
    public UnsummonData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character && character.minion != null) {
            character.minion.Death();
            if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == character) {
                UIManager.Instance.monsterInfoUI.CloseMenu();
            }
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return targetCharacter.minion != null && targetCharacter.minion.isSummoned;
        }
        return false;
    }
    #endregion
}