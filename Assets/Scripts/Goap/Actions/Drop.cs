﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Drop : GoapAction {

    public Drop() : base(INTERACTION_TYPE.DROP) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, target = GOAP_EFFECT_TARGET.TARGET }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Drop Success", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
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
        actor.currentParty.RemovePOI(targetCharacter);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.currentParty.RemovePOI(targetCharacter);
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
    private bool IsInActorParty(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
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
        goapNode.actor.currentParty.RemovePOI(goapNode.poiTarget, dropLocation: tile);
        if(goapNode.poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && goapNode.associatedJobType == JOB_TYPE.APPREHEND 
            && goapNode.poiTarget.gridTileLocation.structure == goapNode.actor.homeSettlement.prison) {
            Restrained restrainedTrait = goapNode.poiTarget.traitContainer.GetNormalTrait<Trait>("Restrained") as Restrained;
            if (restrainedTrait != null) {
                restrainedTrait.SetIsPrisoner(true);
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
