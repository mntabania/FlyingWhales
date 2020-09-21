using Traits;

public class Exile : GoapAction {
    public Exile() : base(INTERACTION_TYPE.EXILE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Life_Changes};
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Exile Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region State Effects
    public void AfterExileSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        if (target.traitContainer.HasTrait("Criminal")) {
            Criminal criminalTrait = target.traitContainer.GetNormalTrait<Criminal>("Criminal");
            criminalTrait.SetIsImprisoned(false);
            criminalTrait.RemoveAllCrimesWantedBy(goapNode.actor.faction);
        }
        //target.traitContainer.RemoveTrait(target, "Criminal", goapNode.actor);
        target.traitContainer.RemoveTrait(target, "Restrained", goapNode.actor);
        Faction oldFaction = target.faction;
        oldFaction.KickOutCharacter(target);
        target.MigrateHomeStructureTo(null);
        target.ClearTerritory();
    }
    #endregion
}
