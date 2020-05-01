using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.KILL;
    public override string name { get { return "Kill"; } }
    public override string description { get { return "This Action can be used to summon Demons or Minions to Kill a Resident."; } }

    public KillData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            PlayerUI.Instance.unleashSummonUI.SetTargetCharacter(targetCharacter);
            PlayerUI.Instance.unleashSummonUI.ShowUnleashSummonUI("kill");
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return !targetCharacter.isDead;
        }
        return canPerform;
    }
    #endregion
}
