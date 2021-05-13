using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperSword : WeaponItem {
    public CopperSword() {
        Initialize(TILE_OBJECT_TYPE.COPPER_SWORD, false);
        
        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
    }
    public CopperSword(SaveDataTileObject data) { }
}