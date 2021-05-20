using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperBow : WeaponItem {
    public CopperBow() {
        Initialize(TILE_OBJECT_TYPE.COPPER_BOW, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public CopperBow(SaveDataTileObject data) { }
}