using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : TileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public EQUIPMENT_QUALITY quality = EQUIPMENT_QUALITY.Normal;
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
    //this is for testing purpose only OnPlacePOI()
    public void MakeQualityHigh() {
        quality = EQUIPMENT_QUALITY.High;
        maxHP += (int)(maxHP * 0.5f);
    }

    public void MakeQualityPremium() {
        quality = EQUIPMENT_QUALITY.Premium;
        maxHP += (int)(maxHP * 2f);
    }

    public EquipmentItem() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_EQUIPMENT);

        maxHP = 700;
        currentHP = maxHP;
    }

    public string GetBonusDescription() {
        if(equipmentData == null) {
            AssignData();
        }
        string description = equipmentData.equipmentUpgradeData.GetBonusDescription(quality);
        description += "\nQuality " + quality;
        resistanceBonuses.ForEach((eachBonus) => description += ("\n" + eachBonus.ToString()));
        return description;
    }

    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (this.currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && this.characterOwner == null && actor.equipmentComponent.EvaluateNewEquipment(this, actor)) {
            actor.jobComponent.CreateTakeItemJob(JOB_TYPE.TAKE_ITEM, this);
        }
    }
    #endregion

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