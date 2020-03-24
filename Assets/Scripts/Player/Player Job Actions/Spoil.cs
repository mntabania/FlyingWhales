using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class SpoilData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.SPOIL;
    public override string name { get { return "Spoil"; } }
    public override string description { get { return "Poison the food at the target table."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    public SpoilData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        targetPOI.traitContainer.AddTrait(targetPOI, "Poisoned");
        Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.traitContainer.HasTrait("Poisoned", "Robust")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    #endregion
}