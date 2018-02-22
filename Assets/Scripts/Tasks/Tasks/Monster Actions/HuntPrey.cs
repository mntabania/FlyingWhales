﻿using UnityEngine;
using System.Collections;

public class HuntPrey : CharacterTask {

    private BaseLandmark _target;

    private enum HUNT_ACTION { EAT, END, NOTHING }

    private WeightedDictionary<HUNT_ACTION> huntActions;

    public HuntPrey(TaskCreator createdBy, BaseLandmark target) 
        : base(createdBy, TASK_TYPE.HUNT_PREY) {
        _target = target;
    }

    #region overrides
    public override void PerformTask() {
        base.PerformTask();
        _assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
        GoToTargetLocation();
        huntActions = new WeightedDictionary<HUNT_ACTION>();
        huntActions.AddElement(HUNT_ACTION.EAT, 15);
        huntActions.AddElement(HUNT_ACTION.END, 15);
        huntActions.AddElement(HUNT_ACTION.NOTHING, 70);
    }
    public override void TaskCancel() {
        base.TaskCancel();
        //Messenger.RemoveListener("OnDayEnd", Hunt);
        _assignedCharacter.DestroyAvatar();
    }
    public override void TaskFail() {
        base.TaskFail();
        //Messenger.RemoveListener("OnDayEnd", Hunt);
        _assignedCharacter.DestroyAvatar();
    }
    public override void PerformDailyAction() {
        if (_canDoDailyAction) {
            base.PerformDailyAction();
            Hunt();
        }
    }
    #endregion

    private void GoToTargetLocation() {
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_target);
        goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
        goToLocation.onTaskActionDone += StartHunt;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }

    private void StartHunt() {
        _target.AddHistory("Monster " + _assignedCharacter.name + " is hunting for food.");
        SetCanDoDailyAction(true);
        //Messenger.AddListener("OnDayEnd", Hunt);
        //GameDate nextDate = GameManager.Instance.Today();
        //nextDate.AddDays(1);
        //SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
		TriggerSaveLandmarkQuest ();
    }

    private void Hunt() {
        if(this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        HUNT_ACTION chosenAct = huntActions.PickRandomElementGivenWeights();
        switch (chosenAct) {
            case HUNT_ACTION.EAT:
                EatCivilian();
                break;
            case HUNT_ACTION.END:
				End();
                break;
            case HUNT_ACTION.NOTHING:
                //GameDate nextDate = GameManager.Instance.Today();
                //nextDate.AddDays(1);
                //SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
                break;
            default:
                break;
        }
    }

    private void EatCivilian() {
        if(_target.civilians > 0) {
            _target.ReduceCivilians(1);
            //GameDate nextDate = GameManager.Instance.Today();
            //nextDate.AddDays(1);
            //SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
        } else {
            End();
        }
    }

	private void TriggerSaveLandmarkQuest(){
		if(_target.location.region.centerOfMass.landmarkOnTile.isOccupied && !_target.location.region.centerOfMass.landmarkOnTile.AlreadyHasQuestOfType(QUEST_TYPE.SAVE_LANDMARK, _target)){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.SaveALandmark (_target);
		}
	}

	private void End(){
        //Messenger.RemoveListener("OnDayEnd", Hunt);
        SetCanDoDailyAction(false);
        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
			settlement.CancelSaveALandmark (_target);
		}
		EndTask(TASK_STATUS.SUCCESS);
	}
}
