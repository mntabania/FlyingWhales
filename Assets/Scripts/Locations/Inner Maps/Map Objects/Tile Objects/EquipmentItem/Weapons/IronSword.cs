using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IronSword : WeaponItem {
    public IronSword() {
        Initialize(TILE_OBJECT_TYPE.IRON_SWORD, false);

        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);

        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public IronSword(SaveDataTileObject data) { }
}