using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MithrilAxe : WeaponItem {
    public MithrilAxe() {
        Initialize(TILE_OBJECT_TYPE.MITHRIL_AXE, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MithrilAxe(SaveDataTileObject data) { }
}