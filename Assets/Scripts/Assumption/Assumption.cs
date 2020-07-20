using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assumption : IReactable {
    public Character characterThatCreatedRumor { get; private set; }
    public Character targetCharacter { get; private set; }
    public ActualGoapNode assumedAction { get; private set; }

    #region getters
    public string name => assumedAction.name;
    public string classificationName => "Assumption";
    public Character actor => assumedAction.actor;
    public IPointOfInterest target => assumedAction.target;
    public Log informationLog => assumedAction.informationLog;
    public bool isStealth => assumedAction.isStealth;
    public List<Character> awareCharacters => assumedAction.awareCharacters;
    #endregion

    public Assumption(Character characterThatCreated, ActualGoapNode assumedAction) {
        characterThatCreatedRumor = characterThatCreated;
        targetCharacter = assumedAction.actor;
        this.assumedAction = assumedAction;
        this.assumedAction.SetAsAssumption(this);
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
