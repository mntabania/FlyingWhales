using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rumor : IReactable {
    public Character characterThatCreatedRumor { get; private set; }
    public Character targetCharacter { get; private set; }
    public IRumorable rumorable { get; private set; }

    #region getters
    public string name => rumorable.name;
    public string typeName => "Rumor";
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
    public string ReactionToActor(Character witness, REACTION_STATUS status) {
        return rumorable.ReactionToActor(witness, status);
    }
    public string ReactionToTarget(Character witness, REACTION_STATUS status) {
        return rumorable.ReactionToTarget(witness, status);
    }
    public string ReactionOfTarget(REACTION_STATUS status) {
        return rumorable.ReactionOfTarget(status);
    }
    public REACTABLE_EFFECT GetReactableEffect(Character witness) {
        return REACTABLE_EFFECT.Neutral;
    }
    public void AddAwareCharacter(Character character) {
        rumorable.AddAwareCharacter(character);
    }
    #endregion
}
