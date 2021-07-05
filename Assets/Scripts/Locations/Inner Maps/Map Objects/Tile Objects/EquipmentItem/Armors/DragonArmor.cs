using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DragonArmor : ArmorItem {
    public DragonArmor() {
        Initialize(TILE_OBJECT_TYPE.DRAGON_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public DragonArmor(SaveDataTileObject data) { }
}