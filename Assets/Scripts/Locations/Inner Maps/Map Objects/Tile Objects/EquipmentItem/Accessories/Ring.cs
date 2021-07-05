using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Ring : AccessoryItem {
    public Ring() {
        Initialize(TILE_OBJECT_TYPE.RING, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public Ring(SaveDataTileObject data) { }
}