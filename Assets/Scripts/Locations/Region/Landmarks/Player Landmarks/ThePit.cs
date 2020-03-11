using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class ThePit : BaseLandmark {

    public ThePit(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public ThePit(HexTile location, SaveDataLandmark data) : base(location, data) { }
}
