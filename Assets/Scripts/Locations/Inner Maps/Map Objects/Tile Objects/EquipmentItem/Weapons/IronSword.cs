using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronSword : WeaponItem {
    public IronSword() {
        Initialize(TILE_OBJECT_TYPE.IRON_SWORD, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronSword(SaveDataTileObject data) { }
}