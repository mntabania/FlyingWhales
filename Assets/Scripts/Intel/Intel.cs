using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using Logs;
using Crime_System;

public class ActionIntel : IIntel, IDisposable {
    public ActualGoapNode node { get; private set; }

    #region getters
    public IReactable reactable => node;
    public Log log => node.isAssumption? node.assumption.assumptionLog : node.descriptionLog;
    public Character actor => node.actor;
    public IPointOfInterest target => node.target;
    #endregion

    public ActionIntel(ActualGoapNode node) {
        this.node = node;
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, true);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
        node.SetIsIntel(true);
    }
    public ActionIntel(SaveDataActionIntel data) {
        node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.node);
    }

    #region IIntel
    public string GetIntelInfoRelationshipText() {
        string text = string.Empty;
        Character actor = this.actor;
        Character witness1 = null;
        Character witness2 = null;
        Character witness3 = null;

        Character affair = null;
        Character familyMember = null;
        Character closeFriend = null;
        Character friend = null;
        Character characterWithRelationship = null;
        for (int i = 0; i < actor.relationshipContainer.charactersWithOpinion.Count; i++) {
            Character target = actor.relationshipContainer.charactersWithOpinion[i];
            string opinionLabel = actor.relationshipContainer.GetOpinionLabel(target);

            //witness 1: randomly between prioritize lovers, affairs, familial relations or close friends (if none, then friends, if none then anyone with relationship)
            if (actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER)) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (witness1 == null) {
                        witness1 = target;
                    }
                }
            } else if (actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.AFFAIR)) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (affair == null) {
                        affair = target;
                    }
                }
            } else if (actor.relationshipContainer.IsFamilyMember(target)) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (familyMember == null) {
                        familyMember = target;
                    }
                }
            } else {
                if (opinionLabel == RelationshipManager.Close_Friend) {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (closeFriend == null) {
                            closeFriend = target;
                        }
                    }
                } else if (opinionLabel == RelationshipManager.Friend) {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (friend == null) {
                            friend = target;
                        }
                    }
                } else {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (characterWithRelationship == null) {
                            characterWithRelationship = target;
                        }
                    }
                }
            }
            //witness 2: acquaintance
            if (witness2 == null) {
                if (opinionLabel == RelationshipManager.Acquaintance) {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (target != affair && target != familyMember && target != closeFriend && target != friend && target != characterWithRelationship) {
                            witness2 = target;
                        }
                    }
                }
            }

            //witness 3: anyone else with a relationship
            if (witness3 == null) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (target != affair && target != familyMember && target != closeFriend && target != friend && target != characterWithRelationship) {
                        witness3 = target;
                    }
                }
            }
        }
        if (witness1 == null) {
            if (affair != null) { witness1 = affair; } 
            else if (familyMember != null) { witness1 = familyMember; }
            else if (closeFriend != null) { witness1 = closeFriend; } 
            else if (friend != null) { witness1 = friend; } 
            else if (characterWithRelationship != null) { witness1 = characterWithRelationship; }
        }

        List<EMOTION> emotions = ObjectPoolManager.Instance.CreateNewEmotionList();
        if (witness1 != null) {
            emotions.Clear();
            if(witness1 == target) {
                node.PopulateReactionsOfTarget(emotions, actor, target, REACTION_STATUS.INFORMED);
            } else {
                node.PopulateReactionsToActor(emotions, actor, target, witness1, REACTION_STATUS.INFORMED);
            }
            string response = GetFeelingTextInIntelInfoRelationshipText(emotions, witness1, actor);
            if (!string.IsNullOrEmpty(response)) {
                if (!string.IsNullOrEmpty(text)) {
                    text += "\n";
                }
                text += response;
            }
        }
        if (witness2 != null) {
            emotions.Clear();
            if (witness2 == target) {
                node.PopulateReactionsOfTarget(emotions, actor, target, REACTION_STATUS.INFORMED);
            } else {
                node.PopulateReactionsToActor(emotions, actor, target, witness2, REACTION_STATUS.INFORMED);
            }
            string response = GetFeelingTextInIntelInfoRelationshipText(emotions, witness2, actor);
            if (!string.IsNullOrEmpty(response)) {
                if (!string.IsNullOrEmpty(text)) {
                    text += "\n";
                }
                text += response;
            }
        }
        if (witness3 != null) {
            emotions.Clear();
            if (witness3 == target) {
                node.PopulateReactionsOfTarget(emotions, actor, target, REACTION_STATUS.INFORMED);
            } else {
                node.PopulateReactionsToActor(emotions, actor, target, witness3, REACTION_STATUS.INFORMED);
            }
            string response = GetFeelingTextInIntelInfoRelationshipText(emotions, witness3, actor);
            if (!string.IsNullOrEmpty(response)) {
                if (!string.IsNullOrEmpty(text)) {
                    text += "\n";
                }
                text += response;
            }
        }
        ObjectPoolManager.Instance.ReturnEmotionListToPool(emotions);
        return text;
    }
    public string GetIntelInfoBlackmailText() {
        string text = string.Empty;
        Character actor = this.actor;
        if (CanBeUsedToBlackmailCharacter(actor)) {
            if (node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset) {
                CrimeType crime = CrimeManager.Instance.GetCrimeType(node.crimeType);
                BLACKMAIL_TYPE blackmailType = GetBlackMailTypeConsideringTarget(actor);
                if (blackmailType != BLACKMAIL_TYPE.None) {
                    //Only show evidence hover if the actor's faction considers this intel as a crime
                    //Reason: It is confusing when the intel is being hovered and it says that it is an evidence but when you try to use it in Meddler, it does not show up as one of the blackmail materials since the faction does not consider it a crime
                    text = $"Blackmail material: Evidence that {actor.visuals.GetCharacterNameWithIconAndColor()} committed <b>{crime.name}</b> crime.";
                }
            }
        }
        return text;
    }
    private string GetFeelingTextInIntelInfoRelationshipText(List<EMOTION> emotions, Character witness, Character actor) {
        string response = string.Empty;
        if (emotions != null) {
            for (int i = 0; i < emotions.Count; i++) {
                if (i > 0) { response += " "; }
                response += CharacterManager.Instance.GetEmotionText(emotions[i]);
            }
        }
        if(response != string.Empty) {
            return $"May make {witness.visuals.GetCharacterNameWithIconAndColor()} feel {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(response, 2)} towards {actor.visuals.GetCharacterNameWithIconAndColor()}.";
        }
        return string.Empty;
    }
    private bool CanBeConsideredWitnessInIntelInfoRelationshipText(Character actor, Character witness1, Character witness2, Character witness3, Character target) {
        //Is Dead checking is due to bug https://trello.com/c/LdNxY1iL/3361-dead-people-show-up-in-the-new-quick-hint-tooltip-for-intel-the-one-that-tells-the-player-how-up-to-three-people-will-react-if-i
        return target != null && !target.isDead && target != actor && target != witness1 && target != witness2 && target != witness3;
    }
    public void OnIntelRemoved() {
        //set is intel in database to false, so that it can be overwritten.
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, false);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public bool CanBeUsedToBlackmailCharacter(Character p_target) {
        if (node.isAssumption) {
            return false;
        }
        if (p_target == actor) {
            BLACKMAIL_TYPE blackmailType = GetBlackMailTypeConsideringTarget(p_target);
            return blackmailType != BLACKMAIL_TYPE.None;
        }
        return false;
    }
    public BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(Character _targetCharacter) {
        CRIME_TYPE crimeType = reactable.crimeType;
        if(crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset && _targetCharacter.faction != null) {
            CRIME_SEVERITY severity = _targetCharacter.faction.GetCrimeSeverity(actor, target, crimeType);
            if(severity == CRIME_SEVERITY.Heinous) {
                return BLACKMAIL_TYPE.Strong;
            } else if (severity == CRIME_SEVERITY.Serious) {
                return BLACKMAIL_TYPE.Normal;
            } else if (severity == CRIME_SEVERITY.Misdemeanor || severity == CRIME_SEVERITY.Infraction) {
                return BLACKMAIL_TYPE.Weak;
            }
        }
        return BLACKMAIL_TYPE.None;
    }
    public string GetFullIntelTooltip() {
        string blackmailText = GetIntelInfoBlackmailText();
        string reactionText = GetIntelInfoRelationshipText();
        string text = string.Empty;

        text += blackmailText;
        if (!string.IsNullOrEmpty(text)) {
            text += "\n";
        }
        text += reactionText;

        if (!string.IsNullOrEmpty(text)) {
            return text;
        } else {
            return "Doesn't seem very useful, but...";   
        }
    }
    public bool CanShareIntelTo(Character p_target) {
        if (node.action.goapType == INTERACTION_TYPE.IS_IMPRISONED) {
            //Do not allow share on imprisoned target
            //Reference: https://trello.com/c/92nDGdD6/4019-blackmail-intel-recipient-issue
            return p_target != target;
        }
        return true;
    }
    #endregion

    #region Listeners
    private void OnCharacterChangedName(Character p_character) {
        if (node.descriptionLog.TryUpdateLogAfterRename(p_character)) {
            Messenger.Broadcast(UISignals.INTEL_LOG_UPDATED, this as IIntel);    
        }
    }
    #endregion

    #region Clean Up
    ~ActionIntel() {
        ReleaseUnmanagedResources();
    }
    private void ReleaseUnmanagedResources() {
        //release unmanaged resources here
        node = null;
    }
    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    #endregion
}
public class InterruptIntel : IIntel, IDisposable {
    //public Interrupt interrupt { get; private set; }
    //public Character actor { get; private set; }
    //public IPointOfInterest target { get; private set; }
    //public Log effectLog { get; private set; }
    public InterruptHolder interruptHolder { get; private set; }

