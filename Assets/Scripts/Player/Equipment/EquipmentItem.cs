﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : TileObject {
    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public EquipmentData equipmentData;

    public override System.Type serializedData => typeof(SaveDataEquipmentItem);

    public void AssignData() {
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
    }

    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataEquipmentItem saveDataEquipment = data as SaveDataEquipmentItem;
        if (saveDataEquipment != null) {
            saveDataEquipment.resistanceBonuses.ForEach(eachResistance => {
                resistanceBonuses.Add(eachResistance);
            });
        }
    }

    public EquipmentItem() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
    }

    #region Save Data
    public class SaveDataEquipmentItem : SaveDataTileObject {

        public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
        public override void Save(TileObject tileObject) {
            base.Save(tileObject);
            EquipmentItem equipment = tileObject as EquipmentItem;
            //Assert.IsNotNull(equipment);
            equipment.resistanceBonuses.ForEach((eachRes) => {
                resistanceBonuses.Add(eachRes);
            }); 
        }
    }
    #endregion
}
