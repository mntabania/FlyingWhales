using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations.Settlements;

public class Gathering : ISavable {
    public string persistentID { get; private set; }
    public Character host { get; protected set; }

    public GATHERING_TYPE gatheringType { get; protected set; }
    public string gatheringName { get; protected set; }
    public int waitTimeInTicks { get; protected set; }
    public int minimumGatheringSize { get; protected set; }
    public System.Type relatedBehaviour { get; protected set; }
    public JOB_OWNER jobQueueOwnerType { get; protected set; }
    public IJobOwner jobOwner { get; protected set; }

    public List<Character> attendees { get; protected set; } //includes the leader
    public bool isWaitTimeOver { get; protected set; }
    public bool isDisbanded { get; protected set; }
    public bool isAlreadyWaiting { get; private set; }

    #region getters
    public virtual IGatheringTarget target => null;
    public virtual Area waitingHexArea => null;
    public virtual System.Type serializedData => typeof(SaveDataGathering);
    public OBJECT_TYPE objectType => OBJECT_TYPE.Gathering;
    #endregion

    public Gathering(GATHERING_TYPE gatheringType) {
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        this.gatheringType = gatheringType;
        gatheringName = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(gatheringType.ToString());
        attendees = new List<Character>();
    }

    public Gathering(SaveDataGathering data) {
        attendees = new List<Character>();
        persistentID = data.persistentID;
        gatheringType = data.gatheringType;
        gatheringName = data.gatheringName;
        waitTimeInTicks = data.waitTimeInTicks;
        minimumGatheringSize = data.minimumGatheringSize;
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
    protected virtual void OnAddAttendee(Character member) {
        member.gatheringComponent.SetCurrentGathering(this);
        member.behaviourComponent.AddBehaviourComponent(relatedBehaviour);
        if (member == host) {
            host.AddAdvertisedAction(INTERACTION_TYPE.JOIN_GATHERING);
        }
        ProcessAdditionOfJoinGatheringJobs();
    }
    protected virtual void OnRemoveAttendee(Character member) {
        member.gatheringComponent.SetCurrentGathering(null);
        member.behaviourComponent.RemoveBehaviourComponent(relatedBehaviour);
        if (member == host) {
            host.RemoveAdvertisedAction(INTERACTION_TYPE.JOIN_GATHERING);
        }
    }
    protected virtual void OnRemoveAttendeeOnDisband(Character member) {
        member.gatheringComponent.SetCurrentGathering(null);
        member.behaviourComponent.RemoveBehaviourComponent(relatedBehaviour);
        if (member == host) {
            host.RemoveAdvertisedAction(INTERACTION_TYPE.JOIN_GATHERING);
        }
        member.jobQueue.CancelAllGatheringJobs();

    }
    protected virtual void OnDisbandGathering() {
        // Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "General", "disband", null, LOG_TAG.Party);
        // log.AddToFillers(host, host.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        // log.AddToFillers(null, gatheringName, LOG_IDENTIFIER.STRING_1);
        //log.AddLogToDatabase();

#if DEBUG_LOG
        host.logComponent.PrintLogIfActive("Disbanded " + gatheringName + " Gathering of " + host.name);
#endif
        isDisbanded = true;
        CancelAllJoinPartyJobs();
    }
    protected virtual void OnBeforeDisbandGathering() { }
    protected virtual void OnWaitTimeOver() { }
    protected virtual void OnWaitTimeOverButGatheringIsDisbanded() { }
    protected virtual void OnSetHost() {
        if (host != null) {
            if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
                jobOwner = host.homeSettlement;
            } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
                jobOwner = host.faction;
            }
            StartWaitTime();
            AddAttendee(host);
        }
    }
#endregion

