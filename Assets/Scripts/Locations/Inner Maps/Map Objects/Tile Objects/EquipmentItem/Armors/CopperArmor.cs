using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperArmor : ArmorItem {
    public CopperArmor() {
        Initialize(TILE_OBJECT_TYPE.COPPER_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public CopperArmor(SaveDataTileObject data) { }
}