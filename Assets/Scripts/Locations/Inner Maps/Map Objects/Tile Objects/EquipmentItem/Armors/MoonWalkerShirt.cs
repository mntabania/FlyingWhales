using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MoonWalkerShirt : ArmorItem {
    public MoonWalkerShirt() {
        Initialize(TILE_OBJECT_TYPE.MOON_WALKER_SHIRT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MoonWalkerShirt(SaveDataTileObject data) { }
}