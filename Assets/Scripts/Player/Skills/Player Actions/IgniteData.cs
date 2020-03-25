using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class IgniteData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.IGNITE;
    public override string name { get { return "Ignite"; } }
    public override string description { get { return "Targets a spot. Target will ignite and start spreading fire."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    public IgniteData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        // LocationGridTile tile = targetPOI.gridTileLocation;
        BurningSource bs = new BurningSource(targetPOI.gridTileLocation.parentMap.region);
        Burning burning = new Burning();
        burning.SetSourceOfBurning(bs, targetPOI);
        targetPOI.traitContainer.AddTrait(targetPOI, burning, bypassElementalChance: true);
        Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.gridTileLocation.genericTileObject.traitContainer.HasTrait("Burning", "Wet", "Fireproof")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    // public override bool CanPerformAbilityTowards(SpecialToken item) {
    //     if (item.gridTileLocation == null || item.gridTileLocation.genericTileObject.traitContainer.HasTrait("Burning")) {
    //         return false;
    //     }
    //     return base.CanPerformAbilityTowards(item);
    // }
    #endregion
}