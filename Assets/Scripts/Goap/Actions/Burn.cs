public class Burn : GoapAction {
    public Burn() : base(INTERACTION_TYPE.BURN) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
            RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
            RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Burn Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void AfterBurnSuccess(ActualGoapNode goapNode) {
        goapNode.actor.marker.animationListener.CreateProjectile(goapNode.poiTarget, null, (damagable, state) => OnHitTarget(damagable, state, goapNode.actor));
    }
    private void OnHitTarget(IDamageable damageable, CombatState state, Character actor) {
        if (damageable is IPointOfInterest poi) {
            poi.traitContainer.AddTrait(poi, "Burning", actor,  bypassElementalChance: true);
        }
    }
    #endregion
}