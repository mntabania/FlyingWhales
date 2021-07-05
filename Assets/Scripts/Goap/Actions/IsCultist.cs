using System.Collections.Generic;

public class IsCultist : GoapAction {
    public IsCultist() : base(INTERACTION_TYPE.IS_CULTIST) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cultist Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        Character poiTarget = target as Character;
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            reactions.Add(EMOTION.Shock);
            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinion(actor, RelationshipManager.Acquaintance)) {
                reactions.Add(EMOTION.Despair);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                reactions.Add(EMOTION.Threatened);
            }

            if (poiTarget != null && witness.relationshipContainer.IsEnemiesWith(poiTarget) == false) {
                reactions.Add(EMOTION.Disapproval);
            }
        } else {
            reactions.Add(EMOTION.Approval);
            if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = UnityEngine.Random.Range(0, 100);
                if (roll < chance) {
                    reactions.Add(EMOTION.Arousal);
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Demon_Worship;
    }
#endregion

//#region State Effects
//    public void PreCultistSuccess(ActualGoapNode goapNode) { }
//    public void PerTickCultistSuccess(ActualGoapNode goapNode) { }
//    public void AfterCultistSuccess(ActualGoapNode goapNode) { }
//#endregion
}