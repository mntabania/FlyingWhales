using System;
using System.Collections.Generic;

public class PowerCrystal : TileObject {

    public bool hasScheduleDestruction { get; private set; }
    public GameDate destroyDate { get; private set; }
    public override string description => GetDescription();
    public override Type serializedData => typeof(SaveDataPowerCrystal);

	public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public float amountBonusResistance;
    public float amountBonusPiercing;
    public PowerCrystal() {
        Initialize(TILE_OBJECT_TYPE.POWER_CRYSTAL, true);

        maxHP = 1000;
        currentHP = maxHP;

        AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL);
        if(UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(1, 100) <= 30) {
            amountBonusPiercing = 5;
        } else {
            amountBonusResistance = 10;
            EquipmentBonusProcessor.SetBonusResistanceOnPowerCrystal(this, 1);
        }

    }
    public PowerCrystal(SaveDataPowerCrystal data) : base(data) {
        hasScheduleDestruction = data.hasScheduleDestruction;
        destroyDate = data.destroyDate;
        data.resistanceBonuses.ForEach(eachResistance => {
            resistanceBonuses.Add(eachResistance);
        });
        amountBonusPiercing = data.bonusPiercing;
        amountBonusResistance = data.bonusResistance;

        if (hasScheduleDestruction) {
            SchedulingManager.Instance.AddEntry(destroyDate, TryExpire, null);
        }
    }

    string GetDescription() {
        //elementLbl.text = UtilityScripts.Utilities.GetRichTextIconForElement(_activeCharacter.combatComponent.elementalDamage.type) + $"{_activeCharacter.combatComponent.elementalDamage.type}";
        float count = amountBonusPiercing;
        string element = string.Empty;
        element = UtilityScripts.Utilities.PiercingIcon();
        if (amountBonusPiercing <= 0f) {
            element = UtilityScripts.Utilities.GetRichTextIconForElement(resistanceBonuses[0].GetElement());
            count = amountBonusResistance;
        }
        
        return $"When absorbed by an Elf, provides +{count}%{element}to all Elves in their Village.";
    }

    public override void OnPlacePOI() {
        base.OnPlacePOI();
        ScheduleExpiry();
    }

    #region Expiry
    public void ScheduleExpiry() {
        if (!hasScheduleDestruction) {
            hasScheduleDestruction = true;
            destroyDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
#if DEBUG_LOG
            UnityEngine.Debug.Log($"{nameWithID} will expire at {destroyDate.ConvertToContinuousDaysWithTime()}");
#endif
            SchedulingManager.Instance.AddEntry(destroyDate, TryExpire, null);
        }
    }
    //private void ScheduleExpiry(GameDate gameDate) {
    //    if (String.IsNullOrEmpty(_destroySchedule)) {
    //        _destroyDate = gameDate;
    //        _destroySchedule = SchedulingManager.Instance.AddEntry(_destroyDate, TryExpire, null);
    //    }
    //}
    private void TryExpire() {
        bool canExpire = true;
        if (isBeingSeized) {
            canExpire = false;
        }
        if (canExpire) {
            Expire();
        } else {
            //reschedule expiry to next hour.
            hasScheduleDestruction = false;
            ScheduleExpiry();
        }

    }
    private void Expire() {
#if DEBUG_LOG
        UnityEngine.Debug.Log($"{nameWithID} has expired.");
#endif
        if (isBeingCarriedBy != null) {
            isBeingCarriedBy.DropItem(this);
        }
        if (gridTileLocation != null) {
            gridTileLocation.structure.RemovePOI(this);
        }
        hasScheduleDestruction = false;
    }
    #endregion expiry
    
    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (actor.race == RACE.ELVES) {
            if (!HasJobTargetingThis(JOB_TYPE.ABSORB_CRYSTAL)) {
                if (!actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.ABSORB_CRYSTAL)) {
                    actor.jobComponent.TriggerAbsorbPowerCrystal(this);
                }
            }
        }
    }

    #endregion
}

#region Save Data
public class SaveDataPowerCrystal : SaveDataTileObject {

    public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();
    public float bonusResistance;
    public float bonusPiercing;
    public bool hasScheduleDestruction;
    public GameDate destroyDate;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        PowerCrystal powerCrystal = tileObject as PowerCrystal;
        //Assert.IsNotNull(equipment);
        bonusResistance = powerCrystal.amountBonusResistance;
        bonusPiercing = powerCrystal.amountBonusPiercing;
        powerCrystal.resistanceBonuses.ForEach((eachRes) => {
            resistanceBonuses.Add(eachRes);
        });
        hasScheduleDestruction = powerCrystal.hasScheduleDestruction;
        destroyDate = powerCrystal.destroyDate;
    }
}
#endregion

