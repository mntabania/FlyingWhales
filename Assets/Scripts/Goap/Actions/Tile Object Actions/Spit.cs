using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Spit : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public Spit() : base(INTERACTION_TYPE.SPIT) {
        actionIconString = GoapActionStateDB.Anger_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Spit Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
        costLog += $" +{cost}(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Spat > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += $" +{timesCost}(10 x Times Spat)";
        }
        if (actor.traitContainer.HasTrait("Evil")) {
            cost += -15;
            costLog += " -15(Evil)";
        }
        if (actor.traitContainer.HasTrait("Treacherous")) {
            cost += -10;
            costLog += " -10(Treacherous)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Tombstone) {
            Character targetCharacter = (target as Tombstone).character;
            string witnessOpinionLabelToDead = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionLabelToDead == RelationshipManager.Friend || witnessOpinionLabelToDead == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            } else if (witnessOpinionLabelToDead == RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            }
        }
        return response;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
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
    }
    public void AfterSpitSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(20f);
        //Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.actor.marker.transform.position, 
        //    4, goapNode.actor.currentRegion.innerMap);
    }
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

public class SpitData : GoapActionData {
    public SpitData() : base(INTERACTION_TYPE.SPIT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget is Tombstone) {
            Tombstone tombstone = poiTarget as Tombstone;
            Character target = tombstone.character;
            return actor.relationshipContainer.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.NEGATIVE;
        }
        return false;
    }
}