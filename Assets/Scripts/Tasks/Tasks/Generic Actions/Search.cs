using UnityEngine;
using System.Collections;
using ECS;
using System;
using System.Collections.Generic;

[Serializable]
public class Search : CharacterTask {

	private delegate BaseLandmark GetTargetLandmark(Character character);
	private event GetTargetLandmark onGetTargetLandmarkAction;

	private delegate bool AreConditionsMetAction(Character character);
	private event AreConditionsMetAction onAreConditionsMetAction;

    private object _searchingFor;
	private string searchingForLog;

	private BaseLandmark _targetLandmark;

	public Search(TaskCreator createdBy, int defaultDaysLeft, object searchingFor, ILocation targetLocation, Quest parentQuest = null, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.SEARCH, stance, defaultDaysLeft, parentQuest) {
        _targetLocation = targetLocation;
        _searchingFor = searchingFor;
		onGetTargetLandmarkAction = GetLandmarkForCharacterSearching;
		onAreConditionsMetAction = AreConditionsMetForCharacterSearching;
		Log log = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", _searchingFor as string); //Add Fillers as necesssary per item seaching for
        if (searchingFor is string) {
            if((searchingFor as string).Equals("Heirloom Necklace")) {
				if(_parentQuest != null){
					if(_parentQuest is FindLostHeir){
						_alignments.Add(ACTION_ALIGNMENT.LAWFUL);
					}else if(_parentQuest is EliminateLostHeir){
						_alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);
					}
				}
			}else if((searchingFor as string).Equals("Book of Inimical Incantations")) {
				SetStance(STANCE.COMBAT);
				onGetTargetLandmarkAction = GetLandmarkForLandmarkItemsSearching;
				onAreConditionsMetAction = AreConditionsMetForLandmarkItemsSearching;
				_alignments.Add(ACTION_ALIGNMENT.VILLAINOUS);
			}else if((searchingFor as string).Equals("Neuroctus")) {
				SetStance(STANCE.COMBAT);
				onGetTargetLandmarkAction = GetLandmarkForLandmarkItemsSearching;
				onAreConditionsMetAction = AreConditionsMetForLandmarkItemsSearching;
				_alignments.Add(ACTION_ALIGNMENT.PEACEFUL);
			}else if((searchingFor as string).Equals("Herbalist")) {
				SetStance(STANCE.COMBAT);
				_alignments.Add(ACTION_ALIGNMENT.PEACEFUL);
			}
            searchingForLog = Utilities.LogReplacer(log);
        }

        _states = new Dictionary<STATE, State>() {
            { STATE.MOVE, new MoveState(this)},
            { STATE.SEARCH, new SearchState(this, searchingFor) }
        };
    }

    #region overrides
    public override CharacterTask CloneTask() {
		Search clonedTask = new Search(_createdBy, _defaultDaysLeft, _searchingFor, _targetLocation, _parentQuest, _stance);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
        return clonedTask;
    }
    public override bool CanBeDone(Character character, ILocation location) {
        return true;
    }
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_targetLocation == null) {
            _targetLocation = GetLandmarkTarget(character);
        }
		if(_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			_targetLandmark = _targetLocation as BaseLandmark;
		}else{
			EndTask(TASK_STATUS.FAIL); //the character could not search anywhere, fail this task
			return;
		}
        ChangeStateTo(STATE.MOVE);
		_assignedCharacter.GoToLocation(_targetLandmark, PATHFINDING_MODE.USE_ROADS, () => StartSearch());
    }
    public override bool AreConditionsMet(Character character) {
        //check if there are any landmarks in region with characters
//		if(onAreConditionsMetAction != null){
//			return onAreConditionsMetAction (character);
//		}
        return true;
    }
    public override void PerformTask() {
        if(!CanPerformTask()){
            return;
        }
        if (_currentState != null) {
            _currentState.PerformStateAction();
        }
        if (_daysLeft == 0) {
            EndTask(TASK_STATUS.FAIL);
            return;
        }
        ReduceDaysLeft(1);
    }
    public override int GetSelectionWeight(ECS.Character character) {
		if (_parentQuest is FindLostHeir || _parentQuest is EliminateLostHeir) {
            return 100;
		}else if (_parentQuest is TheDarkRitual) {
			return 150;
		}else if (_parentQuest is PsytoxinCure) {
			return 150;
		}
        return 0;
    }
	protected override BaseLandmark GetLandmarkTarget(ECS.Character character) {
		base.GetLandmarkTarget(character);
		if(onGetTargetLandmarkAction != null){
			return onGetTargetLandmarkAction (character);
		}
		return null;
	}
    #endregion

	private void StartSearch(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartSearch ());
