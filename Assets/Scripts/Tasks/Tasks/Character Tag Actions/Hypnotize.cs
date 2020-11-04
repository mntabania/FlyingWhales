using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Hypnotize : CharacterTask {

	private ECS.Character _targetCharacter;
	private BaseLandmark _targetLandmark;

	public Hypnotize(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) 
		: base(createdBy, TASK_TYPE.HYPNOTIZE, stance, defaultDaysLeft) {
        _alignments.Add(ACTION_ALIGNMENT.VILLAINOUS);
        _alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);
        _needsSpecificTarget = true;
		_specificTargetClassification = "character";
		_filters = new TaskFilter[] {
			new MustNotHaveTags (CHARACTER_TAG.HYPNOTIZED),
		};
        _states = new Dictionary<STATE, State>() {
            {STATE.MOVE, new MoveState(this) },
            {STATE.HYPNOTIZE, new HypnotizeState(this) }
        };
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_specificTarget == null) {
			_specificTarget = GetCharacterTarget(character);
        }
		if(_specificTarget != null){
			_targetCharacter = (ECS.Character)_specificTarget;
			if (_targetLocation == null){
				_targetLocation = _targetCharacter.specificLocation;
			}
			if(_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
                ChangeStateTo(STATE.MOVE);
				_targetLandmark = _targetLocation as BaseLandmark;
				_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartHypnotize());
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(character.party == null || (!character.party.isFull && !character.party.isDisbanded)){
			if(location.tileLocation.landmarkOnTile != null){
				for (int j = 0; j < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; j++) {
					ECS.Character possibleCharacter = location.tileLocation.landmarkOnTile.charactersAtLocation[j];
					if(possibleCharacter.id != character.id && CanMeetRequirements(possibleCharacter)){
						return true;
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		if (character.party == null || (!character.party.isFull && !character.party.isDisbanded)) {
			return true;
		}
		for (int i = 0; i < character.specificLocation.tileLocation.region.landmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.landmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 5 * character.missingFollowers;//+5 for every missing Follower
    }
    protected override ECS.Character GetCharacterTarget(ECS.Character character) {
        base.GetCharacterTarget(character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.charactersInRegion.Count; i++) {
			ECS.Character currCharacter = character.specificLocation.tileLocation.region.charactersInRegion[i];
			if (currCharacter.id != character.id) {
				if (CanMeetRequirements(currCharacter)) {
					_characterWeights.AddElement(currCharacter, 50);//Each character in the same region: 50
				}
			}
		}
        LogTargetWeights(_characterWeights);
        if (_characterWeights.GetTotalOfWeights() > 0){
			return _characterWeights.PickRandomElementGivenWeights ();
		}
        return null;
    }
    #endregion

    private void StartHypnotize(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartHypnotize ());
//			return;
//		}
		if(_targetCharacter.specificLocation == _targetLandmark){
            ChangeStateTo(STATE.HYPNOTIZE);
            Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Hypnotize", "start");
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
