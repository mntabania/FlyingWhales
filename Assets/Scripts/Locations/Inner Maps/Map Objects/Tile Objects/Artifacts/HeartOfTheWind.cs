using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class HeartOfTheWind : Artifact {

    public HeartOfTheWind() : base(ARTIFACT_TYPE.Heart_Of_The_Wind) {
        maxHP = 700;
        currentHP = maxHP;
    }
    //public HeartOfTheWind(SaveDataArtifact data) : base(data) {
    //}

    #region Overrides
    public override void ActivateTileObject() {
        if (gridTileLocation != null) {
            base.ActivateTileObject();
            TornadoTileObject tornadoTileObject = new TornadoTileObject();
            tornadoTileObject.SetRadius(1);
            tornadoTileObject.SetDuration(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4)));
            tornadoTileObject.SetGridTileLocation(gridTileLocation);
            tornadoTileObject.OnPlacePOI();

            //gridTileLocation.structure.RemovePOI(this);
        }
    }
    #endregion
}
