using System.Collections;
using System.Collections.Generic;
using Locations.Settlements;
using UnityEngine;

public class SettlementNameplateItem : NameplateItem<BaseSettlement> {
    
    [Header("Settlement Attributes")]
    [SerializeField] private LocationPortrait portrait;

    private BaseSettlement _settlement;
    
    public override void SetObject(BaseSettlement o) {
        base.SetObject(o);
        _settlement = o;
        UpdateVisuals();
    }
    private void UpdateVisuals() {
        if (_settlement.tiles.Count > 0) {
            BaseLandmark firstLandmark = _settlement.tiles[0].landmarkOnTile;
            portrait.SetPortrait(firstLandmark?.specificLandmarkType ?? LANDMARK_TYPE.HOUSES);    
        }
        mainLbl.text = _settlement.name;
        subLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(_settlement.locationType.ToString());
    }
}
