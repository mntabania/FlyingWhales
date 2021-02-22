using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Troll : Summon {
    public override string raceClassName => "Troll";

    public override bool defaultDigMode => true;

    public Troll() : base(SUMMON_TYPE.Troll, "Troll", RACE.TROLL, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Troll(string className) : base(SUMMON_TYPE.Troll, className, RACE.TROLL, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Troll(SaveDataSummon data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetAvoidSettlements(true);
        movementComponent.SetEnableDigging(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Troll_Behaviour);
        traitContainer.AddTrait(this, "Petrasol");
    }
    public override void OnSummonAsPlayerMonster() {
        base.OnSummonAsPlayerMonster();
        traitContainer.RemoveTrait(this, "Petrasol");
        movementComponent.SetAvoidSettlements(false);
    }
    public override void SubscribeToSignals() {
        if (hasSubscribedToSignals) {
            return;
        }
        base.SubscribeToSignals();
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void UnsubscribeSignals() {
        if (!hasSubscribedToSignals) {
            return;
        }
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    #endregion
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (character != this && currentStructure != null && !structure.isInterior && currentStructure.isInterior) {
            TIME_IN_WORDS timeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick(null);
            if (timeInWords != TIME_IN_WORDS.EARLY_NIGHT && timeInWords != TIME_IN_WORDS.LATE_NIGHT && timeInWords != TIME_IN_WORDS.AFTER_MIDNIGHT) {
                combatComponent.RemoveHostileInRange(character);
                ForceCancelAllJobsTargetingPOI(character, string.Empty);
            }
        }
    }
}