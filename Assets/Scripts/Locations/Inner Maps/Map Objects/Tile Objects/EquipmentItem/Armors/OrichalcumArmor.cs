using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OrichalcumArmor : ArmorItem {
    public OrichalcumArmor() {
        Initialize(TILE_OBJECT_TYPE.ORICHALCUM_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public OrichalcumArmor(SaveDataTileObject data) { }
}