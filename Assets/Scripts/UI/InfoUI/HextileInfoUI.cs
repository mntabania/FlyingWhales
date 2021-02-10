﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Area_Features;
using TMPro;
using UnityEngine.UI;


public class HextileInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private LocationPortrait _locationPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI tileTypeLbl;
    
    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI featuresLbl;
    
    public HexTile activeHex { get; private set; }
    
    public override void OpenMenu() {
        // currentlyShowingHexTile?.SetBordersState(false, false, Color.red);
        activeHex = _data as HexTile;
        base.OpenMenu();
        // Selector.Instance.Select(currentlyShowingHexTile);
        // currentlyShowingHexTile.SetBordersState(true, true, Color.yellow);
        UpdateBasicInfo();
        UpdateHexTileInfo();
    }
    public override void SetData(object data) {
        base.SetData(data); //replace this existing data
        if (isShowing) {
            UpdateHexTileInfo();
        }
    }
    public override void CloseMenu() {
        // currentlyShowingHexTile.SetBordersState(false, false, Color.red);
        // Selector.Instance.Deselect();
        base.CloseMenu();
        activeHex = null;
    }
    private void UpdateBasicInfo() {
        if (activeHex.landmarkOnTile != null) {
            _locationPortrait.SetPortrait(activeHex.landmarkOnTile.landmarkPortrait);    
        } else {
            _locationPortrait.SetPortrait(activeHex.elevationType == ELEVATION.MOUNTAIN
                ? LANDMARK_TYPE.CAVE
                : LANDMARK_TYPE.NONE);
        }
        //nameLbl.text = activeHex.GetDisplayName();
        //tileTypeLbl.text = activeHex.GetSubName();
    }
    
    public void UpdateHexTileInfo() {
        featuresLbl.text = string.Empty;
        if (activeHex.featureComponent.features.Count == 0) {
            featuresLbl.text = $"{featuresLbl.text}None";
        } else {
            for (int i = 0; i < activeHex.featureComponent.features.Count; i++) {
                AreaFeature feature = activeHex.featureComponent.features[i];
                if (i != 0) {
                    featuresLbl.text = $"{featuresLbl.text}, ";
                }
                featuresLbl.text = $"{featuresLbl.text}<link=\"{i}\">{feature.name}</link>";
            }
        }
    }
    
    public void OnHoverFeature(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            UIManager.Instance.ShowSmallInfo(activeHex.featureComponent.features[index].description);
        }
    }
    public void OnHoverExitFeature() {
        UIManager.Instance.HideSmallInfo();
    }

    #region For Testing
    public void ShowTestingInfo() {
#if UNITY_EDITOR
        string summary = activeHex.ToString();
        if (activeHex.settlementOnTile is NPCSettlement npcSettlement) {
            summary = $"Settlement Ruler: {npcSettlement.ruler?.name ?? "None"}";
            summary += $"\n{npcSettlement.name} Job Assignments: ";
            summary += npcSettlement.jobPriorityComponent.GetJobAssignments();
        }
        UIManager.Instance.ShowSmallInfo(summary);
#endif
    }
    public void HideTestingInfo() {
#if UNITY_EDITOR
        UIManager.Instance.HideSmallInfo();
#endif
    }
    #endregion
}
