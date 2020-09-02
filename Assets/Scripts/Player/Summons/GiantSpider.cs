using System;
using Inner_Maps;
using Traits;

public class GiantSpider : Summon {

    public const string ClassName = "Giant Spider";
    
    public override string raceClassName => $"Giant Spider";
    public override bool defaultDigMode => true;

    public GiantSpider() : base(SUMMON_TYPE.Giant_Spider, ClassName, RACE.SPIDER, UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public GiantSpider(string className) : base(SUMMON_TYPE.Giant_Spider, className, RACE.SPIDER, UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public GiantSpider(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        //behaviourComponent.RemoveBehaviourComponent(typeof(DefaultMonster));
        // behaviourComponent.AddBehaviourComponent(typeof(AbductorMonster));
        movementComponent.SetAvoidSettlements(true);
        movementComponent.SetEnableDigging(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Giant_Spider_Behaviour);
    }
    public override void OnSummonAsPlayerMonster() {
        base.OnSummonAsPlayerMonster();
        movementComponent.SetAvoidSettlements(false);
    }
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
        Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnAddedJobToQueue);
        Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnRemovedJobFromQueue);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
        Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_ADDED_TO_QUEUE, OnAddedJobToQueue);
        Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnRemovedJobFromQueue);
    }
    #endregion

    #region Listeners
    private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob job) {
        if (character == this && job.jobType == JOB_TYPE.MONSTER_ABDUCT) {
            job.targetPOI.traitContainer.AddTrait(job.targetPOI, "Webbed", this);
        }
    }
    private void OnRemovedJobFromQueue(JobQueueItem job, Character character) {
        if (character == this && job.jobType == JOB_TYPE.MONSTER_ABDUCT) {
            if (character is Summon summon) {
                character.combatComponent.SetCombatMode(summon.defaultCombatMode);    
            } else {
                character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
            }
            
        }
    }
    private void OnAddedJobToQueue(JobQueueItem job, Character character) {
        if (character == this && job.jobType == JOB_TYPE.MONSTER_ABDUCT && character.combatComponent.combatMode != COMBAT_MODE.Defend) {
            character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        }
    }
    #endregion
}