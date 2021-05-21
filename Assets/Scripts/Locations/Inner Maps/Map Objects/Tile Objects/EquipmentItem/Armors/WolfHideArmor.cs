using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WolfHideArmor : ArmorItem {
    public WolfHideArmor() {
        Initialize(TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public WolfHideArmor(SaveDataTileObject data) { }
}