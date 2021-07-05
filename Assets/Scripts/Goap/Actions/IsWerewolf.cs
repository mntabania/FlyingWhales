using Traits;
using System.Collections.Generic;

public class IsWerewolf : GoapAction {
    public IsWerewolf() : base(INTERACTION_TYPE.IS_WEREWOLF) {
        actionIconString = GoapActionStateDB.No_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Werewolf Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
  public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if(actor.isLycanthrope) {
            actor.lycanData.AddAwareCharacter(witness);
        }
        return response;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Werewolf);
        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
            reactions.Add(EMOTION.Shock);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Despair);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                reactions.Add(EMOTION.Threatened);
            }
            if (target is Character targetCharacter) {
                string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Disapproval);
                    reactions.Add(EMOTION.Anger);
                } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                    reactions.Add(EMOTION.Disapproval);
                    reactions.Add(EMOTION.Anger);
                } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Anger);
                    }
                }
            }
        } else {
            if (witness.traitContainer.HasTrait("Lycanphiliac")) {
                if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                    reactions.Add(EMOTION.Arousal);
                } else {
                    reactions.Add(EMOTION.Approval);
                }
            } else if (witness.traitContainer.HasTrait("Lycanphobic")) {
                reactions.Add(EMOTION.Threatened);
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Werewolf;
    }
#endregion

#region State Effects
    public void PreVampireSuccess(ActualGoapNode goapNode) { }
    public void PerTickVampireSuccess(ActualGoapNode goapNode) { }
    public void AfterVampireSuccess(ActualGoapNode goapNode) { }
#endregion
}