using Inner_Maps;
using Interrupts;
using UnityEngine;

public abstract class Nymph : Summon {

    public override string raceClassName => characterClass.className;
    
    private string _currentEffectSchedule;

    protected Nymph(SUMMON_TYPE summonType, string className) : base(summonType, className, RACE.NYMPH,
        UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    protected Nymph(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        //after a nymph has been initially placed, schedule it's effect after a random amount of minutes,
        //this is so that nymphs placed on the same tick will execute their effects at different times. 
        ScheduleAOEEffect(Random.Range(20, 40));
    }
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null,
        Log _deathLog = null, LogFiller[] deathLogFillers = null, Interrupt interrupt = null) {
        base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers, interrupt);
        CancelAOEEffect();
    }
    public override void OnSeizePOI() {
        base.OnSeizePOI();
        CancelAOEEffect();
    }
    public override void OnUnseizePOI(LocationGridTile tileLocation) {
        base.OnUnseizePOI(tileLocation);
        if (isDead == false) {
            ScheduleAOEEffect();    
        }
    }
    #endregion

    #region Effects
    private void ScheduleAOEEffect(int minutes = 20) {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(minutes));
        _currentEffectSchedule = SchedulingManager.Instance.AddEntry(dueDate, ExecuteAOEEffect, this);
    }
    private void CancelAOEEffect() {
        if (string.IsNullOrEmpty(_currentEffectSchedule) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_currentEffectSchedule);
            _currentEffectSchedule = string.Empty;
        }
    }
    private void ExecuteAOEEffect() {
        if (Random.Range(0, 100) < 25) {
            gridTileLocation.genericTileObject.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);
            for (int i = 0; i < gridTileLocation.neighbourList.Count; i++) {
                LocationGridTile tile = gridTileLocation.neighbourList[i];
                tile.genericTileObject.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);
            }
        }
        //reschedule after 20 minutes
        ScheduleAOEEffect();
    }
    #endregion
}
