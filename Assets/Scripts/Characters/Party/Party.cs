using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party {
    public Character leader { get; protected set; }

    public PARTY_TYPE partyType { get; protected set; }
    public string partyName { get; protected set; }
    public int waitTimeInTicks { get; protected set; }
    public int minimumPartySize { get; protected set; }
    public System.Type relatedBehaviour { get; protected set; }
    public JOB_OWNER jobQueueOwnerType { get; protected set; }

    public List<Character> members { get; protected set; } //includes the leader
    public bool isWaitTimeOver { get; protected set; }
    public bool isDisbanded { get; protected set; }
    private bool isAlreadyWaiting;

    #region getters
    public virtual IPartyTarget target => null;
    public virtual HexTile waitingHexArea => null;
    #endregion

    public Party(PARTY_TYPE partyType) {
        this.partyType = partyType;
        partyName = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(partyType.ToString());
        members = new List<Character>();
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
    protected virtual void OnDisbandParty() {
        leader.logComponent.PrintLogIfActive("Disbanded " + partyName + " Party of " + leader.name);
        isDisbanded = true;
        CancelAllJoinPartyJobs();
    }
    protected virtual void OnBeforeDisbandParty() { }
    protected virtual void OnWaitTimeOver() { }
    protected virtual void OnWaitTimeOverButPartyIsDisbanded() { }
    #endregion

    #region General
    public void SetLeader(Character newLeader) {
        if (leader != newLeader) {
            if (leader != null) {
                RemoveMember(leader);
            }
            leader = newLeader;
            if (leader != null) {
                StartWaitTime();
                AddMember(leader);
            }
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
            OnRemoveMember(members[i]);
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
            leader.homeSettlement.settlementJobTriggerComponent.TriggerJoinPartyJob(this);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            leader.faction.factionJobTriggerComponent.TriggerJoinPartyJob(this);
        }
    }
    private void CancelAllJoinPartyJobs() {
        if (jobQueueOwnerType == JOB_OWNER.SETTLEMENT) {
            leader.homeSettlement.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_PARTY, leader);
        } else if (jobQueueOwnerType == JOB_OWNER.FACTION) {
            leader.faction.ForceCancelJobTypesTargetingPOI(JOB_TYPE.JOIN_PARTY, leader);
        }
    }
    #endregion
}
