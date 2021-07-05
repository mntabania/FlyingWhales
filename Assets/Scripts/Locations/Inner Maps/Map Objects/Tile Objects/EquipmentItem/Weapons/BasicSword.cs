using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicSword : WeaponItem {
    public BasicSword() {
        Initialize(TILE_OBJECT_TYPE.BASIC_SWORD, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicSword(SaveDataTileObject data) { }
}