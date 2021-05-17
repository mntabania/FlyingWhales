
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class HostSocialParty : GoapAction {

    public HostSocialParty() : base(INTERACTION_TYPE.HOST_SOCIAL_PARTY) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] {
        //    RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
        //    RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
        //    RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON
        //};
        logTags = new[] {LOG_TAG.Party};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Host Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return !actor.partyComponent.hasParty;
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterHostSuccess(ActualGoapNode goapNode) {
        Gathering gathering = CharacterManager.Instance.CreateNewGathering(GATHERING_TYPE.Social, goapNode.actor);
        (gathering as SocialGathering).SetTargetStructure(goapNode.actor.homeSettlement.GetFirstStructureOfTypeWithNoActiveSocialParty(STRUCTURE_TYPE.TAVERN));
        //Party party = CharacterManager.Instance.CreateNewParty(PARTY_QUEST_TYPE.Social, goapNode.actor);
        //(party as SocialParty).SetTargetStructure(goapNode.actor.homeSettlement.GetFirstStructureOfTypeWithNoActiveSocialParty(STRUCTURE_TYPE.TAVERN)); 
    }
#endregion

}