using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicShirt : ArmorItem {
    public BasicShirt() {
        Initialize(TILE_OBJECT_TYPE.BASIC_SHIRT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicShirt(SaveDataTileObject data) { }
}