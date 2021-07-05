using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OrichalcumAxe : WeaponItem {
    public OrichalcumAxe() {
        Initialize(TILE_OBJECT_TYPE.ORICHALCUM_AXE, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public OrichalcumAxe(SaveDataTileObject data) { }
}