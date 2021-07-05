using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Necklace : AccessoryItem {
    public Necklace() {
        Initialize(TILE_OBJECT_TYPE.NECKLACE, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public Necklace(SaveDataTileObject data) { }
}