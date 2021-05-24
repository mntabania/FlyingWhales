using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MithrilStaff : WeaponItem {
    public MithrilStaff() {
        Initialize(TILE_OBJECT_TYPE.MITHRIL_STAFF, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MithrilStaff(SaveDataTileObject data) { }
}