    #region getters
    public IReactable reactable => interruptHolder;
    public Log log => interruptHolder.effectLog;
    public Character actor => interruptHolder.actor;
    public IPointOfInterest target => interruptHolder.target;
    #endregion

    public InterruptIntel(InterruptHolder interrupt) {
        interrupt.SetShouldNotBeObjectPooled(true);
        interruptHolder = interrupt;
        //interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
        //interruptHolder.Initialize(interrupt.interrupt, interrupt.actor, interrupt.target, interrupt.identifier, interrupt.reason);
        //interruptHolder.SetEffectLog(interrupt.effectLog);

        ////This is set because the interrupt intel must copy the data of the interrupt
        //interruptHolder.SetDisguisedActor(interrupt.disguisedActor);
        //interruptHolder.SetDisguisedTarget(interrupt.disguisedTarget);

        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, true);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public InterruptIntel(SaveDataInterruptIntel data) {
        interruptHolder = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.interruptHolder);
    }

    #region IIntel
    public string GetIntelInfoRelationshipText() {
        string text = string.Empty;
        Character actor = this.actor;
        Character witness1 = null;
        Character witness2 = null;
        Character witness3 = null;

        Character affair = null;
        Character familyMember = null;
        Character closeFriend = null;
        Character friend = null;
        Character characterWithRelationship = null;
        for (int i = 0; i < actor.relationshipContainer.charactersWithOpinion.Count; i++) {
            Character target = actor.relationshipContainer.charactersWithOpinion[i];
            string opinionLabel = actor.relationshipContainer.GetOpinionLabel(target);

            //witness 1: randomly between prioritize lovers, affairs, familial relations or close friends (if none, then friends, if none then anyone with relationship)
            if (actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER)) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (witness1 == null) {
                        witness1 = target;
                    }
                }
            } else if (actor.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.AFFAIR)) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (affair == null) {
                        affair = target;
                    }
                }
            } else if (actor.relationshipContainer.IsFamilyMember(target)) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (familyMember == null) {
                        familyMember = target;
                    }
                }
            } else {
                if (opinionLabel == RelationshipManager.Close_Friend) {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (closeFriend == null) {
                            closeFriend = target;
                        }
                    }
                } else if (opinionLabel == RelationshipManager.Friend) {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (friend == null) {
                            friend = target;
                        }
                    }
                } else {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (characterWithRelationship == null) {
                            characterWithRelationship = target;
                        }
                    }
                }
            }
            //witness 2: acquaintance
            if (witness2 == null) {
                if (opinionLabel == RelationshipManager.Acquaintance) {
                    if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                        if (target != affair && target != familyMember && target != closeFriend && target != friend && target != characterWithRelationship) {
                            witness2 = target;
                        }
                    }
                }
            }

            //witness 3: anyone else with a relationship
            if (witness3 == null) {
                if (CanBeConsideredWitnessInIntelInfoRelationshipText(actor, witness1, witness2, witness3, target)) {
                    if (target != affair && target != familyMember && target != closeFriend && target != friend && target != characterWithRelationship) {
                        witness3 = target;
                    }
                }
            }
        }
        if (witness1 == null) {
            if (affair != null) { witness1 = affair; } else if (familyMember != null) { witness1 = familyMember; } else if (closeFriend != null) { witness1 = closeFriend; } else if (friend != null) { witness1 = friend; } else if (characterWithRelationship != null) { witness1 = characterWithRelationship; }
        }

        List<EMOTION> emotions = ObjectPoolManager.Instance.CreateNewEmotionList();
        if (witness1 != null) {
            emotions.Clear();
            interruptHolder.PopulateReactionsToActor(emotions, actor, target, witness1, REACTION_STATUS.INFORMED);
            string response = GetFeelingTextInIntelInfoRelationshipText(emotions, witness1, actor);
            if (!string.IsNullOrEmpty(response)) {
                if (!string.IsNullOrEmpty(text)) {
                    text += "\n";
                }
                text += response;
            }
        }
        if (witness2 != null) {
            emotions.Clear();
            interruptHolder.PopulateReactionsToActor(emotions, actor, target, witness2, REACTION_STATUS.INFORMED);
            string response = GetFeelingTextInIntelInfoRelationshipText(emotions, witness2, actor);
            if (!string.IsNullOrEmpty(response)) {
                if (!string.IsNullOrEmpty(text)) {
                    text += "\n";
                }
                text += response;
            }
        }
        if (witness3 != null) {
            emotions.Clear();
            interruptHolder.PopulateReactionsToActor(emotions, actor, target, witness3, REACTION_STATUS.INFORMED);
            string response = GetFeelingTextInIntelInfoRelationshipText(emotions, witness3, actor);
            if (!string.IsNullOrEmpty(response)) {
                if (!string.IsNullOrEmpty(text)) {
                    text += "\n";
                }
                text += response;
            }
        }
        ObjectPoolManager.Instance.ReturnEmotionListToPool(emotions);
        return text;
    }
    public string GetIntelInfoBlackmailText() {
        string text = string.Empty;
        Character actor = this.actor;
        if (CanBeUsedToBlackmailCharacter(actor)) {
            if (interruptHolder.crimeType != CRIME_TYPE.None && interruptHolder.crimeType != CRIME_TYPE.Unset) {
                CrimeType crime = CrimeManager.Instance.GetCrimeType(interruptHolder.crimeType);
                text = $"Blackmail material: Evidence that {actor.visuals.GetCharacterNameWithIconAndColor()} committed <b>{crime.name}</b> crime.";
            }
        }
        return text;
    }
    private string GetFeelingTextInIntelInfoRelationshipText(List<EMOTION> emotions, Character witness, Character actor) {
        string response = string.Empty;
        if (emotions != null) {
            for (int i = 0; i < emotions.Count; i++) {
                if (i > 0) { response += " "; }
                response += CharacterManager.Instance.GetEmotionText(emotions[i]);
            }
        }
        if (response != string.Empty) {
            return $"May make {witness.visuals.GetCharacterNameWithIconAndColor()} feel {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(response, 2)} towards {actor.visuals.GetCharacterNameWithIconAndColor()}.";
        }
        return string.Empty;
    }
    private bool CanBeConsideredWitnessInIntelInfoRelationshipText(Character actor, Character witness1, Character witness2, Character witness3, Character target) {
        //Is Dead checking is due to bug https://trello.com/c/LdNxY1iL/3361-dead-people-show-up-in-the-new-quick-hint-tooltip-for-intel-the-one-that-tells-the-player-how-up-to-three-people-will-react-if-i
        return target != null && !target.isDead && target != actor && target != witness1 && target != witness2 && target != witness3;
    }
    public void OnIntelRemoved() {
        //set is intel in database to false, so that it can be overwritten.
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, false);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public bool CanBeUsedToBlackmailCharacter(Character p_target) {
        if (p_target == actor) {
            BLACKMAIL_TYPE blackmailType = GetBlackMailTypeConsideringTarget(p_target);
            return blackmailType != BLACKMAIL_TYPE.None;
        }
        return false;
    }
    public BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(Character _targetCharacter) {
        CRIME_TYPE crimeType = reactable.crimeType;
        if(crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset && _targetCharacter.faction != null) {
            CRIME_SEVERITY severity = _targetCharacter.faction.GetCrimeSeverity(actor, target, crimeType);
            if(severity == CRIME_SEVERITY.Heinous) {
                return BLACKMAIL_TYPE.Strong;
            } else if (severity == CRIME_SEVERITY.Serious) {
                return BLACKMAIL_TYPE.Normal;
            } else if (severity == CRIME_SEVERITY.Misdemeanor || severity == CRIME_SEVERITY.Infraction) {
                return BLACKMAIL_TYPE.Weak;
            }
        }
        return BLACKMAIL_TYPE.None;
    }
    public string GetFullIntelTooltip() {
        string blackmailText = GetIntelInfoBlackmailText();
        string reactionText = GetIntelInfoRelationshipText();
        string text = string.Empty;

        text += blackmailText;
        if (!string.IsNullOrEmpty(text)) {
            text += "\n";
        }
        text += reactionText;

        if (!string.IsNullOrEmpty(text)) {
            return text;
        } else {
            return "Doesn't seem very useful, but...";   
        }
    }
    public bool CanShareIntelTo(Character p_target) {
        return true;
    }
    #endregion

    #region Listeners
    private void OnCharacterChangedName(Character p_character) {
        if (interruptHolder.effectLog.TryUpdateLogAfterRename(p_character)) {
            Messenger.Broadcast(UISignals.INTEL_LOG_UPDATED, this as IIntel);    
        }
    }
    #endregion
    
    #region Clean Up
    ~InterruptIntel() {
        ReleaseUnmanagedResources();
    }
    private void ReleaseUnmanagedResources() {
        //release unmanaged resources here
        interruptHolder = null;
    }
    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    #endregion
}

