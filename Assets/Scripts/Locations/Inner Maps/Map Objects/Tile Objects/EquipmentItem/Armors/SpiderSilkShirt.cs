using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SpiderSilkShirt : ArmorItem {
    public SpiderSilkShirt() {
        Initialize(TILE_OBJECT_TYPE.SPIDER_SILK_SHIRT, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public SpiderSilkShirt(SaveDataTileObject data) { }
}