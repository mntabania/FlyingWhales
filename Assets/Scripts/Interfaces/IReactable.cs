using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

public interface IReactable {
    string name { get; }
    string classificationName { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Character disguisedActor { get; }
    Character disguisedTarget { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    CRIME_TYPE crimeType { get; }
    List<Character> awareCharacters { get; }
    List<LOG_TAG> logTags { get; }
    string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status);
    void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status);
    void AddAwareCharacter(Character character);
    REACTABLE_EFFECT GetReactableEffect(Character witness);
}

public interface IRumorable {
    string persistentID { get; }
    OBJECT_TYPE objectType { get; }
    string name { get; }
    RUMOR_TYPE rumorType { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    Character disguisedActor { get; }
    Character disguisedTarget { get; }
    Log informationLog { get; }
    bool isStealth { get; }
    CRIME_TYPE crimeType { get; }
    List<Character> awareCharacters { get; }
    List<LOG_TAG> logTags { get; }
    void SetAsRumor(Rumor rumor);
    void AddAwareCharacter(Character character);
    string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status);
    void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);
    void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status);
}
