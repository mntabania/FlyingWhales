using Inner_Maps;
using Interrupts;
using UnityEngine;
using UnityEngine.Profiling;

public abstract class Nymph : Summon {
    
    private string _currentEffectSchedule;
    public override string raceClassName => characterClass.className;
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;
    
    protected Nymph(SUMMON_TYPE summonType, string className) : base(summonType, className, RACE.NYMPH, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    protected Nymph(SaveDataSummon data) : base(data) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        ScheduleAOEEffect(UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(12, 24)); //Did not save _currentEffectSchedule, instead redo schedule upon loading because we cannot save schedules right now
    }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        //after a nymph has been initially placed, schedule it's effect after a random amount of minutes,
        //this is so that nymphs placed on the same tick will execute their effects at different times. 
        ScheduleAOEEffect(UtilityScripts.GameUtilities.RandomBetweenTwoNumbers(12, 24));
    }
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null,
        Log _deathLog = default, LogFillerStruct[] deathLogFillers = null, Interrupt interrupt = null, bool isPlayerSource = false) {
        base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers, interrupt, isPlayerSource);
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
    private void ScheduleAOEEffect(int minutes = 12) {
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
#if DEBUG_PROFILER
            Profiler.BeginSample($"Nymph - Adjust HP Current Tile");
#endif
            gridTileLocation.tileObjectComponent.genericTileObject.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif

#if DEBUG_PROFILER
            Profiler.BeginSample($"Nymph - Adjust HP Neighbours");
#endif
            for (int i = 0; i < gridTileLocation.neighbourList.Count; i++) {
                LocationGridTile tile = gridTileLocation.neighbourList[i];
                tile.tileObjectComponent.genericTileObject.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);
            }
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
#if DEBUG_PROFILER
        Profiler.BeginSample($"Nymph - Schedule AOE Effect");
#endif
        //reschedule after 20 minutes
        ScheduleAOEEffect();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataCharacter data) {
        base.LoadReferences(data);
        if (isDead == false) {
            ScheduleAOEEffect();
        }
    }
#endregion
}
