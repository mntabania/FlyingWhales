using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FurnitureSpot {

    public Vector3Int location; //where in the template grid is this furniture spot placed
    public FURNITURE_TYPE[] allowedFurnitureTypes;
    public List<FurnitureSetting> furnitureSettings;

    public override string ToString() {
        string summary = string.Empty;
        if (allowedFurnitureTypes != null) {
            for (int i = 0; i < allowedFurnitureTypes.Length; i++) {
                summary += $"|{allowedFurnitureTypes[i]}|";
            }
        }
        return summary;
    }

    public bool TryGetFurnitureSettings(FURNITURE_TYPE type, out FurnitureSetting setting) {
        if (furnitureSettings != null) {
            for (int i = 0; i < furnitureSettings.Count; i++) {
                FurnitureSetting currSetting = furnitureSettings[i];
                if (currSetting.type == type) {
                    setting = currSetting;
                    return true;
                }
            }
        }
        setting = default(FurnitureSetting);
        return false;
    }
}