public interface IIntel {
    IReactable reactable { get; }
    Log log { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
    string GetIntelInfoRelationshipText();
    string GetIntelInfoBlackmailText();

    /// <summary>
    /// Called whenever this intel is used up by the player or the notification it belongs to expires.
    /// </summary>
    void OnIntelRemoved();
    bool CanBeUsedToBlackmailCharacter(Character p_target);
    BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(Character p_target);
    string GetFullIntelTooltip();
    bool CanShareIntelTo(Character p_target);
}

[System.Serializable]
public class SaveDataActionIntel : SaveData<ActionIntel> {
    public string node;

    #region Overrides
    public override void Save(ActionIntel data) {
        node = data.node.persistentID;
        SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.node);
    }

    public override ActionIntel Load() {
        ActionIntel interrupt = new ActionIntel(this);
        return interrupt;
    }
    #endregion
}

[System.Serializable]
public class SaveDataInterruptIntel : SaveData<InterruptIntel> {
    public string interruptHolder;

    #region Overrides
    public override void Save(InterruptIntel data) {
        interruptHolder = data.interruptHolder.persistentID;
        SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.interruptHolder);
    }

    public override InterruptIntel Load() {
        InterruptIntel interrupt = new InterruptIntel(this);
        return interrupt;
    }
    #endregion
}
