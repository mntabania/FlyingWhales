using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MithrilSword : WeaponItem {
    public MithrilSword() {
        Initialize(TILE_OBJECT_TYPE.MITHRIL_SWORD, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MithrilSword(SaveDataTileObject data) { }
}