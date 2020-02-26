using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class Destroy : PlayerSpell {

    public Destroy() : base(SPELL_TYPE.DESTROY) {
        //description = "Remove this object from the world.";
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return;
        }
        targetPOI.gridTileLocation.structure.RemovePOI(targetPOI);
        base.ActivateAction(targetPOI);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "destroyed", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }

    //protected override bool ShouldButtonBeInteractable(Character character, IPointOfInterest targetPOI) {
    //    if (!targetPOI.IsAvailable()) {
    //        return false;
    //    }
    //    return base.ShouldButtonBeInteractable(character, targetPOI);
    //}
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is TileObject)) {
            return false;
        }
        if (targetPOI.gridTileLocation == null) {
            return false;
        }
        return base.CanTarget(targetPOI, ref hoverText);
    }
}

public class DestroyData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.DESTROY;
    public override string name { get { return "Destroy"; } }
    public override string description { get { return "Destroys an object"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }

    public DestroyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        LocationGridTile targetTile = targetPOI.gridTileLocation;
        targetTile.structure.RemovePOI(targetPOI);
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "destroyed", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);

        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Destroy_Explosion);
        if (UIManager.Instance.tileObjectInfoUI.isShowing && UIManager.Instance.tileObjectInfoUI.activeTileObject == targetPOI) {
            UIManager.Instance.tileObjectInfoUI.CloseMenu();
        }
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    // public override bool CanPerformAbilityTowards(SpecialToken item) {
    //     if (item.gridTileLocation == null) {
    //         return false;
    //     }
    //     return base.CanPerformAbilityTowards(item);
    // }
    #endregion
}