using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class RememberFallen : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;
    public RememberFallen() : base(INTERACTION_TYPE.REMEMBER_FALLEN) {
        actionIconString = GoapActionStateDB.Sad_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT, };
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remember Success", goapNode);
    }
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
        int cost = UtilityScripts.Utilities.Rng.Next(80, 121);
#if DEBUG_LOG
        costLog += $" +{cost}(Initial)";
#endif
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Times Reminisced > 5)";
#endif
        }
        if (actor.traitContainer.HasTrait("Psychopath") || actor.partyComponent.isActiveMember) {
            cost += 2000;
#if DEBUG_LOG
            costLog += " +2000(Psychopath or active member in a party)";
#endif
        }
        if (actor.moodComponent.moodState == MOOD_STATE.Bad || actor.moodComponent.moodState == MOOD_STATE.Critical || !actor.limiterComponent.isSociable) {
            cost -= 15;
#if DEBUG_LOG
            costLog += " -15(Low or Critical Mood or Unsociable)";
#endif
        }
        int timesCost = 10 * numOfTimesActionDone;
        cost += timesCost;
#if DEBUG_LOG
        costLog += $" +{timesCost.ToString()}(10 x Times Reminisced)";
        actor.logComponent.AppendCostLog(costLog);
#endif
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
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Tombstone) {
            Character targetCharacter = (target as Tombstone).character;
            string witnessOpinionLabelToDead = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if ((witnessOpinionLabelToDead == RelationshipManager.Friend || witnessOpinionLabelToDead == RelationshipManager.Close_Friend)
                && !witness.traitContainer.HasTrait("Psychopath")) {
                reactions.Add(EMOTION.Approval);
            } else if (witnessOpinionLabelToDead == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Resentment);
                if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    reactions.Add(EMOTION.Disappointment);
                }
            }
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
        goapNode.descriptionLog.AddToFillers(tombstone.character, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickRememberSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(6f);
    }
    //public void AfterRememberSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
    //    //Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.actor.marker.transform.position, 
    //    //    2, goapNode.actor.currentRegion.innerMap);
    //}
    //public void PreTargetMissing() {
    //    Tombstone tombstone = goapNode.poiTarget as Tombstone;
    //    goapNode.descriptionLog.AddToFillers(null, tombstone.character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
#endregion
}