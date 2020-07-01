using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ZombieDeath : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public ZombieDeath() : base(INTERACTION_TYPE.ZOMBIE_DEATH) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Injured_Icon;
        advertisedBy = new[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Zombie Death Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 10;
    }
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData);
        if (satisfied) {
            return actor == target;
        }
        return false;
    }
    #endregion

    #region Effects
    public void AfterZombieDeathSuccess(ActualGoapNode goapNode) {
        goapNode.actor.Death("Zombie Death", goapNode, _deathLog: goapNode.descriptionLog);
    }
    #endregion
}

public class ZombieDeathData : GoapActionData {
    public ZombieDeathData() : base(INTERACTION_TYPE.ZOMBIE_DEATH) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }
}