using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FurShirt : ArmorItem {
    public FurShirt() {
        Initialize(TILE_OBJECT_TYPE.FUR_SHIRT, false);

        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);

        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public FurShirt(SaveDataTileObject data) { }
}