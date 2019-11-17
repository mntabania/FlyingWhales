﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : CharacterState {
    private int _planDuration;

    public PatrolState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Patrol State";
        characterState = CHARACTER_STATE.PATROL;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 24;
        actionIconString = GoapActionStateDB.Patrol_Icon;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartPatrolMovement();
    }
    //public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
    //    if(targetPOI is Character) {
    //        return stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
    //    } else if (stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && targetPOI is SpecialToken) {
    //        SpecialToken token = targetPOI as SpecialToken;
    //        if(token.characterOwner == null) {
    //            //Patrollers should not pick up items from their warehouse
    //            if (token.structureLocation != null && token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE 
    //                && token.specificLocation == stateComponent.character.homeArea) {
    //                return false;
    //            }
    //            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PICK_ITEM_GOAP, stateComponent.character, targetPOI);
    //            if (goapAction.targetTile != null) {
    //                SetCurrentlyDoingAction(goapAction);
    //                goapAction.CreateStates();
    //                stateComponent.character.SetCurrentAction(goapAction);
    //                stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtPickUpLocation);
    //                PauseState();
    //            } else {
    //                Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't pick up item " + targetPOI.name + " because there is no tile to go to!");
    //            }
    //            return true;
    //        }
    //    }
    //    return base.OnEnterVisionWith(targetPOI);
    //}
    protected override void PerTickInState() {
        base.PerTickInState();
        if (!isDone && !isPaused) {
            if(stateComponent.character.traitContainer.GetNormalTrait("Injured") != null) {
                StopStatePerTick();
                OnExitThisState();
                return;
            }
            if (_planDuration >= 4) {
                _planDuration = 0;
                if (!stateComponent.character.PlanFullnessRecoveryActions(true)) {
                    if (!stateComponent.character.PlanTirednessRecoveryActions(true)) {
                        stateComponent.character.PlanHappinessRecoveryActions(true);
                    }
                }
            } else {
                _planDuration++;
            }
        }
    }
    #endregion

    private void OnArriveAtPickUpLocation() {
        if (stateComponent.character.currentActionNode == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + stateComponent.character.name + " arrived at pick up location of item during " + stateName + ", but current action is null");
            return;
        }
        stateComponent.character.currentActionNode.SetEndAction(PatrolAgain);
        stateComponent.character.currentActionNode.Perform();
    }
    private void PatrolAgain(string result, GoapAction goapAction) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentActionNode(null);
        ResumeState();
    }

    private void StartPatrolMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartPatrolMovement);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
