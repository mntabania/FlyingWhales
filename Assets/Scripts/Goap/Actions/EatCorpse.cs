public class EatCorpse : GoapAction {
    public EatCorpse() : base(INTERACTION_TYPE.EAT_CORPSE) {
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        actionIconString = GoapActionStateDB.Eat_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.WOLF };
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsTargetDead);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetHungry(-1);
    }
    #endregion

    #region Preconditions
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character character) {
            return character.isDead;
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void PreEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
    }
    public void PerTickEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustFullness(8.5f);
    }
    public void AfterEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(-1);
        if (goapNode.poiTarget is Character character && character.marker != null) {
            character.DestroyMarker();
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, goapNode.poiTarget, "target is already dead");
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData);
        if (satisfied) {
            if (target is Character targetCharacter) {
                if (targetCharacter.numOfActionsBeingPerformedOnThis > 0) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    #endregion
}