//			return;
//		}
        Log startSearchLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", "start_search");
        startSearchLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startSearchLog.AddToFillers(null, searchingForLog, LOG_IDENTIFIER.OTHER);

		_targetLandmark.AddHistory(startSearchLog);
        _assignedCharacter.AddHistory(startSearchLog);

		ChangeStateTo(STATE.SEARCH);
    }

    #region Get Landmark Target
	private BaseLandmark GetLandmarkForCharacterSearching(Character character){
		Region regionLocation = character.specificLocation.tileLocation.region;
		if(_searchingFor is string){
			string itemName = _searchingFor as string;
			Character characterLookingFor = _assignedCharacter.GetCharacterFromTraceInfo(itemName);
			for (int i = 0; i < regionLocation.landmarks.Count; i++) {
				BaseLandmark currLandmark = regionLocation.landmarks[i];
				int weight = 0;
				weight += currLandmark.charactersAtLocation.Count * 20;//For each character in a landmark in the current region: +20
				if (currLandmark.HasHostilitiesWith(character.faction)) {
					weight -= 50;//If landmark has hostile characters: -50
				}
				if(characterLookingFor != null && characterLookingFor.specificLocation != null && characterLookingFor.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
					BaseLandmark landmark = characterLookingFor.specificLocation as BaseLandmark;
					if(landmark.id == currLandmark.id){
						weight += 500; //If assigned character has a trace info of character he is looking for, and is in this landmark
					}
				}
				for (int j = 0; j < currLandmark.charactersAtLocation.Count; j++) {
					ECS.Character characterThatHasItem = currLandmark.charactersAtLocation [j];
					if(characterThatHasItem.GetItemInInventory(itemName) != null){
						weight += 100;
					}
				}
				//If this character has already Searched in the landmark within the past 6 months: -60
				if (weight > 0) {
					_landmarkWeights.AddElement(currLandmark, weight);
				}
			}
			if(_landmarkWeights.GetTotalOfWeights() > 0){
				return _landmarkWeights.PickRandomElementGivenWeights ();
			}
		}

		return null;
	}

	private BaseLandmark GetLandmarkForLandmarkItemsSearching(Character character){
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.landmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.landmarks[i];
			int weight = 0;
			weight += currLandmark.itemsInLandmark.Count * 20;//For each item in a landmark in the current region: +20
			if (currLandmark.HasHostilitiesWith(character.faction)) {
				weight -= 50;//If landmark has hostile characters: -50
			}
			//If this character has already Searched in the landmark within the past 6 months: -60
			if (weight > 0) {
				_landmarkWeights.AddElement(currLandmark, weight);
			}
		}
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
	#endregion

	#region Are Conditions Met Action
	private bool AreConditionsMetForCharacterSearching(Character character){
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.landmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.landmarks[i];
			if(currLandmark.charactersAtLocation.Count > 0) {
				return true;
			}
		}
		return false;
	}
	private bool AreConditionsMetForLandmarkItemsSearching(Character character){
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.landmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.landmarks[i];
			if(currLandmark.itemsInLandmark.Count > 0) {
				return true;
			}
		}
		return false;
	}
	#endregion

    #region Logs
    public override string GetArriveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", "arrive_action");
        arriveLog.AddToFillers(null, searchingForLog, LOG_IDENTIFIER.OTHER);
        return Utilities.LogReplacer(arriveLog);
    }
    public override string GetLeaveActionString() {
        Log arriveLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Search", "leave_action");
        arriveLog.AddToFillers(null, searchingForLog, LOG_IDENTIFIER.OTHER);
        return Utilities.LogReplacer(arriveLog);
    }
    #endregion
}
