using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

public class BerserkOrb : Artifact {

    public BerserkOrb() : base(ARTIFACT_TYPE.Berserk_Orb) {
        maxHP = 700;
        currentHP = maxHP;
    }
    public BerserkOrb(SaveDataArtifact data) : base(data) { }

    #region Overrides
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddAdvertisedAction(INTERACTION_TYPE.INSPECT);
    }
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Berserk_Orb_Activate);
            GameManager.Instance.StartCoroutine(BerserkOrbEffect(gridTileLocation));
        }
    }
    private IEnumerator BerserkOrbEffect(LocationGridTile tileLocation) {
        yield return new WaitForSeconds(0.5f);
        List<LocationGridTile> tilesInRange = RuinarchListPool<LocationGridTile>.Claim();
        tileLocation.PopulateTilesInRadius(tilesInRange, 3);
        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile currTile = tilesInRange[i];
            if (currTile.charactersHere.Count > 0) {
                for (int j = 0; j < currTile.charactersHere.Count; j++) {
                    Character character = currTile.charactersHere[j];
                    character.traitContainer.AddTrait(character, "Berserked");
                }
            }
        }
        RuinarchListPool<LocationGridTile>.Release(tilesInRange);
        //tileLocation.structure.RemovePOI(this);
    }
    public override void OnInspect(Character inspector) {
        base.OnInspect(inspector);
        inspector.traitContainer.AddTrait(inspector, "Berserked");
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "Berserk Orb", "inspect", providedTags: LOG_TAG.Life_Changes);
        log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToDatabase(true);

        if (GameUtilities.RollChance(30)) {
            gridTileLocation.structure.RemovePOI(this, inspector);
        }
    }
    #endregion
}
