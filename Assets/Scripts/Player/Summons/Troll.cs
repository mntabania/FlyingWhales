using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Troll : Summon {
    public override string raceClassName => "Troll";

    public bool isAwakened { get; private set; }
    public bool isAttackingPlayer { get; private set; }
    public bool willLeaveWorld { get; private set; }
    public LocationStructure targetStructure { get; private set; }

    public Troll() : base(SUMMON_TYPE.Troll, "Troll", RACE.TROLL, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Troll(string className) : base(SUMMON_TYPE.Troll, className, RACE.TROLL, UtilityScripts.Utilities.GetRandomGender()) {
    }
    public Troll(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetAvoidSettlements(true);
        movementComponent.SetEnableDigging(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Troll_Behaviour);
    }
    public override void OnSummonAsPlayerMonster() {
        movementComponent.SetAvoidSettlements(false);
    }
    protected override void OnTickStarted() {
        base.OnTickStarted();
        CheckBecomeStone();
    }
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    #endregion

    private void CheckBecomeStone() {
        if (!currentStructure.isInterior) {
            TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick(null);
            if (timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                traitContainer.RemoveTrait(this, "Stoned");
            } else {
                if (!traitContainer.HasTrait("Stoned")) {
                    traitContainer.AddTrait(this, "Stoned");
                }
            }
        }
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (character != this && !structure.isInterior && currentStructure.isInterior) {
            TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick(null);
            if(timeInWords != TIME_IN_WORDS.EARLY_NIGHT && timeInWords != TIME_IN_WORDS.LATE_NIGHT && timeInWords != TIME_IN_WORDS.AFTER_MIDNIGHT) {
                combatComponent.RemoveHostileInRange(character);
            }
        }
    }
}