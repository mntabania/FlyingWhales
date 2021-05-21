using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WoolShirt : ArmorItem {
    public WoolShirt() {
        Initialize(TILE_OBJECT_TYPE.WOOL_SHIRT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public WoolShirt(SaveDataTileObject data) { }
}