using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicCloth : WeaponItem {
    public BasicCloth() {
        Initialize(TILE_OBJECT_TYPE.BASIC_CLOTH, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicCloth(SaveDataTileObject data) { }
}