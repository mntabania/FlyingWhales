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
    public string ReactionToActor(Character witness, REACTION_STATUS status) {
        return assumedAction.ReactionToActor(witness, status);
    }
    public string ReactionToTarget(Character witness, REACTION_STATUS status) {
        return assumedAction.ReactionToTarget(witness, status);
    }
    public string ReactionOfTarget(REACTION_STATUS status) {
        return assumedAction.ReactionOfTarget(status);
    }
    public REACTABLE_EFFECT GetReactableEffect(Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public void AddAwareCharacter(Character character) {
        assumedAction.AddAwareCharacter(character);
    }
    #endregion
}
