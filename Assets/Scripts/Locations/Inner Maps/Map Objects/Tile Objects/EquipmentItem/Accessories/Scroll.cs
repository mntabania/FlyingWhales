using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Scroll : AccessoryItem {
    public Scroll() {
        Initialize(TILE_OBJECT_TYPE.SCROLL, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public Scroll(SaveDataTileObject data) { }
}