using System;
using System.Collections.Generic;
using Interrupts;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Ghost : Summon {
    public Character betrayedBy { get; private set; }

    public override string raceClassName => characterClass.className;
    public Ghost() : base(SUMMON_TYPE.Ghost, "Ghost", RACE.GHOST,
        UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public Ghost(string className) : base(SUMMON_TYPE.Ghost, className, RACE.GHOST,
    UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
    }
    public Ghost(SaveDataCharacter data) : base(data) {
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_ENDED_STATE, OnCharacterEndedState);
    }
    #endregion

    public void SetBetrayedBy(Character character) {
        betrayedBy = character;
    }
    
    private void OnCharacterStartedState(Character character, CharacterState state) {
        if (character == this && state.characterState == CHARACTER_STATE.COMBAT) {
            Messenger.AddListener(Signals.TICK_ENDED, FearCheck);    
        }
    }
    private void OnCharacterEndedState(Character character, CharacterState state) {
        if (character == this && state.characterState == CHARACTER_STATE.COMBAT) {
            Messenger.RemoveListener(Signals.TICK_ENDED, FearCheck);    
        }
    }
    
    private void FearCheck() {
        if (UtilityScripts.Utilities.IsEven(GameManager.Instance.Today().tick)) {
            if (UnityEngine.Random.Range(0, 100) < 15) {
                //cast fear on random hostile
                List<Character> choices = new List<Character>();
                for (int i = 0; i < combatComponent.hostilesInRange.Count; i++) {
                    IPointOfInterest poi = combatComponent.hostilesInRange[i];
                    if (poi is Character character && character.marker.hasFleePath == false && 
                        (character.interruptComponent.isInterrupted == false || 
                         character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Cowering)) {
                        choices.Add(character);
                    }
                }
                for (int i = 0; i < combatComponent.avoidInRange.Count; i++) {
                    IPointOfInterest poi = combatComponent.avoidInRange[i];
                    if (poi is Character character && character.marker.hasFleePath == false && 
                        (character.interruptComponent.isInterrupted == false || 
                         character.interruptComponent.currentInterrupt.interrupt.type != INTERRUPT.Cowering)) {
                        choices.Add(character);
                    }
                }
                if (choices.Count > 0) {
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(choices);
                    chosenCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Feared, this);
                    Log log = new Log(GameManager.Instance.Today(), "Summon", "Ghost", "feared");
                    log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(chosenCharacter, chosenCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToInvolvedObjects();
                }
            }
        }
    }
}
