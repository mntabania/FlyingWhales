using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperAxe : WeaponItem {
    public CopperAxe() {
        Initialize(TILE_OBJECT_TYPE.COPPER_AXE, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public CopperAxe(SaveDataTileObject data) { }
}