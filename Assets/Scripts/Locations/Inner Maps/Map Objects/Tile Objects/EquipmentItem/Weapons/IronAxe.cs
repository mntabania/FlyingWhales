using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronAxe : WeaponItem {
    public IronAxe() {
        Initialize(TILE_OBJECT_TYPE.IRON_AXE, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronAxe(SaveDataTileObject data) { }
}