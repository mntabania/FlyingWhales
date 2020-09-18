using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations.Settlements;

public class Party : ISavable {
    public string persistentID { get; private set; }
    public Character leader { get; protected set; }

    public PARTY_TYPE partyType { get; protected set; }
    public string partyName { get; protected set; }
    public int waitTimeInTicks { get; protected set; }
    public int minimumPartySize { get; protected set; }
    public System.Type relatedBehaviour { get; protected set; }
    public JOB_OWNER jobQueueOwnerType { get; protected set; }
    public IJobOwner jobOwner { get; protected set; }

    public List<Character> members { get; protected set; } //includes the leader
    public bool isWaitTimeOver { get; protected set; }
    public bool isDisbanded { get; protected set; }
    public bool isAlreadyWaiting { get; private set; }

    #region getters
    public virtual IPartyTarget target => null;
    public virtual HexTile waitingHexArea => null;
    public virtual System.Type serializedData => typeof(SaveDataParty);
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
    #endregion

    public Party(PARTY_TYPE partyType) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.partyType = partyType;
        partyName = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(partyType.ToString());
        members = new List<Character>();
    }

    public Party(SaveDataParty data) {
        members = new List<Character>();
        persistentID = data.persistentID;
        partyType = data.partyType;
        partyName = data.partyName;
        waitTimeInTicks = data.waitTimeInTicks;
        minimumPartySize = data.minimumPartySize;
        relatedBehaviour = System.Type.GetType(data.relatedBehaviour);
        jobQueueOwnerType = data.jobQueueOwnerType;

        isWaitTimeOver = data.isWaitTimeOver;
        isDisbanded = data.isDisbanded;
        isAlreadyWaiting = data.isAlreadyWaiting;
    }

    #region Virtuals
    public virtual bool IsAllowedToJoin(Character character) {
        return true;
    }
    protected virtual void OnAddMember(Character member) {
        member.partyComponent.SetCurrentParty(this);
        member.behaviourComponent.AddBehaviourComponent(relatedBehaviour);
        if (member == leader) {
            leader.AddAdvertisedAction(INTERACTION_TYPE.JOIN_PARTY);
        }
        ProcessAdditionOfJoinPartyJobs();
    }
    protected virtual void OnRemoveMember(Character member) {
        member.partyComponent.SetCurrentParty(null);
        member.behaviourComponent.RemoveBehaviourComponent(relatedBehaviour);
        if (member == leader) {
            leader.RemoveAdvertisedAction(INTERACTION_TYPE.JOIN_PARTY);
        }
    }
    protected virtual void OnRemoveMemberOnDisband(Character member) {
        member.partyComponent.SetCurrentParty(null);
        member.behaviourComponent.RemoveBehaviourComponent(relatedBehaviour);
        if (member == leader) {
            leader.RemoveAdvertisedAction(INTERACTION_TYPE.JOIN_PARTY);
        }
        member.jobQueue.CancelAllPartyJobs();

    }
    protected virtual void OnDisbandParty() {
        Log log = new Log(GameManager.Instance.Today(), "Party", "General", "disband");
        log.AddToFillers(leader, leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, partyName, LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();

        leader.logComponent.PrintLogIfActive("Disbanded " + partyName + " Party of " + leader.name);
        isDisbanded = true;
        CancelAllJoinPartyJobs();
    }
    protected virtual void OnBeforeDisbandParty() { }
    protected virtual void OnWaitTimeOver() { }
    protected virtual void OnWaitTimeOverButPartyIsDisbanded() { }
    protected virtual void OnSetLeader() {
        if(leader != null) {
            if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
                jobOwner = leader.homeSettlement;
            } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
                jobOwner = leader.faction;
            }
            StartWaitTime();
            AddMember(leader);
        }
    }
    #endregion

    #region General
    public void SetLeader(Character newLeader) {
        if (leader != newLeader) {
            if (leader != null) {
                RemoveMember(leader);
            }
            leader = newLeader;
            OnSetLeader();
        }
    }
    public bool AddMember(Character character) {
        if (!members.Contains(character)) {
            members.Add(character);
            OnAddMember(character);
            return true;
        }
        return false;
    }
    public bool RemoveMember(Character character) {
        if (members.Remove(character)) {
            OnRemoveMember(character);
            if(members.Count <= 0) {
                DisbandParty();
            }
            return true;
        }
        return false;
    }
    public void DisbandParty() {
        if (isDisbanded) { return; }
        OnBeforeDisbandParty();
        for (int i = 0; i < members.Count; i++) {
            OnRemoveMemberOnDisband(members[i]);
        }
        members.Clear();
        OnDisbandParty();
    }
    public bool IsLeader(Character character) {
        return leader == character;
    }
    public bool IsMember(Character character) {
        return members.Contains(character);
    }
    public Character GetMemberInCombatExcept(Character character) {
        for (int i = 0; i < members.Count; i++) {
            Character member = members[i];
            if(member != character) {
                if (member.combatComponent.isInCombat) {
                    return member;
                }
            }
        }
        return null;
    }
    #endregion

    #region Wait Time
    public void StartWaitTime() {
        if (!isWaitTimeOver && !isAlreadyWaiting) {
            isAlreadyWaiting = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(waitTimeInTicks);
            SchedulingManager.Instance.AddEntry(dueDate, ProcessWaiting, this);
        }
    }
    private void ProcessWaiting() {
        if (!isWaitTimeOver) {
            if(members.Count < minimumPartySize) {
                //Disband party if minimum party size is not reached by the time wait time is over
                DisbandParty();
                OnWaitTimeOverButPartyIsDisbanded();
            } else {
                OnWaitTimeOver();
            }
            isWaitTimeOver = true;
        }
    }
    #endregion

    #region Join Party
    private void ProcessAdditionOfJoinPartyJobs() {
        if(members.Count < (minimumPartySize + 2) && !isWaitTimeOver) {
            CreateJoinPartyJob();
        }
    }

    private void CreateJoinPartyJob() {
        if(jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            (jobOwner as NPCSettlement).settlementJobTriggerComponent.TriggerJoinPartyJob(this);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            (jobOwner as Faction).factionJobTriggerComponent.TriggerJoinPartyJob(this);
        }
    }
    private void CancelAllJoinPartyJobs() {
        if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            jobOwner.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_PARTY, leader);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            jobOwner.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_PARTY, leader);
        }
    }
    #endregion

    #region Loading
    public virtual void LoadReferences(SaveDataParty data) {
        leader = CharacterManager.Instance.GetCharacterByPersistentID(data.leader);
        if(jobQueueOwnerType == JOB_OWNER.CHARACTER) {
            jobOwner = CharacterManager.Instance.GetCharacterByPersistentID(data.jobOwner);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            jobOwner = FactionManager.Instance.GetFactionByPersistentID(data.jobOwner);
        } else if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            jobOwner = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.jobOwner) as NPCSettlement;
        }
        for (int i = 0; i < data.members.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.members[i]);
            members.Add(character);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataParty : SaveData<Party>, ISavableCounterpart {
    public string persistentID { get; set; }
    public string leader;

    public PARTY_TYPE partyType;
    public string partyName;
    public int waitTimeInTicks;
    public int minimumPartySize;
    public string relatedBehaviour;
    public JOB_OWNER jobQueueOwnerType;
    public string jobOwner;

    public List<string> members;
    public bool isWaitTimeOver;
    public bool isDisbanded;
    public bool isAlreadyWaiting;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
    #endregion

    #region Overrides
    public override void Save(Party data) {
        persistentID = data.persistentID;
        leader = data.leader.persistentID;
        partyType = data.partyType;
        partyName = data.partyName;
        waitTimeInTicks = data.waitTimeInTicks;
        minimumPartySize = data.minimumPartySize;
        relatedBehaviour = data.relatedBehaviour.ToString();
        jobQueueOwnerType = data.jobQueueOwnerType;
        jobOwner = data.jobOwner.persistentID;
        isWaitTimeOver = data.isWaitTimeOver;
        isDisbanded = data.isDisbanded;
        isAlreadyWaiting = data.isAlreadyWaiting;

        members = new List<string>();
        for (int i = 0; i < data.members.Count; i++) {
            members.Add(data.members[i].persistentID);
        }
    }

    public override Party Load() {
        Party party = CharacterManager.Instance.CreateNewParty(this);
        return party;
    }
    #endregion
}