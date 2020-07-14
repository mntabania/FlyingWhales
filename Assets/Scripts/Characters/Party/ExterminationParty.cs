using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ExterminationParty : Party {

    public LocationStructure targetStructure { get; private set; }
    public HexTile waitingArea { get; private set; }
    private bool isExterminating;
    private NPCSettlement originSettlement;

    #region getters
    public override IPartyTarget target => targetStructure;
    public override HexTile waitingHexArea => waitingArea;
    #endregion

    public ExterminationParty() : base(PARTY_TYPE.Extermination) {
        minimumPartySize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(2);
        relatedBehaviour = typeof(ExterminateBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        StartExterminationTimer();
    }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
        member.traitContainer.AddTrait(member, "Travelling");
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        if(originSettlement.exterminateTargetStructure == targetStructure) {
            originSettlement.SetExterminateTarget(null);
        }
    }
    #endregion

    #region General
    private void ProcessExterminationOrDisbandment() {
        if (!targetStructure.settlementLocation.HasAliveResidentInsideSettlement()) {
            DisbandParty();
        } else {
            StartExterminationTimer();
        }
    }
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            if(targetStructure != null) {
                SetWaitingArea();
            }
        }
    }
    public void SetOriginSettlement(NPCSettlement settlement) {
        originSettlement = settlement;
    }
    private void SetWaitingArea() {
        waitingArea = targetStructure.settlementLocation.GetAPlainAdjacentHextile();
    }
    #endregion

    #region Extermination Timer
    private void StartExterminationTimer() {
        if (!isExterminating) {
            isExterminating = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30));
            SchedulingManager.Instance.AddEntry(dueDate, DoneExterminationTimer, this);
        }
    }
    private void DoneExterminationTimer() {
        if (isExterminating) {
            isExterminating = false;
            ProcessExterminationOrDisbandment();
        }
    }
    #endregion
}
