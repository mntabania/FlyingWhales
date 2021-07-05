using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Murder : GoapAction {
    public Murder() : base(INTERACTION_TYPE.MURDER) {
        actionIconString = GoapActionStateDB.Death_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Murder Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).isDead == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (actor != targetCharacter) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    reactions.Add(EMOTION.Fear);
                } else {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if (opinionLabel == RelationshipManager.Rival) {
                        reactions.Add(EMOTION.Approval);
                    } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Anger);
                        reactions.Add(EMOTION.Threatened);
                    } else {
                        reactions.Add(EMOTION.Shock);
                        reactions.Add(EMOTION.Disapproval);
                    }
                }
            } else {
                reactions.Add(EMOTION.Shock);
                if (witness.traitContainer.HasTrait("Psychopath") || witness.relationshipContainer.IsEnemiesWith(actor)) {
                    reactions.Add(EMOTION.Scorn);
                } else {
                    reactions.Add(EMOTION.Disapproval);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            if (actor != targetCharacter) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Rival) {
                    reactions.Add(EMOTION.Scorn);
                } else {
                    reactions.Add(EMOTION.Concern);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            if (actor != targetCharacter) {
                reactions.Add(EMOTION.Anger);
                if (targetCharacter.relationshipContainer.IsFriendsWith(actor) && !targetCharacter.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Betrayal);
                }
            }
        }
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(actor == target) {
            return CRIME_TYPE.None;
        }
        return CRIME_TYPE.Murder;
    }
#endregion

#region State Effects
    public void AfterMurderSuccess(ActualGoapNode goapNode) {
        (goapNode.poiTarget as Character).Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor != poiTarget && !(poiTarget as Character).isDead;
        }
        return false;
    }
#endregion
}