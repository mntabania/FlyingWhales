using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class BerserkOrb : Artifact {

    public BerserkOrb() : base(ARTIFACT_TYPE.Berserk_Orb) {
        maxHP = 700;
    }
    //public BerserkOrb(SaveDataArtifact data) : base(data) {
    //}

    #region Overrides
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Berserk_Orb_Activate);
            GameManager.Instance.StartCoroutine(BerserkOrbEffect(gridTileLocation));
        }
    }
    private IEnumerator BerserkOrbEffect(LocationGridTile tileLocation) {
        yield return new WaitForSeconds(0.5f);
        List<LocationGridTile> tilesInRange = tileLocation.GetTilesInRadius(3);
        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile currTile = tilesInRange[i];
            if (currTile.charactersHere.Count > 0) {
                for (int j = 0; j < currTile.charactersHere.Count; j++) {
                    Character character = currTile.charactersHere[j];
                    character.traitContainer.AddTrait(character, "Berserked");
                }
            }
        }
        tileLocation.structure.RemovePOI(this);
    }
    #endregion
}
