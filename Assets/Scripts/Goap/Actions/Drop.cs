using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class Drop : GoapAction {

    public Drop() : base(INTERACTION_TYPE.DROP) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Haul_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
            RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
            RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
    }

    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, target = GOAP_EFFECT_TARGET.TARGET }, IsCarriedOrInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Drop Success", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0] is LocationStructure) {
                return otherData[0] as LocationStructure;
            } else if (otherData.Length == 2 && otherData[0] is LocationStructure && otherData[1] is LocationGridTile) {
                return otherData[0] as LocationStructure;
            }
        }
        return base.GetTargetStructure(node);
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        if (otherData != null) {
            if (otherData.Length == 2 && otherData[0] is LocationStructure && otherData[1] is LocationGridTile) {
                return otherData[1] as LocationGridTile;
            }
        }
        return null;
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(targetCharacter);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsDropTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        return goapActionInvalidity;
    }
    private bool IsDropTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.IsAvailable() == false 
            || (poiTarget.gridTileLocation == null && node.actor.ownParty.IsPOICarried(poiTarget) == false)) {
            return true;
        }
        return false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget;
        }
        return satisfied;
    }
    #endregion

    #region Preconditions
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        // if (poiTarget is Character) {
        //     Character target = poiTarget as Character;
        //     return target.currentParty == actor.currentParty;    
        // } else {
        //     return actor.ownParty.IsPOICarried(poiTarget);
        // }
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
    #endregion

    #region State Effects
    //public void PreDropSuccess(ActualGoapNode goapNode) {
    //    //GoapActionState currentState = this.states[goapNode.currentStateName];
    //    goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        //Character target = goapNode.poiTarget as Character;
        object[] otherData = goapNode.otherData;
        LocationGridTile tile = null;
        if (otherData != null) {
            if (otherData.Length == 2 && otherData[0] is LocationStructure && otherData[1] is LocationGridTile) {
                tile = otherData[1] as LocationGridTile;
            }
        }
        goapNode.actor.UncarryPOI(goapNode.poiTarget, dropLocation: tile);
        if(goapNode.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && goapNode.associatedJobType == JOB_TYPE.APPREHEND 
            && goapNode.poiTarget.gridTileLocation.structure == goapNode.actor.homeSettlement.prison) {
            Restrained restrainedTrait = goapNode.poiTarget.traitContainer.GetNormalTrait<Restrained>("Restrained");
            if (restrainedTrait != null) {
                restrainedTrait.SetIsPrisoner(true);
            } else {
                Trait restrained;
                goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Restrained", out restrained, goapNode.actor);
                (restrained as Restrained).SetIsPrisoner(true);
            }
        }
    }
    #endregion
}

public class DropData : GoapActionData {
    public DropData() : base(INTERACTION_TYPE.DROP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}
