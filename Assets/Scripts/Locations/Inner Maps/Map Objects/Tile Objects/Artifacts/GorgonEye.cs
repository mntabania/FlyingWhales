using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

public class GorgonEye : Artifact {

    public GorgonEye() : base(ARTIFACT_TYPE.Gorgon_Eye) {
        maxHP = 700;
        currentHP = maxHP;
    }
    public GorgonEye(SaveDataArtifact data) : base(data) { }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddAdvertisedAction(INTERACTION_TYPE.INSPECT);
    }
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            List<LocationGridTile> tilesInRange = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(gridTileLocation.parentMap, gridTileLocation.localPlace, 3);
            for (int i = 0; i < tilesInRange.Count; i++) {
                LocationGridTile currTile = tilesInRange[i];
                if (currTile.charactersHere.Count > 0) {
                    for (int j = 0; j < currTile.charactersHere.Count; j++) {
                        Character character = currTile.charactersHere[j];
                        character.traitContainer.AddTrait(character, "Paralyzed");
                    }
                }
            }
            GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Gorgon_Eye);
            //gridTileLocation.structure.RemovePOI(this);
        }
    }
    public override void OnInspect(Character inspector) {
        base.OnInspect(inspector);
        inspector.traitContainer.AddTrait(inspector, "Paralyzed");
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "Gorgon Eye", "inspect", providedTags: LOG_TAG.Life_Changes);
        log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToDatabase(true);

        if (GameUtilities.RollChance(30)) {
            gridTileLocation.structure.RemovePOI(this, inspector);
        }
    }
    #endregion
}
