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
    string ReactionToActor(Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character witness, REACTION_STATUS status);
    string ReactionOfTarget(REACTION_STATUS status);
    REACTABLE_EFFECT GetReactableEffect();
}

public interface IRumorable {
    string name { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    void SetAsRumor(Rumor rumor);
    string ReactionToActor(Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character witness, REACTION_STATUS status);
    string ReactionOfTarget(REACTION_STATUS status);
}
