using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BoarHideArmor : ArmorItem {
    public BoarHideArmor() {
        Initialize(TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BoarHideArmor(SaveDataTileObject data) { }
}