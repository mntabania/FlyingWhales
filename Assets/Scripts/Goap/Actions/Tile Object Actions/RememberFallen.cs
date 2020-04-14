using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class RememberFallen : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public RememberFallen() : base(INTERACTION_TYPE.REMEMBER_FALLEN) {
        actionIconString = GoapActionStateDB.Sad_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT, };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remember Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
        costLog += $" +{cost}(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Reminisced > 5)";
        } else {
            int timesCost = 10 * numOfTimesActionDone;
            cost += timesCost;
            costLog += $" +{timesCost}(10 x Times Reminisced)";
        }
        if (actor.traitContainer.HasTrait("Psychopath")) {
            cost += 2000;
            costLog += " +2000(Psychopath)";
        }
        if (actor.moodComponent.moodState == MOOD_STATE.LOW || actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
            cost += -15;
            costLog += " -15(Low or Crit Mood)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        LocationStructure targetStructure = node.targetStructure;
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        Tombstone tombstone = poiTarget as Tombstone;
        log.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
        //if (targetStructure != null) {
        //    log.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //} else {
        //    log.AddToFillers(actor.specificLocation, actor.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        //}
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetBored(-1);
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Tombstone) {
            Character targetCharacter = (target as Tombstone).character;
            string witnessOpinionLabelToDead = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if ((witnessOpinionLabelToDead == RelationshipManager.Friend || witnessOpinionLabelToDead == RelationshipManager.Close_Friend)
                && !witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
            } else if (witnessOpinionLabelToDead == RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status);
                if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status);
                }
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
            if (poiTarget is Tombstone) {
                Tombstone tombstone = poiTarget as Tombstone;
                Character target = tombstone.character;
                return actor.relationshipContainer.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.POSITIVE;
            }
            return false;
        }
        return false;   
    }
    #endregion

    #region Effects
    public void PreRememberSuccess(ActualGoapNode goapNode) {
        Tombstone tombstone = goapNode.poiTarget as Tombstone;
        goapNode.descriptionLog.AddToFillers(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickRememberSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(4.5f);
    }
    public void AfterRememberSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.actor.marker.transform.position, 
        //    2, goapNode.actor.currentRegion.innerMap);
    }
    //public void PreTargetMissing() {
    //    Tombstone tombstone = goapNode.poiTarget as Tombstone;
    //    goapNode.descriptionLog.AddToFillers(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion
}

public class RememberFallenData : GoapActionData {
    public RememberFallenData() : base(INTERACTION_TYPE.REMEMBER_FALLEN) {
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
            return actor.relationshipContainer.GetRelationshipEffectWith(target) == RELATIONSHIP_EFFECT.POSITIVE;
        }
        return false;
    }
}