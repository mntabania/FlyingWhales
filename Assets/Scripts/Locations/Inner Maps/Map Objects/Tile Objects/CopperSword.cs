using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CopperSword : WeaponItem {
    public CopperSword() {
        Initialize(TILE_OBJECT_TYPE.COPPER_SWORD, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);

        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
    }
    public CopperSword(SaveDataTileObject data) { }
}