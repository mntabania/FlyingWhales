using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : TileObject {
    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public EquipmentData equipmentData;

    public EquipmentItem() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
    }
}
