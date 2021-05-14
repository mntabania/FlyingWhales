using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MinkShirt : ArmorItem {
    public MinkShirt() {
        Initialize(TILE_OBJECT_TYPE.MINK_SHIRT, false);

        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);

        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MinkShirt(SaveDataTileObject data) { }
}