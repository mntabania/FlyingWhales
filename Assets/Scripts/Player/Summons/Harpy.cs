﻿using Inner_Maps;
using Traits;

public class Harpy : Summon {

    public const string ClassName = "Harpy";
    
    public override string raceClassName => "Harpy";

    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

    public bool hasCapturedForTheDay { get; private set; }

    public Harpy() : base(SUMMON_TYPE.Harpy, ClassName, RACE.HARPY, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Harpy(string className) : base(SUMMON_TYPE.Harpy, className, RACE.HARPY, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Harpy(SaveDataSummon data) : base(data) {
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetEnableDigging(true);
        movementComponent.SetTagAsTraversable(InnerMapManager.Obstacle_Tag);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Harpy_Behaviour);
    }
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<Character, GoapPlanJob>(JobSignals.CHARACTER_WILL_DO_JOB, OnCharacterWillDoJob);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
        Messenger.AddListener<Character>(CharacterSignals.HEALTH_CRITICALLY_LOW, OnHealthCriticallyLow);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, GoapPlanJob>(JobSignals.CHARACTER_WILL_DO_JOB, OnCharacterWillDoJob);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
        Messenger.RemoveListener<Character>(CharacterSignals.HEALTH_CRITICALLY_LOW, OnHealthCriticallyLow);
    }
    #endregion

    #region Listeners
    private void OnCharacterWillDoJob(Character character, GoapPlanJob job) {
        if (character == this) {
            if (job.jobType == JOB_TYPE.CAPTURE_CHARACTER) {
                combatComponent.SetCombatMode(COMBAT_MODE.Passive);
            }
        }
    }
    private void OnJobRemovedFromQueue(JobQueueItem job, Character character) {
        if(character == this) {
            if(job.jobType == JOB_TYPE.CAPTURE_CHARACTER) {
                IPointOfInterest target = job.poiTarget;
                if(target != null) {
                    Prisoner prisoner = target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                    if(prisoner != null && prisoner.prisonerOfCharacter == this) {
                        target.traitContainer.RemoveRestrainAndImprison(target);
                    }
                }
                combatComponent.SetCombatMode(defaultCombatMode);
            }
        }
    }
    private void OnHealthCriticallyLow(Character character) {
        if(character == this) {
            if(character.currentJob != null && character.currentJob.jobType == JOB_TYPE.CAPTURE_CHARACTER) {
                character.jobQueue.CancelFirstJob();
            }
        }
    }
    #endregion

    public void SetHasCapturedForTheDay(bool state) {
        if(hasCapturedForTheDay != state) {
            hasCapturedForTheDay = state;
            if (hasCapturedForTheDay) {
                GameDate dueDate = GameManager.Instance.Today().AddDays(1);
                SchedulingManager.Instance.AddEntry(dueDate, () => SetHasCapturedForTheDay(false), this);
            }
        }
    }
}

