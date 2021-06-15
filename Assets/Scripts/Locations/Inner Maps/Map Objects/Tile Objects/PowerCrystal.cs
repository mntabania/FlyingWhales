using System;
using System.Collections.Generic;

public class PowerCrystal : TileObject {

    public override Type serializedData => typeof(SaveDataPowerCrystal);

	public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public float amountBonusResistance;
    public float amountBonusPiercing;
    public PowerCrystal() {
        Initialize(TILE_OBJECT_TYPE.POWER_CRYSTAL, true);

        maxHP = 1000;
        currentHP = maxHP;

        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL);
        if(UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(1, 100) > 50) {
            amountBonusPiercing = 5;
        } else {
            EquipmentBonusProcessor.SetBonusResistanceOnPowerCrystal(this, 1);
        }
    }
    public PowerCrystal(SaveDataTileObject data) : base(data) { }

    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataPowerCrystal powerCrystalSave = data as SaveDataPowerCrystal;
        if (powerCrystalSave != null) {
            powerCrystalSave.resistanceBonuses.ForEach(eachResistance => {
                resistanceBonuses.Add(eachResistance);
            });
            amountBonusPiercing = powerCrystalSave.bonusPiercing;
            amountBonusResistance = powerCrystalSave.bonusResistance;
        }
    }
}

#region Save Data
public class SaveDataPowerCrystal : SaveDataTileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public float bonusResistance;
    public float bonusPiercing;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        PowerCrystal powerCrystal = tileObject as PowerCrystal;
        //Assert.IsNotNull(equipment);
        bonusResistance = powerCrystal.amountBonusResistance;
        bonusPiercing = powerCrystal.amountBonusPiercing;
        powerCrystal.resistanceBonuses.ForEach((eachRes) => {
            resistanceBonuses.Add(eachRes);
        });
    }
}
#endregion

