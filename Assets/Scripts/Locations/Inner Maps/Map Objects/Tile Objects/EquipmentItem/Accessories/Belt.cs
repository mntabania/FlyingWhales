using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Belt : AccessoryItem {
    public Belt() {
        Initialize(TILE_OBJECT_TYPE.BELT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public Belt(SaveDataTileObject data) { }
}