using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class DestroyData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.DESTROY;
    public override string name { get { return "Destroy"; } }
    public override string description { get { return "This Action destroys an object."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.PLAYER_ACTION; } }

    public DestroyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        LocationGridTile targetTile = targetPOI.gridTileLocation;
        targetPOI.AdjustHP(-targetPOI.currentHP, ELEMENTAL_TYPE.Normal, true);
        // targetTile.structure.RemovePOI(targetPOI);
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "destroyed", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);

        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Destroy_Explosion);
        if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == targetPOI) {
            UIManager.Instance.tileObjectInfoUI.CloseMenu();
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    #endregion
}