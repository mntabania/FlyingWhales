using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class AnkhOfAnubis : Artifact {

    public AnkhOfAnubis() : base(ARTIFACT_TYPE.Ankh_Of_Anubis) {
    }
    public AnkhOfAnubis(SaveDataArtifact data) : base(data) {
    }

    #region Overrides
    public override void ActivateArtifactEffect() {
        if(gridTileLocation != null) {
            base.ActivateArtifactEffect();
            Quicksand quicksand = InnerMapManager.Instance.CreateNewTileObject<Quicksand>(TILE_OBJECT_TYPE.QUICKSAND);
            quicksand.SetGridTileLocation(gridTileLocation);
            quicksand.OnPlacePOI();

            gridTileLocation.structure.RemovePOI(this);
        }
    }
    #endregion
}
