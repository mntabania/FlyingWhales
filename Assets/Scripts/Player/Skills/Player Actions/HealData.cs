using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HEAL;
    public override string name => "Heal";
    public override string description => "This Action fully replenishes a character's HP.";
    public HealData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            targetCharacter.ResetToFullHP();
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Character targetCharacter) {
            return targetCharacter.currentHP < targetCharacter.maxHP;
        }
        return false;
    }
    #endregion
}