#region General
    public void SetHost(Character newHost) {
        if (host != newHost) {
            if (host != null) {
                RemoveAttendee(host);
            }
            host = newHost;
            OnSetHost();
        }
    }
    public bool AddAttendee(Character character) {
        if (!attendees.Contains(character)) {
            attendees.Add(character);
            OnAddAttendee(character);
            return true;
        }
        return false;
    }
    public bool RemoveAttendee(Character character) {
        if (attendees.Remove(character)) {
            OnRemoveAttendee(character);
            if (attendees.Count <= 0) {
                DisbandGathering();
            }
            return true;
        }
        return false;
    }
    public void DisbandGathering() {
        if (isDisbanded) { return; }
        OnBeforeDisbandGathering();
        for (int i = 0; i < attendees.Count; i++) {
            OnRemoveAttendeeOnDisband(attendees[i]);
        }
        attendees.Clear();
        OnDisbandGathering();
    }
    public bool IsHost(Character character) {
        return host == character;
    }
    public bool IsAttendee(Character character) {
        return attendees.Contains(character);
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
            if (attendees.Count < minimumGatheringSize) {
                //Disband party if minimum party size is not reached by the time wait time is over
                DisbandGathering();
                OnWaitTimeOverButGatheringIsDisbanded();
            } else {
                OnWaitTimeOver();
            }
            isWaitTimeOver = true;
        }
    }
#endregion

#region Join Gathering
    private void ProcessAdditionOfJoinGatheringJobs() {
        if (attendees.Count < (minimumGatheringSize + 2) && !isWaitTimeOver) {
            CreateJoinGatheringJob();
        }
    }

    private void CreateJoinGatheringJob() {
        if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            (jobOwner as NPCSettlement).settlementJobTriggerComponent.TriggerJoinGatheringJob(this);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            (jobOwner as Faction).factionJobTriggerComponent.TriggerJoinGatheringJob(this);
        }
    }
    private void CancelAllJoinPartyJobs() {
        if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            jobOwner.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_GATHERING, host);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            jobOwner.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_GATHERING, host);
        }
    }
#endregion

#region Loading
    public virtual void LoadReferences(SaveDataGathering data) {
        host = CharacterManager.Instance.GetCharacterByPersistentID(data.host);
        if (jobQueueOwnerType == JOB_OWNER.CHARACTER) {
            jobOwner = CharacterManager.Instance.GetCharacterByPersistentID(data.jobOwner);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            jobOwner = FactionManager.Instance.GetFactionByPersistentID(data.jobOwner);
        } else if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            jobOwner = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.jobOwner) as NPCSettlement;
        } else if (jobQueueOwnerType == JOB_OWNER.PARTY) {
            jobOwner = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.jobOwner);
        }
        for (int i = 0; i < data.attendees.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.attendees[i]);
            if (character != null) {
                attendees.Add(character);
            }
        }
    }
#endregion
}

[System.Serializable]
public class SaveDataGathering : SaveData<Gathering>, ISavableCounterpart {
    public string persistentID { get; set; }
    public string host;

    public GATHERING_TYPE gatheringType;
    public string gatheringName;
    public int waitTimeInTicks;
    public int minimumGatheringSize;
    public string relatedBehaviour;
    public JOB_OWNER jobQueueOwnerType;
    public string jobOwner;

    public List<string> attendees;
    public bool isWaitTimeOver;
    public bool isDisbanded;
    public bool isAlreadyWaiting;

#region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
#endregion

#region Overrides
    public override void Save(Gathering data) {
        persistentID = data.persistentID;
        host = data.host.persistentID;
        gatheringType = data.gatheringType;
        gatheringName = data.gatheringName;
        waitTimeInTicks = data.waitTimeInTicks;
        minimumGatheringSize = data.minimumGatheringSize;
        relatedBehaviour = data.relatedBehaviour.ToString();
        jobQueueOwnerType = data.jobQueueOwnerType;
        jobOwner = data.jobOwner.persistentID;
        isWaitTimeOver = data.isWaitTimeOver;
        isDisbanded = data.isDisbanded;
        isAlreadyWaiting = data.isAlreadyWaiting;

        attendees = new List<string>();
        for (int i = 0; i < data.attendees.Count; i++) {
            attendees.Add(data.attendees[i].persistentID);
        }
    }

    public override Gathering Load() {
        Gathering gathering = CharacterManager.Instance.CreateNewGathering(this);
        return gathering;
    }
#endregion
}