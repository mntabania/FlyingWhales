using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class OrichalcumDagger : WeaponItem {
    public OrichalcumDagger() {
        Initialize(TILE_OBJECT_TYPE.ORICHALCUM_DAGGER, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public OrichalcumDagger(SaveDataTileObject data) { }
}