using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicBow : WeaponItem {
    public BasicBow() {
        Initialize(TILE_OBJECT_TYPE.BASIC_BOW, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicBow(SaveDataTileObject data) { }
}