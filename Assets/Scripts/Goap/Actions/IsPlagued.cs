public class IsPlagued : GoapAction {
    public IsPlagued() : base(INTERACTION_TYPE.IS_PLAGUED) {
        actionIconString = GoapActionStateDB.Sick_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Plague Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 0;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, poiTarget, witness, node, status);
        if (witness.relationshipContainer.IsFriendsWith(actor)) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status, node);
        } else if (witness.relationshipContainer.IsEnemiesWith(actor)) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status, node);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status, node);
        } else {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status, node);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
        }
        
        return response;
    }
    #endregion
    
    #region State Effects
    public void PrePlagueSuccess(ActualGoapNode goapNode) { }
    public void PerTickPlagueSuccess(ActualGoapNode goapNode) { }
    public void AfterPlagueSuccess(ActualGoapNode goapNode) { }
    #endregion
}