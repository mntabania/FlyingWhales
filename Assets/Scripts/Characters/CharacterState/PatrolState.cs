using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class PatrolState : CharacterState {
    private int _planDuration;

    public PatrolState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Patrol State";
        characterState = CHARACTER_STATE.PATROL;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
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
    //        return stateComponent.character.combatComponent.AddHostileInRange(targetPOI as Character);
    //    } else if (stateComponent.character.role.roleType != CHARACTER_ROLE.BEAST && targetPOI is SpecialToken) {
    //        SpecialToken token = targetPOI as SpecialToken;
    //        if(token.characterOwner == null) {
    //            //Patrollers should not pick up items from their warehouse
    //            if (token.structureLocation != null && token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE 
    //                && token.specificLocation == stateComponent.character.homeNpcSettlement) {
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
    public override void PerTickInState() {
        base.PerTickInState();
        if (!stateComponent.character.canCombat) {
            stateComponent.ExitCurrentState();
            return;
        }
        if (_planDuration >= 4) {
            _planDuration = 0;
            if (!stateComponent.character.needsComponent.PlanFullnessRecoveryActions(stateComponent.character)) {
                if (!stateComponent.character.needsComponent.PlanTirednessRecoveryActions(stateComponent.character)) {
                    stateComponent.character.needsComponent.PlanHappinessRecoveryActions(stateComponent.character);
                }
            }
        } else {
            _planDuration++;
        }
    }
    #endregion
    private void StartPatrolMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartPatrolMovement);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.currentRegion.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception(
                $"No tile in {chosenStructure.name} for {stateComponent.character.name} to go to in {stateName}");
        }
    }
}
