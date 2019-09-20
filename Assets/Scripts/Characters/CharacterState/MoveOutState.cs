﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveOutState : CharacterState {

    private int travelTimeInTicks {
        get {
            int travel = 3 * GameManager.ticksPerHour; //3 hours
            if (stateComponent.character.GetNormalTrait("Fast") != null) { //Reference: https://trello.com/c/Gb3kfZEm/2658-fast
                travel -= (int)(travel * 0.25f); //NOTE: Did not create a new world travel time modifier in character because it seems unneccessary if this is the only thing it is used for. Will put variable if more things need it.
            }
            return travel;
        }
    }

    public MoveOutState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Move Out State";
        characterState = CHARACTER_STATE.MOVE_OUT;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Explore_Icon;
    }

    #region Overrides
    protected override void StartState() {
        base.StartState();
        stateComponent.character.AdjustDoNotDisturb(1);
    }
    public override void PauseState() {
        base.PauseState();
        stateComponent.character.AdjustDoNotDisturb(-1);
    }
    public override void ResumeState() {
        base.ResumeState();
        stateComponent.character.AdjustDoNotDisturb(1);
    }
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        if (stateComponent.character.specificLocation == stateComponent.character.homeArea) {
            //if the character is still at his/her home area, go to the nearest edge tile
            LocationGridTile nearestEdgeTile = stateComponent.character.GetNearestUnoccupiedEdgeTileFromThis();
            stateComponent.character.marker.GoTo(nearestEdgeTile, OnArriveAtNearestEdgeTile);
        }
    }
    public override void OnExitThisState() {
        base.OnExitThisState();
        if (!string.IsNullOrEmpty(goHomeSchedID)) { //if this state is exited, and its goHomeSchedID is not empty (Usually because character died mid way). Cancel that schedule.
            SchedulingManager.Instance.RemoveSpecificEntry(goHomeSchedID);
        }
        stateComponent.character.SetPOIState(POI_STATE.ACTIVE);
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);
        stateComponent.character.marker.PlaceMarkerAt(stateComponent.character.homeArea.GetRandomUnoccupiedEdgeTile());
        stateComponent.character.AdjustDoNotDisturb(-1);
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
    }
    protected override void PerTickInState() { }
    protected override void CreateThoughtBubbleLog() {
        base.CreateThoughtBubbleLog();
        if (thoughtBubbleLog != null) {
            thoughtBubbleLog.AddToFillers(stateComponent.character.specificLocation, stateComponent.character.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        }
    }
    #endregion

    bool hasSceduledArriveAtRandomRegion;

    private void OnArriveAtNearestEdgeTile() {
        if (hasSceduledArriveAtRandomRegion) {
            return;
        }
        if (GetValidRegionsToDoJob(stateComponent.character).Count == 0) {
            job.jobQueueParent.CancelJob(job, "no valid regions", false);
            return;
        }
        hasSceduledArriveAtRandomRegion = true;
        stateComponent.character.CancelAllPlans();
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(true);
        stateComponent.character.SetPOIState(POI_STATE.INACTIVE);
        stateComponent.character.marker.gameObject.SetActive(false);
        stateComponent.character.marker.StopMovement();
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, this.stateComponent.character.ownParty);
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(travelTimeInTicks);
        SchedulingManager.Instance.AddEntry(dueDate, ArriveAtRegion, this);

        //Show log
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "left");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.specificLocation, stateComponent.character.specificLocation.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    private Region chosenRegion;
    private void ArriveAtRegion() {
        List<Region> choices = GetValidRegionsToDoJob(stateComponent.character);
        if (choices.Count > 0) {
            stateComponent.character.ownParty.icon.SetIsTravellingOutside(false);
            chosenRegion = choices[Random.Range(0, choices.Count)];
            stateComponent.character.specificLocation.RemoveCharacterFromLocation(stateComponent.character);
            OnArriveAtRegion();
            chosenRegion.AddCharacterToLocation(stateComponent.character);
        } else {
            job.jobQueueParent.CancelJob(job, "no valid regions", false);
        }
    }

    public string goHomeSchedID { get; private set; }
    private void OnArriveAtRegion() {
        //schedule go home
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(3 * GameManager.ticksPerHour);
        goHomeSchedID = SchedulingManager.Instance.AddEntry(dueDate, GoHome, this);
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "arrived_region");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(chosenRegion, chosenRegion.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    public void GoHome() {
        goHomeSchedID = string.Empty;
        stateComponent.character.ownParty.icon.SetIsTravellingOutside(true);
        chosenRegion.RemoveCharacterFromLocation(stateComponent.character); //remove character from landmark. He/She is now just floating.
        GameDate dueDate = GameManager.Instance.Today();
        dueDate = dueDate.AddTicks(travelTimeInTicks);
        SchedulingManager.Instance.AddEntry(dueDate, ArriveHome, this);

        //Show log
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "going_home");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.homeArea, stateComponent.character.homeArea.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;
    }

    private void ArriveHome() {
        OnExitThisState();
        Messenger.Broadcast(Signals.PARTY_DONE_TRAVELLING, stateComponent.character.currentParty);
        CheckNeeds();
        
        //Show log
        Log log = new Log(GameManager.Instance.Today(), "CharacterState", this.GetType().ToString(), "arrive_home");
        log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(stateComponent.character.homeArea, stateComponent.character.homeArea.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
        thoughtBubbleLog = log;

    }

    private void CheckNeeds() {
        string summary = GameManager.Instance.TodayLogString() + stateComponent.character.name + " has arrived home and will check his/her needs.";
        if (stateComponent.character.isStarving) {
            summary += "\n" + stateComponent.character.name + " is starving. Planning fullness recovery actions...";
            stateComponent.character.PlanFullnessRecoveryActions(true);
        }
        if (stateComponent.character.isExhausted) {
            summary += "\n" + stateComponent.character.name + " is exhausted. Planning tiredness recovery actions...";
            stateComponent.character.PlanTirednessRecoveryActions(true);
        }
        if (stateComponent.character.isForlorn) {
            summary += "\n" + stateComponent.character.name + " is forlorn. Planning happiness recovery actions...";
            stateComponent.character.PlanHappinessRecoveryActions(true);
        }
        Debug.Log(summary);
    }

    public override string ToString() {
        return "Move Out State by " + stateComponent.character.name;
    }

    private List<Region> GetValidRegionsToDoJob(Character character) {
        if (job == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + character.name + " is checking for valid regions to do job but his/her job is null.");
        }

        List<LANDMARK_TYPE> validLandmarkTypes = new List<LANDMARK_TYPE>();
        if (job.jobType == JOB_TYPE.DESTROY_PROFANE_LANDMARK) {
            validLandmarkTypes.Add(LANDMARK_TYPE.THE_PROFANE);
        } else {
            validLandmarkTypes.AddRange(Utilities.GetEnumValues<LANDMARK_TYPE>());
        }
        List<Region> choices = GridMap.Instance.allRegions.Where(x => 
            x.activeEvent == null && 
            x.coreTile.areaOfTile != stateComponent.character.homeArea && 
            validLandmarkTypes.Contains(x.mainLandmark.specificLandmarkType) &&
            StoryEventsManager.Instance.GetEventsThatCanProvideEffects(x, character, job.jobType.GetAllowedEventEffects()).Count > 0
        ).ToList();

        return choices;
    }
}
