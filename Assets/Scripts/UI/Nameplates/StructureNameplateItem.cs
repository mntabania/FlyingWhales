using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

public class StructureNameplateItem : NameplateItem<LocationStructure> {
    
    [Header("Settlement Attributes")]
    [SerializeField] private LocationPortrait portrait;

    private LocationStructure _structure;
    
    public override void SetObject(LocationStructure o) {
        base.SetObject(o);
        _structure = o;
        UpdateVisuals();
    }
    private void UpdateVisuals() {
        mainLbl.text = _structure.name;
        subLbl.text = string.Empty;
        portrait.SetPortrait(_structure.structureType);
    }
}
