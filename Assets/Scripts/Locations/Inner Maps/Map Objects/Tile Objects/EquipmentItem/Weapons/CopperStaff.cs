using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperStaff : WeaponItem {
    public CopperStaff() {
        Initialize(TILE_OBJECT_TYPE.COPPER_STAFF, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public CopperStaff(SaveDataTileObject data) { }
}