public class PlagueHysteria : Emotion {

    public PlagueHysteria() : base(EMOTION.Plague_Hysteria) {
        responses = new[] {"Plague_Fear"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if (target is Character targetCharacter) {
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Plague Hysteria", -10);
            witness.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, targetCharacter, INTERACTION_TYPE.IS_PLAGUED, REACTION_STATUS.WITNESSED);
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}
