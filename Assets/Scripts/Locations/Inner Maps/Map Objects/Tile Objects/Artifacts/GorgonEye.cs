using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class GorgonEye : Artifact {

    public GorgonEye() : base(ARTIFACT_TYPE.Gorgon_Eye) {
        maxHP = 700;
    }
    //public GorgonEye(SaveDataArtifact data) : base(data) {
    //}

    #region Overrides
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
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    #endregion
}
