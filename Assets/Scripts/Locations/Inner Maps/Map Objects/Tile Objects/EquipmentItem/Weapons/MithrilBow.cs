using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MithrilBow : WeaponItem {
    public MithrilBow() {
        Initialize(TILE_OBJECT_TYPE.MITHRIL_BOW, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MithrilBow(SaveDataTileObject data) { }
}