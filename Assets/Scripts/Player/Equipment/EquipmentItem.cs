using UtilityScripts;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : TileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public EQUIPMENT_QUALITY quality = EQUIPMENT_QUALITY.Normal;
    public EquipmentData equipmentData;

    public List<EQUIPMENT_BONUS> addedBonus = new List<EQUIPMENT_BONUS>();
    public EQUIPMENT_SLAYER_BONUS randomSlayerBonus = EQUIPMENT_SLAYER_BONUS.None;
    public EQUIPMENT_WARD_BONUS randomWardBonus = EQUIPMENT_WARD_BONUS.None;
    public override System.Type serializedData => typeof(SaveDataEquipmentItem);

    public void AssignData() {
        equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(this.name);
        if (equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Random_Ward_Bonus)) {
            if (randomWardBonus == EQUIPMENT_WARD_BONUS.None) {
                randomWardBonus = (EQUIPMENT_WARD_BONUS)GameUtilities.RandomBetweenTwoNumbers(1, (int)EQUIPMENT_WARD_BONUS.Demon_Ward);
                addedBonus.Add(EQUIPMENT_BONUS.Ward_Bonus);
            }
        }
        if (equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Random_Slayer_Bonus)) {
            if (randomSlayerBonus == EQUIPMENT_SLAYER_BONUS.None) {
                randomSlayerBonus = (EQUIPMENT_SLAYER_BONUS)GameUtilities.RandomBetweenTwoNumbers(1, (int)EQUIPMENT_SLAYER_BONUS.Demon_Slayer);
                addedBonus.Add(EQUIPMENT_BONUS.Slayer_Bonus);
            }
        }
        if (randomSlayerBonus != EQUIPMENT_SLAYER_BONUS.None) { 
            if(equipmentData.equipmentUpgradeData.slayerBonus == EQUIPMENT_SLAYER_BONUS.None) {
                equipmentData.equipmentUpgradeData.slayerBonus = randomSlayerBonus;
            }
        }
        if (randomWardBonus != EQUIPMENT_WARD_BONUS.None) {
            if (equipmentData.equipmentUpgradeData.wardBonus == EQUIPMENT_WARD_BONUS.None) {
                equipmentData.equipmentUpgradeData.wardBonus = randomWardBonus;
            }
        }
    }

    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataEquipmentItem saveDataEquipment = data as SaveDataEquipmentItem;
        if (saveDataEquipment != null) {
            saveDataEquipment.resistanceBonuses.ForEach(eachResistance => {
                resistanceBonuses.Add(eachResistance);
            });
            randomSlayerBonus = saveDataEquipment.randomSlayerBonus;
            randomWardBonus = saveDataEquipment.randomWardBonus;
            saveDataEquipment.addedBonus.ForEach((eachBonus) => addedBonus.Add(eachBonus));
            AssignData();
        }
    }
    //this is for testing purpose only OnPlacePOI()
    public void MakeQualityHigh() {
        quality = EQUIPMENT_QUALITY.High;
        traitContainer.AddTrait(this, "High Quality");
        maxHP += (int)(maxHP * 0.5f);
        currentHP = (int)Mathf.Clamp(currentHP + (maxHP * 0.5f), 0, maxHP);
    }

    public void MakeQualityPremium() {
        quality = EQUIPMENT_QUALITY.Premium;
        maxHP += (int)(maxHP * 2f);
        traitContainer.AddTrait(this, "Premium");
        currentHP = (int)Mathf.Clamp(currentHP + (maxHP * 2f), 0, maxHP);
    }

    public EquipmentItem() {
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.SCRAP);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_EQUIPMENT);
    }

	public override void OnPlacePOI() {
		base.OnPlacePOI();
    }

    public string GetBonusDescription() {
        if (equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Random_Slayer_Bonus) && randomSlayerBonus == EQUIPMENT_SLAYER_BONUS.None) {
            AssignData();
        }
        if (equipmentData.equipmentUpgradeData.bonuses.Contains(EQUIPMENT_BONUS.Random_Ward_Bonus) && randomWardBonus == EQUIPMENT_WARD_BONUS.None) {
            AssignData();
        }
        string description = equipmentData.equipmentUpgradeData.GetBonusDescription(quality);
        if (randomSlayerBonus != EQUIPMENT_SLAYER_BONUS.None) {
            description += randomSlayerBonus.ToString().Replace("_", " ");
        }
        if (randomWardBonus != EQUIPMENT_WARD_BONUS.None) {
            description += randomWardBonus.ToString().Replace("_", " ");
        }
        //description += "\nQuality " + quality;
        description += equipmentData.equipmentUpgradeData.GetDescriptionForRandomResistance(resistanceBonuses, quality);
        return description;
    }

    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (this.currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && (this.characterOwner == null || this.characterOwner == actor) && actor.equipmentComponent.EvaluateNewEquipment(this, actor)) {
            if (!actor.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM)) {
                actor.jobComponent.CreateTakeItemJob(JOB_TYPE.TAKE_ITEM, this);
            }
        }
    }
    #endregion

    #region Save Data
    public class SaveDataEquipmentItem : SaveDataTileObject {

        public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
        public EQUIPMENT_SLAYER_BONUS randomSlayerBonus = EQUIPMENT_SLAYER_BONUS.None;
        public EQUIPMENT_WARD_BONUS randomWardBonus = EQUIPMENT_WARD_BONUS.None;
        public List<EQUIPMENT_BONUS> addedBonus = new List<EQUIPMENT_BONUS>();
        public override void Save(TileObject tileObject) {
            base.Save(tileObject);
            EquipmentItem equipment = tileObject as EquipmentItem;
            //Assert.IsNotNull(equipment);
            equipment.resistanceBonuses.ForEach((eachRes) => {
                resistanceBonuses.Add(eachRes);
            });
            randomSlayerBonus = equipment.randomSlayerBonus;
            randomWardBonus = equipment.randomWardBonus;
            equipment.addedBonus.ForEach((eachBonus) => addedBonus.Add(eachBonus));
        }
    }
    #endregion
}