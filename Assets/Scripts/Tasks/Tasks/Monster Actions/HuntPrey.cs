﻿using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;

public class HuntPrey : CharacterTask {

    private BaseLandmark _target;

	private string hunterName;

	public HuntPrey(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.HUNT_PREY, defaultDaysLeft) {
		SetStance (STANCE.COMBAT);
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget (character);
		}
		if(_targetLocation != null && _targetLocation is BaseLandmark){
			_target = (BaseLandmark)_targetLocation;
			hunterName = _assignedCharacter.name;
			if(_assignedCharacter.party != null){
				hunterName = _assignedCharacter.party.name;
			}
			_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartHunt ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
//        TriggerSaveLandmarkQuest();
    }
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();
        Hunt();
        //GoToTargetLocation();
        
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
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0){
			if(character.faction == null){
				return true;
			}else{
				if(location.tileLocation.landmarkOnTile.owner.id != character.faction.id){
					return true;
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        Hunt();
    //    }
    //}

	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
//				if(_assignedCharacter.faction == null){
//					_landmarkWeights.AddElement (landmark, 100);
//				}else{
//					if(_assignedCharacter.faction.id != landmark.owner.id){
//						_landmarkWeights.AddElement (landmark, 100);
//					}
//				}
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
    #endregion

    //private void GoToTargetLocation() {
    //    GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
    //    goToLocation.InitializeAction(_target);
    //    goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
    //    goToLocation.onTaskActionDone += StartHunt;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;
    //    goToLocation.DoAction(_assignedCharacter);
    //}

    private void StartHunt() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartHunt ());
			return;
		}
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "HuntPrey", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _target.AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);
    }

    private void Hunt() {
        if(this.taskStatus != TASK_STATUS.IN_PROGRESS) {
            return;
        }
        HUNT_ACTION chosenAct = TaskManager.Instance.huntActions.PickRandomElementGivenWeights();
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
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft (1);
    }

    private void EatCivilian() {
        if(_target.civilians > 0) {
			RACE[] races = _target.civiliansByRace.Keys.Where(x => _target.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_target.AdjustCivilians (chosenRace, -1);
            Log eatLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "HuntPrey", "eat_civilian");
            eatLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            eatLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(chosenRace).ToLower(), LOG_IDENTIFIER.OTHER);
            _target.AddHistory(eatLog);
            _assignedCharacter.AddHistory(eatLog);

            //          _target.ReduceCivilians(1);
            //GameDate nextDate = GameManager.Instance.Today();
            //nextDate.AddDays(1);
            //SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
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
        //SetCanDoDailyAction(false);
//        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
//			settlement.CancelSaveALandmark (_target);
//		}
		EndTask(TASK_STATUS.SUCCESS);
	}
}
