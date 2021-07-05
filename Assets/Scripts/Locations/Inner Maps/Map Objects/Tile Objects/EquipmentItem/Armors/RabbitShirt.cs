using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RabbitShirt : ArmorItem {
    public RabbitShirt() {
        Initialize(TILE_OBJECT_TYPE.RABBIT_SHIRT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public RabbitShirt(SaveDataTileObject data) { }
}