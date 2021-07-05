using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicAxe : WeaponItem {
    public BasicAxe() {
        Initialize(TILE_OBJECT_TYPE.BASIC_AXE, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicAxe(SaveDataTileObject data) { }
}