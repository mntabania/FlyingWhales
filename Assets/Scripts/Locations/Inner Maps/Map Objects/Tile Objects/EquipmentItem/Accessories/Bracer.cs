using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bracer : AccessoryItem {
    public Bracer() {
        Initialize(TILE_OBJECT_TYPE.BRACER, false);

        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);

        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public Bracer(SaveDataTileObject data) { }
}