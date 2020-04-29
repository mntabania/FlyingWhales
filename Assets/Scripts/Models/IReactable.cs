using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReactable {
    string name { get; }
    string typeName { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    List<Character> awareCharacters { get; }
    string ReactionToActor(Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character witness, REACTION_STATUS status);
    string ReactionOfTarget(REACTION_STATUS status);
    void AddAwareCharacter(Character character);
    REACTABLE_EFFECT GetReactableEffect(Character witness);
}

public interface IRumorable {
    string name { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    List<Character> awareCharacters { get; }
    void SetAsRumor(Rumor rumor);
    void AddAwareCharacter(Character character);
    string ReactionToActor(Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character witness, REACTION_STATUS status);
    string ReactionOfTarget(REACTION_STATUS status);
}
