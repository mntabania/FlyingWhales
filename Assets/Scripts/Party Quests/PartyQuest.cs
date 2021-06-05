﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations.Settlements;
using UtilityScripts;
public class PartyQuest : ISavable {
    public string persistentID { get; private set; }
    public PARTY_QUEST_TYPE partyQuestType { get; protected set; }
    public int minimumPartySize { get; protected set; }
    public bool isWaitTimeOver { get; protected set; }
    public System.Type relatedBehaviour { get; protected set; }
    public Party assignedParty { get; protected set; }
    public BaseSettlement madeInLocation { get; protected set; } //Where was this party quest created?
    public bool isSuccessful { get; protected set; }

    #region getters
    public virtual IPartyQuestTarget target => null;
    public virtual System.Type serializedData => typeof(SaveDataPartyQuest);
    public virtual bool waitingToWorkingStateImmediately => false;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party_Quest;
    public bool isAssigned => assignedParty != null;
    public virtual bool shouldAssignedPartyRetreatUponKnockoutOrKill => false;
    public virtual bool canStillJoinQuestAnytime => false;
    public virtual bool workingStateImmediately => false;
    #endregion

    public PartyQuest(PARTY_QUEST_TYPE partyType) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.partyQuestType = partyType;
    }

    public PartyQuest(SaveDataPartyQuest data) {
        persistentID = data.persistentID;
        partyQuestType = data.partyQuestType;
        minimumPartySize = data.minimumPartySize;
        isWaitTimeOver = data.isWaitTimeOver;
        isSuccessful = data.isSuccessful;
        relatedBehaviour = System.Type.GetType(data.relatedBehaviour);
    }

    #region Virtuals
    public virtual void OnAcceptQuest(Party partyThatAcceptedQuest) {
        if (shouldAssignedPartyRetreatUponKnockoutOrKill) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
        }
    }
    public virtual void OnAcceptQuestFromSaveData(Party partyThatAcceptedQuest) {
        if (shouldAssignedPartyRetreatUponKnockoutOrKill) {
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
        }
    }
    public virtual void OnWaitTimeOver() {
        isWaitTimeOver = true;
    }
    protected virtual void OnEndQuest() {
        if (shouldAssignedPartyRetreatUponKnockoutOrKill) {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
        }
        if (madeInLocation != null && madeInLocation is NPCSettlement npcSettlement) {
            npcSettlement.OnFinishedQuest(this);
        }
    }
    public virtual void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        if(fromState == PARTY_STATE.Waiting && toState == PARTY_STATE.Moving && !waitingToWorkingStateImmediately) {
            OnWaitTimeOver();
        } else if (fromState == PARTY_STATE.Waiting && toState == PARTY_STATE.Working && waitingToWorkingStateImmediately) {
            OnWaitTimeOver();
        }
    }
    public virtual IPartyTargetDestination GetTargetDestination() { return null; }
    public virtual void OnRemoveMemberThatJoinedQuest(Character character) { }
    public virtual string GetPartyQuestTextInLog() { return string.Empty; }
    public virtual void OnCharacterDeath(Character p_character) {
        if (shouldAssignedPartyRetreatUponKnockoutOrKill) {
            if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(p_character)) {
                if (GameUtilities.RollChance(assignedParty.chanceToRetreatUponKnockoutOrDeath)) {
                    EndQuest(p_character.name + " is dead");
                }
                assignedParty.SetChanceToRetreatUponKnockoutOrDeath(100);
            }
        }
    }
    #endregion

    #region General
    public void SetAssignedParty(Party party) {
        if(assignedParty == null) {
            assignedParty = party;
        } else if (!assignedParty.IsPartyTheSameAsThisParty(party)) {
            assignedParty = party;
        }
    }
    public void SetMadeInLocation(BaseSettlement settlement) {
        madeInLocation = settlement;
    }
    public void SetIsSuccessful(bool state) {
        isSuccessful = state;
    }
    public void EndQuest(string reason) {
        OnEndQuest();
        assignedParty.DropQuest(reason);
    }
    private void OnCharacterNoLongerPerform(Character character) {
        if (character.traitContainer.HasTrait("Unconscious")) {
            if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(character)) {
                if (GameUtilities.RollChance(assignedParty.chanceToRetreatUponKnockoutOrDeath)) {
                    EndQuest(character.name + " is incapacitated");
                } else {
                    assignedParty.SetChanceToRetreatUponKnockoutOrDeath(100);
                }
            }
        }
    }
    #endregion

    #region Loading
    public virtual void LoadReferences(SaveDataPartyQuest data) {
        if (!string.IsNullOrEmpty(data.assignedParty)) {
            assignedParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.assignedParty);
        }
        if (!string.IsNullOrEmpty(data.madeInLocation)) {
            madeInLocation = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.madeInLocation);
        }
    }
    #endregion
}

public class SaveDataPartyQuest : SaveData<PartyQuest>, ISavableCounterpart {
    public string persistentID { get; set; }
    public PARTY_QUEST_TYPE partyQuestType;
    public int minimumPartySize;
    public bool isWaitTimeOver;
    public string relatedBehaviour;
    public string assignedParty;
    public string madeInLocation;
    public bool isSuccessful;

    public OBJECT_TYPE objectType => OBJECT_TYPE.Party_Quest;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        persistentID = data.persistentID;
        partyQuestType = data.partyQuestType;
        minimumPartySize = data.minimumPartySize;
        isWaitTimeOver = data.isWaitTimeOver;
        isSuccessful = data.isSuccessful;
        relatedBehaviour = data.relatedBehaviour.ToString();
        if(data.assignedParty != null) {
            assignedParty = data.assignedParty.persistentID;
        }
        if(data.madeInLocation != null) {
            madeInLocation = data.madeInLocation.persistentID;
        }
    }
    public override PartyQuest Load() {
        PartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(this);
        return quest;
    }
    #endregion
}