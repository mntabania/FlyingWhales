using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OrichalcumSword : WeaponItem {
    public OrichalcumSword() {
        Initialize(TILE_OBJECT_TYPE.ORICHALCUM_SWORD, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public OrichalcumSword(SaveDataTileObject data) { }
}