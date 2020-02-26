using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    public HexTile currentlyShowingHexTile { get; private set; }
    
    public override void OpenMenu() {
        currentlyShowingHexTile?.SetBordersState(false, false, Color.red);
        currentlyShowingHexTile = _data as HexTile;
        base.OpenMenu();
        Selector.Instance.Select(currentlyShowingHexTile);
        currentlyShowingHexTile.SetBordersState(true, true, Color.yellow);
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
        currentlyShowingHexTile.SetBordersState(false, false, Color.red);
        Selector.Instance.Deselect();
        base.CloseMenu();
        currentlyShowingHexTile = null;
    }
    private void UpdateBasicInfo() {
        if (currentlyShowingHexTile.landmarkOnTile != null) {
            _locationPortrait.SetPortrait(currentlyShowingHexTile.landmarkOnTile.specificLandmarkType);    
        } else {
            _locationPortrait.SetPortrait(LANDMARK_TYPE.NONE);
        }
        nameLbl.text = currentlyShowingHexTile.GetDisplayName();
        tileTypeLbl.text = currentlyShowingHexTile.GetSubName();
    }
    
    public void UpdateHexTileInfo() {
        featuresLbl.text = string.Empty;
        if (currentlyShowingHexTile.featureComponent.features.Count == 0) {
            featuresLbl.text = $"{featuresLbl.text}None";
        } else {
            for (int i = 0; i < currentlyShowingHexTile.featureComponent.features.Count; i++) {
                TileFeature feature = currentlyShowingHexTile.featureComponent.features[i];
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
            UIManager.Instance.ShowSmallInfo(currentlyShowingHexTile.featureComponent.features[index].description);
        }
    }
    public void OnHoverExitFeature() {
        UIManager.Instance.HideSmallInfo();
    }

    #region For Testing
    public void ShowTestingInfo() {
        string summary = $"Settlement Ruler: {currentlyShowingHexTile.settlementOnTile?.ruler?.name}" ?? "None";
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
