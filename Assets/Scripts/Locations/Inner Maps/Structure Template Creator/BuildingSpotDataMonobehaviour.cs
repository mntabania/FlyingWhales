using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class BuildingSpotDataMonobehaviour : MonoBehaviour {

    public int id;
    public List<BuildingSpotDataMonobehaviour> adjacentSpots;
    public TextMeshPro idText;
    public bool isOpen = false;

    public Vector2 buildingSpotGridPos;

    private void Update() {
        id = transform.GetSiblingIndex();
        idText.text = id.ToString();
        string summary = $"{id} - {this.transform.localPosition}";
        this.name = summary;
    }

    public BuildingSpotData Convert() {
        int[] converted = new int[adjacentSpots.Count];
        for (int i = 0; i < adjacentSpots.Count; i++) {
            BuildingSpotDataMonobehaviour currSpot = adjacentSpots[i];
            converted[i] = currSpot.id;
        }
        return new BuildingSpotData() {
            id = this.id,
            location = new Vector3Int((int)(transform.localPosition.x), (int)(transform.localPosition.y), 0),
            isOpen = this.isOpen,
            adjacentSpots = converted,
            buildingSpotGridPos = new Vector2Int((int)this.buildingSpotGridPos.x, (int)this.buildingSpotGridPos.y)
        };
    }
}
