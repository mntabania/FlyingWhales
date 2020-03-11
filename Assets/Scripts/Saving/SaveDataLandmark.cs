using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataLandmark {
    public int id;
    public string landmarkName;
    public LANDMARK_TYPE landmarkType;
    public int locationID;
    public int connectedTileID;
    public int invasionTicks;
    public List<LANDMARK_TAG> landmarkTags;
   
    public virtual void Save(BaseLandmark landmark) {
        id = landmark.id;
        landmarkName = landmark.landmarkName;
        landmarkType = landmark.specificLandmarkType;
        locationID = landmark.tileLocation.id;
        landmarkTags = landmark.landmarkTags;
        invasionTicks = landmark.invasionTicks;
    }
    public virtual void LoadSpecificLandmarkData(BaseLandmark landmark) { }

    public void Load(HexTile tile) {
        BaseLandmark landmark = tile.CreateLandmarkOfType(this);
        LoadSpecificLandmarkData(landmark);
    }
}
