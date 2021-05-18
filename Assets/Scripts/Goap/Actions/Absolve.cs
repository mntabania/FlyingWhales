using Traits;
using System.Collections.Generic;

public class Absolve : GoapAction {
    public Absolve() : base(INTERACTION_TYPE.ABSOLVE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Absolve Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion
    
#region State Effects
    public void AfterAbsolveSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        if (target.traitContainer.HasTrait("Criminal")) {
            Criminal criminalTrait = target.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
            criminalTrait.SetIsImprisoned(false);
        }
        target.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(target.faction, CRIME_STATUS.Absolved, goapNode.actor);
        target.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);
        //target.traitContainer.RemoveTrait(target, "Criminal", goapNode.actor);
        target.traitContainer.RemoveRestrainAndImprison(target, goapNode.actor);
    }
#endregion
}