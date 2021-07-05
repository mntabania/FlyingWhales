using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

public class Rumor : IReactable {
    //Rumors are not ISavable because we do not need a persistent ID for them
    //Rumors are saved under SaveDataInterruptHolder or SaveDataActualGoapNode
    //Since they are the only ones who has a reference to rumors, we do not need to put them in the hub to conserve save space
    //This also means that we do not need to save the IRumorable because their parent (SaveDataInterruptHolder or SaveDataActualGoapNode) are the ones responsible for setting it

    public Character characterThatCreatedRumor { get; private set; }
    public Character targetCharacter { get; private set; }
    public IRumorable rumorable { get; private set; }

    #region getters
    public string name => rumorable.name;
    public string classificationName => "Rumor";
    public Character actor => rumorable.actor;
    public IPointOfInterest target => rumorable.target;
    public Character disguisedActor => rumorable.disguisedActor;
    public Character disguisedTarget => rumorable.disguisedTarget;
    public Log informationLog => rumorable.informationLog;
    public bool isStealth => rumorable.isStealth;
    public CRIME_TYPE crimeType => rumorable.crimeType;
    public List<Character> awareCharacters => rumorable.awareCharacters;
    public List<LOG_TAG> logTags => rumorable.logTags;
    #endregion

    public Rumor(Character characterThatCreated, Character targetCharacter) {
        characterThatCreatedRumor = characterThatCreated;
        this.targetCharacter = targetCharacter;
    }
    
    public void SetRumorable(IRumorable rumorable) {
        this.rumorable = rumorable;
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
    public void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        rumorable.PopulateReactionsToActor(reactions, actor, target, witness, status);
    }
    public void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status) {
        rumorable.PopulateReactionsToTarget(reactions, actor, target, witness, status);
    }
    public void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status) {
        rumorable.PopulateReactionsOfTarget(reactions, actor, target, status);
    }
    public REACTABLE_EFFECT GetReactableEffect(Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public void AddAwareCharacter(Character character) {
        rumorable.AddAwareCharacter(character);
    }
    #endregion
}


[System.Serializable]
public class SaveDataRumor : SaveData<Rumor> {
    public string characterThatCreatedRumorID;
    public string targetCharacterID;

    #region Overrides
    public override void Save(Rumor data) {
        characterThatCreatedRumorID = data.characterThatCreatedRumor.persistentID;
        targetCharacterID = data.targetCharacter.persistentID;
    }

    public override Rumor Load() {
        Character characterThatCreatedRumor = CharacterManager.Instance.GetCharacterByPersistentID(characterThatCreatedRumorID);
        Character targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(targetCharacterID);
        Rumor rumor = new Rumor(characterThatCreatedRumor, targetCharacter);
        return rumor;
    }
    #endregion
}