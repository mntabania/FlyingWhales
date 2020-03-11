using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class StrollState : CharacterState {
    private int _planDuration;

    public StrollState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Stroll State";
        characterState = CHARACTER_STATE.STROLL;
        //stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = GameManager.ticksPerHour;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartStrollMovement();
    }
    public override void PerTickInState() {
        base.PerTickInState();
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
    // public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
    //     if (stateComponent.character.faction.isMajorFriendlyNeutral && 
    //         stateComponent.character.faction != FactionManager.Instance.zombieFaction &&
    //         !Utilities.IsRaceBeast(stateComponent.character.race) && 
    //         targetPOI is SpecialToken) {
    //         SpecialToken token = targetPOI as SpecialToken;
    //         if (token.CanBePickedUpNormallyUponVisionBy(stateComponent.character)) {
    //             ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PICK_UP], stateComponent.character, targetPOI, null, 0);
    //             GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, stateComponent.character);
    //             GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_UP, targetPOI, stateComponent.character);
    //             goapPlan.SetDoNotRecalculate(true);
    //             job.SetCannotBePushedBack(true);
    //             job.SetAssignedPlan(goapPlan);
    //             stateComponent.character.jobQueue.AddJobInQueue(job);
    //             return true;
    //         }
    //     }
    //     return base.OnEnterVisionWith(targetPOI);
    // }
    #endregion
    private void StartStrollMovement() {
        LocationGridTile target = PickRandomTileToGoTo();
        stateComponent.character.marker.GoTo(target, StartStrollMovement);
        //Debug.Log(stateComponent.character.name + " will stroll to " + target.ToString());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 4, 3, false, true);
        if (tiles.Count > 0) {
            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        } else {
            throw new System.Exception(
                $"No unoccupied tile in 3-tile radius for {stateComponent.character.name} to go to in {stateName}");
        }
        //int multiplier = 1;//UnityEngine.Random.Range(5, 8);
        //Vector3 forwardPos = stateComponent.character.marker.visualsParent.up * multiplier;
        //Vector2Int forwardPosInt = new Vector2Int((int) forwardPos.x, (int) forwardPos.y);
        //LocationGridTile chosenTile = stateComponent.character.GetLocationGridTileByXY(forwardPosInt.x, forwardPosInt.y, false);
        //if (chosenTile != null && chosenTile.structure != null) {
        //    return chosenTile;
        //} else {
        //    List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentAreaMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 3, false, true);
        //    if (tiles.Count > 0) {
        //        return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        //    } else {
        //        throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        //    }
        //}
    }
}
