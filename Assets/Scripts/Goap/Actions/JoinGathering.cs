
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;

public class JoinGathering : GoapAction {

    public JoinGathering() : base(INTERACTION_TYPE.JOIN_GATHERING) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON, RACE.RATMAN
        };
        doesNotStopTargetCharacter = true;
        logTags = new[] {LOG_TAG.Party};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Join Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget is Character partyLeader) {
            log.AddToFillers(null, partyLeader.gatheringComponent.currentGathering.gatheringName, LOG_IDENTIFIER.STRING_1); //partyLeader.partyComponent.currentParty
        }
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if(poiTarget is Character partyLeader) {
                return !partyLeader.isDead && partyLeader.gatheringComponent.hasGathering; //&& partyLeader.partyComponent.hasParty;
            }
            return actor != poiTarget;
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterJoinSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character partyLeader) {
            partyLeader.gatheringComponent.currentGathering.AddAttendee(goapNode.actor);
            // partyLeader.partyComponent.currentParty?.AddMember(goapNode.actor);
        }
    }
#endregion

}