using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronStaff : WeaponItem {
    public IronStaff() {
        Initialize(TILE_OBJECT_TYPE.IRON_STAFF, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronStaff(SaveDataTileObject data) { }
}