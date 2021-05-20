using System.Collections.Generic;

public class PowerCrystal : TileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public int bonusResistance;
    public int bonusPiercing;
    public PowerCrystal() {
        Initialize(TILE_OBJECT_TYPE.POWER_CRYSTAL, true);

        maxHP = 1000;
        currentHP = maxHP;

        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL);
        bonusPiercing = UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(1, 10);
        EquipmentBonusProcessor.SetBonusResistanceOnPowerCrystal(this);
    }

    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDatapowerCrystalItem powerCrystalSave = data as SaveDatapowerCrystalItem;
        if (powerCrystalSave != null) {
            powerCrystalSave.resistanceBonuses.ForEach(eachResistance => {
                resistanceBonuses.Add(eachResistance);
            });
            bonusPiercing = powerCrystalSave.bonusPiercing;
            bonusResistance = powerCrystalSave.bonusResistance;
        }
    }

    
}

#region Save Data
public class SaveDatapowerCrystalItem : SaveDataTileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public int bonusResistance;
    public int bonusPiercing;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        PowerCrystal powerCrystal = tileObject as PowerCrystal;
        //Assert.IsNotNull(equipment);
        bonusResistance = powerCrystal.bonusResistance;
        bonusPiercing = powerCrystal.bonusPiercing;
        powerCrystal.resistanceBonuses.ForEach((eachRes) => {
            resistanceBonuses.Add(eachRes);
        });
    }
}
#endregion

