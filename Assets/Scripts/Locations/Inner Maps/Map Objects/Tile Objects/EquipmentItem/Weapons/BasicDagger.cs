using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicDagger : WeaponItem {
    public BasicDagger() {
        Initialize(TILE_OBJECT_TYPE.BASIC_DAGGER, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicDagger(SaveDataTileObject data) { }
}