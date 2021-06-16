using System;
using System.Collections.Generic;

public class PowerCrystal : TileObject {

    private string _destroySchedule;
    private GameDate _destroyDate;

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

        ScheduleExpiry();
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

    #region Expiry
    public void TryCancelExpiry() {
        if (String.IsNullOrEmpty(_destroySchedule) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_destroySchedule);
            _destroySchedule = string.Empty;
        }
    }
    public void ScheduleExpiry() {
        if (String.IsNullOrEmpty(_destroySchedule)) {
            _destroyDate = GameManager.Instance.Today();
            _destroyDate.AddTicks(20);
            // _destroyDate.AddTicks(3);
#if DEBUG_LOG
            UnityEngine.Debug.Log($"{nameWithID}'s marker will expire at {_destroyDate.ConvertToContinuousDaysWithTime()}");
#endif
            _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, null);
        }
    }
    private void ScheduleExpiry(GameDate gameDate) {
        if (String.IsNullOrEmpty(_destroySchedule)) {
            _destroyDate = gameDate;
            _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, null);
        }
    }
    private void TryExpire() {
        bool canExpire = true;
        if (isBeingCarriedBy != null) {
            canExpire = false;
        }
        if (isBeingSeized) {
            canExpire = false;
        }
        if (canExpire) {
            Expire();
        } else {
            //reschedule expiry to next hour.
            _destroyDate = GameManager.Instance.Today();
            _destroyDate.AddTicks(20);
            _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, null);
        }

    }
    private void Expire() {
#if DEBUG_LOG
        UnityEngine.Debug.Log($"{nameWithID}'s marker has expired.");
#endif

        gridTileLocation.structure.RemovePOI(this);
    }
    #endregion
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

