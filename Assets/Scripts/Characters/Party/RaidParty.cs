using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class RaidParty : Party {

    public BaseSettlement targetSettlement { get; private set; }
    //public LocationStructure targetStructure { get; private set; }
    public HexTile waitingArea { get; private set; }
    private bool isRaiding;

    #region getters
    public override IPartyTarget target => targetSettlement;
    public override HexTile waitingHexArea => waitingArea;
    #endregion

    public RaidParty() : base(PARTY_TYPE.Raid) {
        minimumPartySize = 4;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(RaidBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        for (int i = 0; i < members.Count; i++) {
            members[i].traitContainer.AddTrait(members[i], "Travelling");
        }
        StartRaidTimer();
    }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnRemoveMemberOnDisband(Character member) {
        base.OnRemoveMemberOnDisband(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        //TODO: notif reason why raid party disbanded
    }
    #endregion

    #region General
    private void ProcessRaidOrDisbandment() {
        if (!targetSettlement.HasAliveResidentInsideSettlement()) {
            DisbandParty();
        } else {
            StartRaidTimer();
        }
    }
    public void SetTargetSettlement(BaseSettlement settlement) {
        if(targetSettlement != settlement) {
            targetSettlement = settlement;
            if (targetSettlement != null) {
                SetWaitingArea();
            }
        }
    }
    private void SetWaitingArea() {
        waitingArea = targetSettlement.GetAPlainAdjacentHextile();
    }
    #endregion

    #region Raid Timer
    private void StartRaidTimer() {
        if (!isRaiding) {
            isRaiding = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30));
            SchedulingManager.Instance.AddEntry(dueDate, DoneRaidTimer, this);
        }
    }
    private void DoneRaidTimer() {
        if (isRaiding) {
            isRaiding = false;
            ProcessRaidOrDisbandment();
        }
    }
    #endregion
}
