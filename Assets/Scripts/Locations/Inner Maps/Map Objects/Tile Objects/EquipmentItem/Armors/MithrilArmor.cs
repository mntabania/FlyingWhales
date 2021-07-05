using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MithrilArmor : ArmorItem {
    public MithrilArmor() {
        Initialize(TILE_OBJECT_TYPE.MITHRIL_ARMOR, false);
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
    }
    public MithrilArmor(SaveDataTileObject data) { }
}