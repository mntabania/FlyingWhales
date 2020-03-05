using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class TheSpire : BaseLandmark {

    public TheSpire(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
      
    }

    public TheSpire(HexTile location, SaveDataLandmark data) : base(location, data) {
    }
    public void LoadSavedData(SaveDataTheSpire data) {
       
    }

    #region Override
    public override void DestroyLandmark() {
        base.DestroyLandmark();
    }
    #endregion
}

public class SaveDataTheSpire: SaveDataLandmark {
    public int currentCooldownTick;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        //TheSpire spire = landmark as TheSpire;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        //TheSpire spire = landmark as TheSpire;
        //spire.LoadSavedData(this);
    }
}
