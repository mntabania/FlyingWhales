﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class FurnitureSpotMono : MonoBehaviour {

    [SerializeField] private TextMeshPro furnitureSummary;

    public List<FURNITURE_TYPE> allowedFurnitureTypes;

    private void Update() {
        string summary = string.Empty;
        for (int i = 0; i < allowedFurnitureTypes.Count ; i++) {
            summary += allowedFurnitureTypes[i].ToString() + ", ";
        }
        this.name = summary;
        furnitureSummary.text = summary;
    }
}
