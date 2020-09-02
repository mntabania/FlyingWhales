﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assumption : IReactable {
    //Same as Rumor - See Rumor comments

    public Character characterThatCreatedAssumption { get; private set; }
    public Character targetCharacter { get; private set; }
    public ActualGoapNode assumedAction { get; private set; }

    #region getters
    public string name => assumedAction.name;
    public string classificationName => "Assumption";
    public Character actor => assumedAction.actor;
    public IPointOfInterest target => assumedAction.target;
    public Character disguisedActor => assumedAction.disguisedActor;
    public Character disguisedTarget => assumedAction.disguisedTarget;
    public Log informationLog => assumedAction.informationLog;
    public bool isStealth => assumedAction.isStealth;
    public List<Character> awareCharacters => assumedAction.awareCharacters;
    #endregion

    public Assumption(Character characterThatCreated, Character targetCharacter) {
        characterThatCreatedAssumption = characterThatCreated;
        this.targetCharacter = targetCharacter;
    }

    public void SetAssumedAction(ActualGoapNode assumedAction) {
        this.assumedAction = assumedAction;
    }

    #region IReactable
    public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        return assumedAction.ReactionToActor(actor, target, witness, status);
    }
    public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        REACTION_STATUS status) {
        return assumedAction.ReactionToTarget(actor, target, witness, status);
    }
    public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status) {
        return assumedAction.ReactionOfTarget(actor, target, status);
    }
    public REACTABLE_EFFECT GetReactableEffect(Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public void AddAwareCharacter(Character character) {
        assumedAction.AddAwareCharacter(character);
    }
    #endregion
}

[System.Serializable]
public class SaveDataAssumption : SaveData<Assumption> {
    public string characterThatCreatedAssumptionID;
    public string targetCharacterID;

    #region Overrides
    public override void Save(Assumption data) {
        characterThatCreatedAssumptionID = data.characterThatCreatedAssumption.persistentID;
        targetCharacterID = data.targetCharacter.persistentID;
    }

    public override Assumption Load() {
        Character characterThatCreatedAssumption = CharacterManager.Instance.GetCharacterByPersistentID(characterThatCreatedAssumptionID);
        Character targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(targetCharacterID);
        Assumption rumor = new Assumption(characterThatCreatedAssumption, targetCharacter);
        return rumor;
    }
    #endregion
}