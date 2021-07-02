using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class PickUp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PickUp() : base(INTERACTION_TYPE.PICK_UP) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON, RACE.TROLL, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        if (node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset) {
            return true;
        }
        return base.ShouldActionBeAnIntel(node);
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        //SpecialToken token = poiTarget as SpecialToken;
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), targetPOI = actor });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.TARGET));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
        List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
        if (baseEE != null && baseEE.Count > 0) {
            ee.AddRange(baseEE);
        }
        TileObject item = target as TileObject;
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, target = GOAP_EFFECT_TARGET.TARGET });
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, target = GOAP_EFFECT_TARGET.ACTOR });
        isOverridden = true;
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
#if DEBUG_LOG
                costLog += $" +2000(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (job != null && job.jobType == JOB_TYPE.TAKE_ITEM && target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out var settlement) 
            && settlement.locationType == LOCATION_TYPE.VILLAGE && settlement.owner != null) {
            if (settlement.owner != actor.faction && actor.faction != null) {
                FactionRelationship rel = actor.faction.GetRelationshipWith(settlement.owner);
                if (rel != null && rel.relationshipStatus != FACTION_RELATIONSHIP_STATUS.Hostile) {
#if DEBUG_LOG
                    costLog += $" +2000(Job is take item and Location of target is in settlement that is NOT hostile with actor)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                }
            }
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}:";
#endif
        int cost = 0;
        if (job != null && job.jobType == JOB_TYPE.OBTAIN_PERSONAL_ITEM) {
            if (!target.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)) {
                cost += 2000;
#if DEBUG_LOG
                costLog = $"{costLog} +2000(Object is not part of home settlement and job is Obtain Personal Item)";
#endif
            }
            
            //else if (target.IsOwnedBy(actor) && target.gridTileLocation.structure == actor.homeStructure) {
            //    cost += 2000;
            //    costLog = $"{costLog} +2000(Object is owned by actor and object is in home and job is Obtain Personal Item)";
            //}
        }
        if(target is TileObject targetTileObject) {
            if(targetTileObject is Heirloom && job != null && job.jobType == JOB_TYPE.DROP_ITEM_PARTY) { //|| job.jobType == JOB_TYPE.DROP_ITEM
                cost += 10;
#if DEBUG_LOG
                costLog = $"{costLog} +10(Heirloom)";
#endif
            } else if(targetTileObject is FoodPile && job != null && job.jobType == JOB_TYPE.DISPOSE_FOOD_PILE) { //|| job.jobType == JOB_TYPE.DROP_ITEM
                cost += 10;
#if DEBUG_LOG
                costLog = $"{costLog} +10(Dispose Food Pile)";
#endif
            } else {
                if (targetTileObject.characterOwner == null) {
                    if (job != null && (job.jobType == JOB_TYPE.TAKE_ITEM || job.jobType == JOB_TYPE.HAUL || job.jobType == JOB_TYPE.COMBINE_STOCKPILE || job.jobType == JOB_TYPE.STOCKPILE_FOOD || job.jobType == JOB_TYPE.OBTAIN_WANTED_ITEM)) {
                        cost += 10;
#if DEBUG_LOG
                        costLog = $"{costLog} +10(No personal owner, Take Item/Haul/Combine Stockpile/Stockpile Food)";
#endif
                    } else if (job != null && job.jobType == JOB_TYPE.REMOVE_STATUS) {
                        if (target.gridTileLocation != null && actor.homeSettlement != null &&
                            actor.movementComponent.HasPathTo(targetTileObject.gridTileLocation) &&
                             (targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) ||
                              targetTileObject.gridTileLocation.IsNextToSettlementArea(actor.homeSettlement))) {
                            int randomCost = UtilityScripts.Utilities.Rng.Next(40, 81);
                            cost += randomCost;
#if DEBUG_LOG
                            costLog = $"{costLog} +{randomCost.ToString()}(Job is remove status and object is reachable or is next to or part of actor's home settlement)";
#endif
                        } else {
                            cost += 2000;
#if DEBUG_LOG
                            costLog = $"{costLog} +2000(Job is remove status and object is NOT reachable and is NOT next to or part of actor's home settlement)";
#endif
                        }
                    } else if (actor.homeSettlement != null && targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.area.settlementOnArea == actor.homeSettlement) {
                        int randomCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                        cost += randomCost;
#if DEBUG_LOG
                        costLog = $"{costLog} +{randomCost.ToString()}(No personal owner, object inside actor home settlement)";
#endif
                    } else if (!actor.isFactionless && !actor.isVagrantOrFactionless && targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.area.settlementOnArea != null
                         && targetTileObject.gridTileLocation.area.settlementOnArea.owner == actor.faction) {
                        int randomCost = UtilityScripts.Utilities.Rng.Next(100, 121);
                        cost += randomCost;
#if DEBUG_LOG
                        costLog = $"{costLog} +{randomCost.ToString()}(No personal owner, object inside actor's faction owned settlement)";
#endif
                    } else {
                        cost += 2000;
#if DEBUG_LOG
                        costLog = $"{costLog} +2000(No personal owner)";
#endif
                    }
                } else {
                    if (targetTileObject.IsOwnedBy(actor)) {
                        if (targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.structure == actor.homeStructure) {
                            cost += UtilityScripts.Utilities.Rng.Next(10, 31);
#if DEBUG_LOG
                            costLog = $"{costLog} +{cost}(Personal owner is actor and object is inside home structure of actor)";
#endif
                        } else {
                            cost += UtilityScripts.Utilities.Rng.Next(40, 81);
#if DEBUG_LOG
                            costLog = $"{costLog} +{cost}(Personal owner is actor)";
#endif
                        }
                    } else {
                        cost += 2000;
#if DEBUG_LOG
                        costLog = $"{costLog} +2000(Has owner)";
#endif
                        //if (actor.traitContainer.HasTrait("Kleptomaniac") || !actor.relationshipContainer.HasRelationshipWith(targetTileObject.characterOwner) || (job != null && job.jobType == JOB_TYPE.HAUL)) {
                        //    cost += UtilityScripts.Utilities.Rng.Next(80, 91);
                        //    costLog += $" +{cost}(Kleptomaniac/No rel with owner)";
                        //} else {
                        //    cost += 2000;
                        //    costLog += " +2000(Not Kleptomaniac/Has rel with owner)";
                        //}
                    }
                }
            }
        }
        if(actor is Troll && job != null && job.jobType == JOB_TYPE.DROP_ITEM) {
            cost = 10;
#if DEBUG_LOG
            costLog = $"{costLog} 10(Troll, Drop Item Job)";
#endif
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is TileObject tileObject) {
            if (tileObject.characterOwner != null && !tileObject.IsOwnedBy(node.actor)) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is TileObject targetTileObject) {
            if (targetTileObject.characterOwner != null && !targetTileObject.IsOwnedBy(node.actor)) {
                reactions.Add(EMOTION.Disapproval);
                if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    reactions.Add(EMOTION.Disappointment);
                    reactions.Add(EMOTION.Shock);
                    if (targetTileObject.IsOwnedBy(witness)) {
                        reactions.Add(EMOTION.Betrayal);
                    }
                } else if (targetTileObject.IsOwnedBy(witness)) {
                    reactions.Add(EMOTION.Anger);
                }
            }
        }
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(target is TileObject tileObject) {
            if(tileObject.characterOwner != null) {
                Character ownerLover = tileObject.characterOwner.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                if(actor != tileObject.characterOwner && actor != ownerLover) {
                    return CRIME_TYPE.Theft;
                }
            }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        if (node.associatedJobType == JOB_TYPE.COMBINE_STOCKPILE) {
            //if this action was stopped and the associated job was combine stockpile, make sure to drop the resource pile that the character just picked up
            Character actor = node.actor;
            if (actor.HasItem<ResourcePile>()) {
                List<ResourcePile> resourcePiles = RuinarchListPool<ResourcePile>.Claim();
                actor.PopulateItemsOfType<ResourcePile>(resourcePiles);
                for (int i = 0; i < resourcePiles.Count; i++) {
                    ResourcePile resourcePile = resourcePiles[i];
                    actor.DropItem(resourcePile);
                }
                RuinarchListPool<ResourcePile>.Release(resourcePiles);
            }
        }
    }
    #endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            TileObject item = poiTarget as TileObject;
            return poiTarget.gridTileLocation != null && !actor.HasItem(item) && poiTarget.numOfActionsBeingPerformedOnThis <= 0;
        }
        return false;
        
    }
#endregion

#region State Effects
    public void PreTakeSuccess(ActualGoapNode goapNode) {
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        //Picked up item when in haul job should not set its ownership to the actor because he/she will just deliver it to the main storage
        //This will fix na assumption issues and the issue with other characters not being able to do haul job if the first character to haul dropped the item (first character will be the item owner) because of the pick up costing
        bool setOwnership = !(goapNode.associatedJobType == JOB_TYPE.HAUL 
                              || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL 
                              || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT
                              || goapNode.associatedJobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD);
        goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject, setOwnership: setOwnership);
    }
#endregion
}