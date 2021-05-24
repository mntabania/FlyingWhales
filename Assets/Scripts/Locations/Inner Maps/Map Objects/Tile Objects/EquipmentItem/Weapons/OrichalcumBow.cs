using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OrichalcumBow : WeaponItem {
    public OrichalcumBow() {
        Initialize(TILE_OBJECT_TYPE.ORICHALCUM_BOW, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public OrichalcumBow(SaveDataTileObject data) { }
}