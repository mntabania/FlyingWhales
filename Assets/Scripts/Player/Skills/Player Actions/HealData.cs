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
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if(targetCharacter.currentHP >= targetCharacter.maxHP) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.currentHP >= targetCharacter.maxHP) {
            reasons += $"{targetCharacter.name} is at full HP,";
        }
        return reasons;
    }
    #endregion
}
