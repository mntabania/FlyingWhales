using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronBow : WeaponItem {
    public IronBow() {
        Initialize(TILE_OBJECT_TYPE.IRON_BOW, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronBow(SaveDataTileObject data) { }
}