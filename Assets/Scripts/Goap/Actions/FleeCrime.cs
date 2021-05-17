public class FleeCrime : GoapAction {
    public FleeCrime() : base(INTERACTION_TYPE.FLEE_CRIME) {
        actionIconString = GoapActionStateDB.Flee_Icon;
        //actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
        //    RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
        //    RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
        shouldAddLogs = false;
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Flee Success", goapNode);
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
    public void AfterFleeSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        actor.traitContainer.RemoveTrait(actor, "Criminal");

        //If this criminal character is being apprehended and survived (meaning he did not die, or is not unconscious or restrained)
        if (!actor.isVagrantOrFactionless) {
            //Leave current faction and become banned from the current faction
            if (actor.faction != null) {
                actor.faction.AddBannedCharacter(actor);
            }
            actor.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
        }
        actor.MigrateHomeStructureTo(null);
    }
#endregion
}