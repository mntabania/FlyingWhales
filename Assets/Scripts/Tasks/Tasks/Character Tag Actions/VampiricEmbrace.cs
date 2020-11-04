using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class VampiricEmbrace : CharacterTask {

	private ECS.Character _targetCharacter;
	private BaseLandmark _targetLandmark;

	public VampiricEmbrace(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) 
		: base(createdBy, TASK_TYPE.VAMPIRIC_EMBRACE, stance, defaultDaysLeft) {
        _alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);
		_needsSpecificTarget = true;
		_specificTargetClassification = "character";
		_filters = new TaskFilter[] {
			new MustNotHaveTags (CHARACTER_TAG.VAMPIRE),
		};
        _states = new Dictionary<STATE, State>() {
            { STATE.MOVE, new MoveState(this) },
            { STATE.VAMPIRIC_EMBRACE, new VampiricEmbraceState(this) }
        };
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_specificTarget == null){
            _specificTarget = GetCharacterTarget(character);
            if (specificTarget == null) {
                EndTask(TASK_STATUS.FAIL);
                return;
            }
		}
		if(_specificTarget is ECS.Character){
			_targetCharacter = (ECS.Character)_specificTarget;
			if(_targetLocation == null){
				_targetLocation = _targetCharacter.specificLocation;
			}

			if (_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                ChangeStateTo(STATE.MOVE);
				_targetLandmark = _targetLocation as BaseLandmark;
				_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartVampiricEmbrace ());
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}

	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			for (int j = 0; j < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; j++) {
				ECS.Character possibleCharacter = location.tileLocation.landmarkOnTile.charactersAtLocation[j];
				if(possibleCharacter.id != character.id && CanMeetRequirements(possibleCharacter)){
					return true;
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.landmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.landmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 5;
    }
    protected override ECS.Character GetCharacterTarget(ECS.Character character) {
        base.GetCharacterTarget(character);
        List<Region> regionsToCheck = new List<Region>();
        Region regionOfCharacter = character.specificLocation.tileLocation.region;
        regionsToCheck.Add(regionOfCharacter);
        regionsToCheck.AddRange(regionOfCharacter.adjacentRegionsViaRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            for (int j = 0; j < currRegion.charactersInRegion.Count; j++) {
				ECS.Character currChar = currRegion.charactersInRegion[j];
                if (currChar.id != character.id) {
                    int weight = 0;
                    if (!currChar.HasTag(CHARACTER_TAG.VAMPIRE)) {
                        if (currChar.HasTag(CHARACTER_TAG.SAPIENT)) {
                            if (currRegion.id != regionOfCharacter.id) {
                                weight += 20; //Non Vampire Sapient Characters in adjacent regions: 20
                            } else {
                                weight += 50; //Non Vampire Sapient Characters in the current region: 50
                            }

                        } else {
                            if (currRegion.id == regionOfCharacter.id) {
                                weight += 5; //Non Vampire Non Sapient Characters in the current region: 5
                            }
                        }
                    }
                    if (weight > 0) {
                        _characterWeights.AddElement(currChar, weight);
                    }
                }
            }
        }
        LogTargetWeights(_characterWeights);
        if (_characterWeights.GetTotalOfWeights() > 0){
			return _characterWeights.PickRandomElementGivenWeights();
		}
		return null;
    }
    #endregion

    private void StartVampiricEmbrace(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartVampiricEmbrace ());
//			return;
//		}
		if(_targetCharacter.specificLocation == _targetLandmark){
            ChangeStateTo(STATE.VAMPIRIC_EMBRACE);
            Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "VampiricEmbrace", "start");
            startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            startLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            _targetLandmark.AddHistory(startLog);
            _targetCharacter.AddHistory(startLog);
            _assignedCharacter.AddHistory(startLog);
        } else {
            EndTask(TASK_STATUS.FAIL);
        }
	}
}
