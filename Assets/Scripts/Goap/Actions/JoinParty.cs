
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class JoinParty : GoapAction {

    public JoinParty() : base(INTERACTION_TYPE.JOIN_PARTY) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON
        };
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Join Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget is Character partyLeader) {
            log.AddToFillers(partyLeader.partyComponent.currentParty, partyLeader.partyComponent.currentParty.partyName, LOG_IDENTIFIER.STRING_1);
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if(poiTarget is Character partyLeader) {
                return !partyLeader.isDead && partyLeader.partyComponent.hasParty;
            }
            return actor != poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterJoinSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character partyLeader) {
            partyLeader.partyComponent.currentParty.AddMember(goapNode.actor);
        }
    }
    #endregion

}