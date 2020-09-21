using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations.Settlements;

public class PartyQuest : ISavable {
    public string persistentID { get; private set; }
    public PARTY_QUEST_TYPE partyQuestType { get; protected set; }
    public int minimumPartySize { get; protected set; }
    public bool isWaitTimeOver { get; protected set; }
    public System.Type relatedBehaviour { get; protected set; }
    public Party assignedParty { get; protected set; }

    #region getters
    public virtual IPartyQuestTarget target => null;
    public virtual System.Type serializedData => typeof(SaveDataPartyQuest);
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party_Quest;
    public bool isAssigned => assignedParty != null;
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
        relatedBehaviour = System.Type.GetType(data.relatedBehaviour);
    }

    #region Virtuals
    public virtual void OnAcceptQuest(Party partyThatAcceptedQuest) { }
    public virtual void OnWaitTimeOver() {
        isWaitTimeOver = true;
    }
    protected virtual void OnEndQuest() { }
    public virtual void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        if(fromState == PARTY_STATE.Waiting && toState == PARTY_STATE.Moving) {
            OnWaitTimeOver();
        }
    }
    public virtual IPartyTargetDestination GetTargetDestination() { return null; }
    public virtual void OnRemoveMemberThatJoinedQuest(Character character) { }
    #endregion

    #region General
    public void SetAssignedParty(Party party) {
        if(assignedParty != party) {
            assignedParty = party;
        }
    }
    public void EndQuest() {
        OnEndQuest();
        assignedParty.DropQuest();
    }
    #endregion

    #region Loading
    public virtual void LoadReferences(SaveDataPartyQuest data) {
        if (!string.IsNullOrEmpty(data.assignedParty)) {
            assignedParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.assignedParty);
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

    public OBJECT_TYPE objectType => OBJECT_TYPE.Party_Quest;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        persistentID = data.persistentID;
        partyQuestType = data.partyQuestType;
        minimumPartySize = data.minimumPartySize;
        isWaitTimeOver = data.isWaitTimeOver;
        relatedBehaviour = data.relatedBehaviour.ToString();
        if(data.assignedParty != null) {
            assignedParty = data.assignedParty.persistentID;
        }
    }
    public override PartyQuest Load() {
        PartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(this);
        return quest;
    }
    #endregion
}