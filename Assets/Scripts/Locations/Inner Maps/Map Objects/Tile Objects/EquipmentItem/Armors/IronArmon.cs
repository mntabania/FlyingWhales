using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronArmor : ArmorItem {
    public IronArmor() {
        Initialize(TILE_OBJECT_TYPE.IRON_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronArmor(SaveDataTileObject data) { }
}