using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class PickUp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PickUp() : base(INTERACTION_TYPE.PICK_UP) {
        actionIconString = GoapActionStateDB.Inspect_Icon;
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //SpecialToken token = poiTarget as SpecialToken;
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), targetPOI = actor });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.TARGET));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List <GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        TileObject item = target as TileObject;
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, target = GOAP_EFFECT_TARGET.TARGET });
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, target = GOAP_EFFECT_TARGET.ACTOR });
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (job != null && job.jobType == JOB_TYPE.OBTAIN_PERSONAL_ITEM) {
            if (!target.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)) {
                cost += 2000;
                costLog += " +2000(Object is not part of home settlement and job is Obtain Personal Item)";
            } else if (target.IsOwnedBy(actor) && target.gridTileLocation.structure == actor.homeStructure) {
                cost += 2000;
                costLog += " +2000(Object is owned by actor and object is in home and job is Obtain Personal Item)";
            }
        }
        if(target is TileObject targetTileObject) {
            if(targetTileObject.characterOwner == null) {
                if (job != null && (job.jobType == JOB_TYPE.TAKE_ITEM || job.jobType == JOB_TYPE.HAUL)) {
                    cost += 10;
                    costLog += $" +10(No personal owner, Take Item Job)";
                } else if(actor.homeSettlement != null && targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && targetTileObject.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == actor.homeSettlement) {
                    int randomCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                    cost += randomCost;
                    costLog += $" +{randomCost}(No personal owner, object inside actor home settlement)";
                } else if (!actor.isFactionless && !actor.isFriendlyFactionless && targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                     && targetTileObject.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null
                     && targetTileObject.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.owner == actor.faction) {
                    int randomCost = UtilityScripts.Utilities.Rng.Next(100, 121);
                    cost += randomCost;
                    costLog += $" +{randomCost}(No personal owner, object inside actor's faction owned settlement)";
                } else {
                    cost += 2000;
                    costLog += $" +2000(No personal owner)";
                }
            } else {
                if(targetTileObject.IsOwnedBy(actor)) {
                    cost += UtilityScripts.Utilities.Rng.Next(20, 61);
                    costLog += $" +{cost}(Personal owner is actor)";
                } else {
                    if(actor.traitContainer.HasTrait("Kleptomaniac") || !actor.relationshipContainer.HasRelationshipWith(targetTileObject.characterOwner) || (job != null && job.jobType == JOB_TYPE.HAUL)) {
                        cost += UtilityScripts.Utilities.Rng.Next(80, 91);
                        costLog += $" +{cost}(Kleptomaniac/No rel with owner)";
                    } else {
                        cost += 2000;
                        costLog += " +2000(Not Kleptomanic/Has rel with owner)";
                    }
                }
            }
        }
        actor.logComponent.AppendCostLog(costLog);
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
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;

        if(target is TileObject targetTileObject) {
            if (targetTileObject.characterOwner != null && !targetTileObject.IsOwnedBy(node.actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
                    if (targetTileObject.IsOwnedBy(witness)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                    }
                } else if (targetTileObject.IsOwnedBy(witness)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                }
                CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
            }
        }
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
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
        bool setOwnership = true;
        if(goapNode.associatedJobType == JOB_TYPE.HAUL 
            || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL 
            || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT
            || goapNode.associatedJobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD) {
            setOwnership = false;
        }
        goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject, setOwnership: setOwnership);
    }
    #endregion
}

public class PickItemData : GoapActionData {
    public PickItemData() : base(INTERACTION_TYPE.PICK_UP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        TileObject item = poiTarget as TileObject;
        return poiTarget.gridTileLocation != null && !actor.HasItem(item);
    }
}
