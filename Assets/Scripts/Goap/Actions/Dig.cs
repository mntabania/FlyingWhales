using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;

public class Dig : GoapAction {
    public Dig() : base(INTERACTION_TYPE.DIG) {
        actionIconString = GoapActionStateDB.Bury_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON
        };
        canBePerformedEvenIfPathImpossible = true;
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Dig Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 0;
    }
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData);
        if (satisfied) {
            return target.gridTileLocation != null;
        }
        return false;
    }
    #endregion
    
    #region State Effects
    [UsedImplicitly]
    public void AfterDigSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.gridTileLocation.structure.RemovePOI(goapNode.poiTarget);
    }
    #endregion
}