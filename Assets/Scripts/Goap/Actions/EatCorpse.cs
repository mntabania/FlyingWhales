public class EatCorpse : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

    public EatCorpse() : base(INTERACTION_TYPE.EAT_CORPSE) {
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        actionIconString = GoapActionStateDB.Eat_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.WOLF, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsTargetDead);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity invalidity = base.IsInvalid(node);
        if (invalidity.isInvalid == false) {
            if (node.poiTarget is Character targetCharacter) {
                if (targetCharacter.numOfActionsBeingPerformedOnThis > 0) {
                    invalidity.isInvalid = true;
                }
            }    
        }
        return invalidity;
    }
    #endregion

    #region Preconditions
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        if (poiTarget is Character character) {
            return character.isDead;
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void PreEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
        if(goapNode.associatedJobType == JOB_TYPE.MONSTER_EAT_CORPSE || goapNode.associatedJobType == JOB_TYPE.HUNT_PREY) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Abstain Fullness");
        }
    }
    public void PerTickEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustFullness(8.5f);
    }
    public void AfterEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(-1);
        if (goapNode.poiTarget is Character character && character.marker != null) {
            if (character.currentRegion != null) {
                character.currentRegion.RemoveCharacterFromLocation(character);
            }
            character.DestroyMarker();
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, goapNode.poiTarget, "target is already dead");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, goapNode.poiTarget, "target is already dead");
        }
    }
    #endregion
}