using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronDagger : WeaponItem {
    public IronDagger() {
        Initialize(TILE_OBJECT_TYPE.IRON_DAGGER, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronDagger(SaveDataTileObject data) { }
}