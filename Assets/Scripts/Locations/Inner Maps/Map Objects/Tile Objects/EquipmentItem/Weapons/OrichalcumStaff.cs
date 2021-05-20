using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OrichalcumStaff : WeaponItem {
    public OrichalcumStaff() {
        Initialize(TILE_OBJECT_TYPE.ORICHALCUM_STAFF, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public OrichalcumStaff(SaveDataTileObject data) { }
}