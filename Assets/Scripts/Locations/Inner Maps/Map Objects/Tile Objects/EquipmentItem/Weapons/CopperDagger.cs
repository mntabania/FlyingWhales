using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperDagger : WeaponItem {
    public CopperDagger() {
        Initialize(TILE_OBJECT_TYPE.COPPER_DAGGER, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public CopperDagger(SaveDataTileObject data) { }
}