using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class BerserkOrb : Artifact {

    public BerserkOrb() : base(ARTIFACT_TYPE.Berserk_Orb) {
    }
    //public BerserkOrb(SaveDataArtifact data) : base(data) {
    //}

    #region Overrides
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            List<LocationGridTile> tilesInRange = gridTileLocation.GetTilesInRadius(1);
            for (int i = 0; i < tilesInRange.Count; i++) {
                LocationGridTile currTile = tilesInRange[i];
                if(currTile.charactersHere.Count > 0) {
                    for (int j = 0; j < currTile.charactersHere.Count; j++) {
                        Character character = currTile.charactersHere[j];
                        character.traitContainer.AddTrait(character, "Berserked");
                    }
                }
            }
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    #endregion
}
