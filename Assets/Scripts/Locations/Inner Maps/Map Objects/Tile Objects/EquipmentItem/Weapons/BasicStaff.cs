﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BasicStaff : WeaponItem {
    public BasicStaff() {
        Initialize(TILE_OBJECT_TYPE.BASIC_STAFF, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public BasicStaff(SaveDataTileObject data) { }
}