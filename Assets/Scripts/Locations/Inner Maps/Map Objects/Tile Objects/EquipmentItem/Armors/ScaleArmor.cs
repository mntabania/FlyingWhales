using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ScaleArmor : ArmorItem {
    public ScaleArmor() {
        Initialize(TILE_OBJECT_TYPE.SCALE_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public ScaleArmor(SaveDataTileObject data) { }
}