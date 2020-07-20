using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rumor : IReactable {
    public Character characterThatCreatedRumor { get; private set; }
    public Character targetCharacter { get; private set; }
    public IRumorable rumorable { get; private set; }

    #region getters
    public string name => rumorable.name;
    public string classificationName => "Rumor";
    public Character actor => rumorable.actor;
    public IPointOfInterest target => rumorable.target;
    public Log informationLog => rumorable.informationLog;
    public bool isStealth => rumorable.isStealth;
    public List<Character> awareCharacters => rumorable.awareCharacters;
    #endregion

    public Rumor(Character characterThatCreated, IRumorable rumorable) {
        characterThatCreatedRumor = characterThatCreated;
        targetCharacter = rumorable.actor;
        this.rumorable = rumorable;
        this.rumorable.SetAsRumor(this);
    }

    #region IReactable
    public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        return rumorable.ReactionToActor(actor, target, witness, status);
    }
    public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        REACTION_STATUS status) {
        return rumorable.ReactionToTarget(actor, target, witness, status);
    }
    public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status) {
        return rumorable.ReactionOfTarget(actor, target, status);
    }
    public REACTABLE_EFFECT GetReactableEffect(Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public void AddAwareCharacter(Character character) {
        rumorable.AddAwareCharacter(character);
    }
    #endregion
}
