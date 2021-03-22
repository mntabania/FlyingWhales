using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

public class SettlementNameplateItem : NameplateItem<BaseSettlement> {
    
    [Header("Settlement Attributes")]
    [SerializeField] private LocationPortrait portrait;

    private BaseSettlement _settlement;
    private void OnEnable() {
        AddOnRightClickAction(OnRightClickItem);
    }
    private void OnDisable() {
        RemoveOnRightClickAction(OnRightClickItem);
    }
    
    public override void SetObject(BaseSettlement o) {
        base.SetObject(o);
        _settlement = o;
        UpdateVisuals();
    }
    private void UpdateVisuals() {
        if (_settlement.areas.Count > 0) {
            portrait.SetPortrait(STRUCTURE_TYPE.CITY_CENTER);
        }
        mainLbl.text = _settlement.name;
        if (_settlement is NPCSettlement npcSettlement) {
            if (npcSettlement.settlementType != null) {
                subLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(npcSettlement.settlementType.settlementType.ToString());    
            } else if (npcSettlement.structures.Count > 0) {
                STRUCTURE_TYPE structureType = npcSettlement.structures.First().Key;
                subLbl.text = structureType.StructureName();
            } else {
                subLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(_settlement.locationType.ToString());
            }
        } else {
            subLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(_settlement.locationType.ToString());    
        }
    }

    private void OnRightClickItem(BaseSettlement p_settlement) {
        UIManager.Instance.ShowPlayerActionContextMenu(p_settlement, Input.mousePosition, true);
    }
    public override void Reset() {
        base.Reset();
        _settlement = null;
    }
}
