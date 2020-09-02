using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class IgniteData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.IGNITE;
    public override string name => "Ignite";
    public override string description => "This Action can be used to apply Burning to an object.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.PLAYER_ACTION;
    public IgniteData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        // LocationGridTile tile = targetPOI.gridTileLocation;
        BurningSource bs = new BurningSource();
        Burning burning = new Burning();
        burning.InitializeInstancedTrait();
        burning.SetSourceOfBurning(bs, targetPOI);
        targetPOI.traitContainer.AddTrait(targetPOI, burning, bypassElementalChance: true);
        Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.gridTileLocation.genericTileObject.traitContainer.HasTrait("Burning", "Wet", "Fireproof")) {
            return false;
        }
        if (!tileObject.traitContainer.HasTrait("Flammable")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    #endregion
}