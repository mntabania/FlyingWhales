using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoonwalkerShirt : ArmorItem {
    public MoonwalkerShirt() {
        Initialize(TILE_OBJECT_TYPE.MOONWALKER_SHIRT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MoonwalkerShirt(SaveDataTileObject data) { }
}