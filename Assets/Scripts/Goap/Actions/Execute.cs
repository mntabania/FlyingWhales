using Traits;
using UnityEngine;
using System.Collections.Generic;

public class Execute : GoapAction {
    public Execute() : base(INTERACTION_TYPE.EXECUTE) {
        actionIconString = GoapActionStateDB.Death_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Life_Changes};
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        // AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", false, GOAP_EFFECT_TARGET.TARGET), IsTargetRestrained);
        // AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        // AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Criminal", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Execute Success", goapNode);
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
        Character targetCharacter = target as Character;
        if (targetCharacter.crimeComponent.HasWantedCrime() && targetCharacter.crimeComponent.IsTargetOfACrime(witness)) {
            reactions.Add(EMOTION.Approval);
        } else {
            if (witness.relationshipContainer.IsFriendsWith(targetCharacter)
                && witness.traitContainer.HasTrait("Psychopath") == false) {
                reactions.Add(EMOTION.Resentment);
            }
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    if (Random.Range(0, 100) < 75) {
                        reactions.Add(EMOTION.Fear);
                    } else {
                        reactions.Add(EMOTION.Shock);
                    }
                } else {
                    if (Random.Range(0, 100) < 15) {
                        reactions.Add(EMOTION.Fear);
                    } else {
                        reactions.Add(EMOTION.Shock);
                    }
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        if (witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Acquaintance)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                reactions.Add(EMOTION.Sadness);
            }
        } else if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
            if (witness.traitContainer.HasTrait("Psychopath") == false) {
                reactions.Add(EMOTION.Sadness);
            }
        } else if (witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival)) {
            if (witness.traitContainer.HasTrait("Diplomatic") == false) {
                reactions.Add(EMOTION.Scorn);
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        Character targetCharacter = target as Character;
        if (Random.Range(0, 100) < 20) {
            reactions.Add(EMOTION.Resentment);
        }
        if (target.traitContainer.HasTrait("Hothead") || Random.Range(0, 100) < 20) {
            reactions.Add(EMOTION.Anger);
        }
    }
#endregion

#region State Effects
    public void AfterExecuteSuccess(ActualGoapNode goapNode) {
        Character target = goapNode.target as Character;
        if (target.traitContainer.HasTrait("Criminal")) {
            Criminal criminalTrait = target.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
            criminalTrait.SetIsImprisoned(false);
        }
        target.crimeComponent.SetDecisionAndJudgeToAllUnpunishedCrimesWantedBy(target.faction, CRIME_STATUS.Executed, goapNode.actor);
        target.crimeComponent.RemoveAllCrimesWantedBy(goapNode.actor.faction);

        target.traitContainer.RemoveTrait(target, "Criminal", goapNode.actor);
        target.traitContainer.RemoveRestrainAndImprison(target, goapNode.actor);
        target.Death("executed", goapNode, goapNode.actor);
    }
#endregion
    
#region Preconditions
    private bool IsTargetRestrained(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.HasTrait("Restrained");
    }
#endregion

}