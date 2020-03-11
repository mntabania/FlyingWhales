using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class FurnitureSpotMono : MonoBehaviour {
    
    public FURNITURE_TYPE[] allowedFurnitureTypes;
    public List<FurnitureSetting> furnitureSettings;

    // private void Update() {
    //     string summary = string.Empty;
    //     for (int i = 0; i < allowedFurnitureTypes.Length ; i++) {
    //         summary += allowedFurnitureTypes[i].ToString() + ", ";
    //     }
    //     this.name = summary;
    //     furnitureSummary.text = summary;
    // }

    public FurnitureSpot GetFurnitureSpot() {
        return new FurnitureSpot() {
            allowedFurnitureTypes = allowedFurnitureTypes,
            location = new Vector3Int((int)(transform.localPosition.x - 0.5f), (int)(transform.localPosition.y - 0.5f), 0),
            furnitureSettings = furnitureSettings
        };
    }
}

[System.Serializable]
public struct FurnitureSetting {
    public FURNITURE_TYPE type;
    public Vector3 rotation;
    public Sprite assetToUse;
}
