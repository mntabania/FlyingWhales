using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockoutData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.KNOCKOUT;
    public override string name { get { return "Knockout"; } }
    public override string description { get { return "Knockout"; } }

    public KnockoutData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character targetCharacter) {
            PlayerUI.Instance.unleashSummonUI.SetTargetCharacter(targetCharacter);
            PlayerUI.Instance.unleashSummonUI.ShowUnleashSummonUI("knockout");
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return !targetCharacter.traitContainer.HasTrait("Unconscious");
        }
        return canPerform;
    }
    #endregion
}
