using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BearHideArmor : ArmorItem {
    public BearHideArmor() {
        Initialize(TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BearHideArmor(SaveDataTileObject data) { }
}