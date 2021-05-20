using System.Collections.Generic;

public class PowerCrystal : TileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public float amountBonusResistance;
    public float amountBonusPiercing;
    public PowerCrystal() {
        Initialize(TILE_OBJECT_TYPE.POWER_CRYSTAL, true);

        maxHP = 1000;
        currentHP = maxHP;

        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL);
        if(UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(0, 100) > 50) {
            amountBonusPiercing = 5;
        } else {
            amountBonusResistance = 10;
            EquipmentBonusProcessor.SetBonusResistanceOnPowerCrystal(this, 1);
        }
    }

    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDatapowerCrystalItem powerCrystalSave = data as SaveDatapowerCrystalItem;
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
public class SaveDatapowerCrystalItem : SaveDataTileObject {

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

