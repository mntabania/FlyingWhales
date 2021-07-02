using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;
using Inner_Maps;
using Debug = System.Diagnostics.Debug;

public class Spit : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.VERBAL; } }

    public Spit() : base(INTERACTION_TYPE.SPIT) {
        actionIconString = GoapActionStateDB.Anger_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Spit Success", goapNode);
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    node.actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                    LocationGridTile centerGridTileOfActor = actor.gridTileLocation.area.gridTileComponent.centerGridTile;
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
#if DEBUG_LOG
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
#endif
                        return 2000;
                    }
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(80, 131);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Times Spat > 5)";
#endif
        }
        if (!actor.partyComponent.isActiveMember) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Is not in active party quest)";
#endif
        }
        if (!actor.traitContainer.HasTrait("Angry", "Annoyed", "Drunk")) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Not angry, annoyed or drunk)";
#endif
        }
        Betrayed betrayed = actor.traitContainer.GetTraitOrStatus<Betrayed>("Betrayed");
        if (target is Tombstone tombstone) {
            if (betrayed != null && betrayed.IsResponsibleForTrait(tombstone.character)) {
                cost -= 25;
#if DEBUG_LOG
                costLog += " -25(Actor is betrayed by target)";
#endif
            }
        }
        if (actor.traitContainer.HasTrait("Evil")) {
            cost -= 10;
#if DEBUG_LOG
            costLog += " -10(Evil)";
#endif
        }
        if (actor.traitContainer.HasTrait("Treacherous")) {
            cost -= 10;
#if DEBUG_LOG
            costLog += " -10(Treacherous)";
#endif
        }

        int timesCost = 10 * numOfTimesActionDone;
        cost += timesCost;
#if DEBUG_LOG
        costLog += $" +{timesCost.ToString()}(10 x Times Spat)";

        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Tombstone) {
            Character targetCharacter = (target as Tombstone).character;
            string witnessOpinionLabelToDead = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionLabelToDead == RelationshipManager.Friend || witnessOpinionLabelToDead == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Anger);
                reactions.Add(EMOTION.Disapproval);
            } else if (witnessOpinionLabelToDead == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Approval);
            } else {
                reactions.Add(EMOTION.Disapproval);
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        if(node.poiTarget is Tombstone tombstone) {
            log.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            if (poiTarget is Tombstone tombstone) {
                Character target = tombstone.character;
                return actor.relationshipContainer.IsEnemiesWith(target);
                // return actor.relationshipContainer.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE;
            }
            return false;
        }
        return false;
    }
#endregion

#region Effects
    public void PreSpitSuccess(ActualGoapNode goapNode) {
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
    }
    public void PerTickSpitSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(50f);
    }
    //public void AfterSpitSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
#endregion

    //#region Intel Reactions
    //private List<string> SpitSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Tombstone tombstone = poiTarget as Tombstone;
    //    Character targetCharacter = tombstone.character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("I know what I did.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                if(RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    reactions.Add(string.Format("{0} does not respect me.", actor.name));
    //                    AddTraitTo(recipient, "Annoyed");
    //                } else {
    //                    reactions.Add(string.Format("{0} should not do that again.", actor.name));
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                    reactions.Add("That was very rude!");
    //                    AddTraitTo(recipient, "Annoyed");
    //                } else {
    //                    reactions.Add(string.Format("{0} should not do that again.", actor.name));
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                reactions.Add("That was not nice.");
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                reactions.Add("That was not nice.");
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}