using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SeizeCharacterData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SEIZE_CHARACTER;
    public override string name { get { return "Seize Character"; } }
    public override string description { get { return "Seize Character"; } }

    public SeizeCharacterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && !targetCharacter.traitContainer.HasTrait("Leader", "Blessed");
    }
    #endregion
}