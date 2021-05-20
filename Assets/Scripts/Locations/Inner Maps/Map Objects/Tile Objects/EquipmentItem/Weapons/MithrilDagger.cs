using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MithrilDagger : WeaponItem {
    public MithrilDagger() {
        Initialize(TILE_OBJECT_TYPE.MITHRIL_DAGGER, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MithrilDagger(SaveDataTileObject data) { }
}