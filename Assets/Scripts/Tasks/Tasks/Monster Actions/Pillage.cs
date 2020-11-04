using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS;

public class Pillage : CharacterTask {
    private BaseLandmark _target;

	//private string pillagerName;

	public Pillage(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.COMBAT) 
        : base(createdBy, TASK_TYPE.PILLAGE, stance, defaultDaysLeft) {
        _alignments.Add(ACTION_ALIGNMENT.HOSTILE);
        _alignments.Add(ACTION_ALIGNMENT.VILLAINOUS);
        _states = new System.Collections.Generic.Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.PILLAGE, new PillageState (this) }
		};
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
		if(_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			_target = _targetLocation as BaseLandmark;
			//pillagerName = _assignedCharacter.name;
			//if(_assignedCharacter.party != null){
			//	pillagerName = _assignedCharacter.party.name;
			//}
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartPillage ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
//    public override void TaskCancel() {
//        base.TaskCancel();
//        //Messenger.RemoveListener(Signals.HOUR_ENDED, DoPillage);
//        _assignedCharacter.DestroyAvatar();
////		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
////			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
////			settlement.CancelSaveALandmark (_target);
////		}
//    }
//    public override void TaskFail() {
//        base.TaskFail();
//        //Messenger.RemoveListener(Signals.HOUR_ENDED, DoPillage);
//        _assignedCharacter.DestroyAvatar();
////		if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
////			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
////			settlement.CancelSaveALandmark (_target);
////		}
//    }

	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.itemsInLandmark.Count > 0){
			if (location.tileLocation.landmarkOnTile is Settlement || location.tileLocation.landmarkOnTile is ResourceLandmark) {
				if (character.faction == null || location.tileLocation.landmarkOnTile.owner == null) {
					return true;
				} else {
					if (location.tileLocation.landmarkOnTile.owner.id != character.faction.id) {
						return true;
					}
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
        return 25;
    }
    protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.landmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.landmarks [i];
            int weight = 0;
            if ((landmark.owner == null || (character.faction == null || landmark.owner.id != character.faction.id))) {
                if (landmark is ResourceLandmark) {
                    weight += 80; //Resource Gathering Landmark not owned by my faction: +80
                } else if (landmark is Settlement) {
                    weight += 50; //Settlement Landmark not owned by my faction: +50
                }
            }
            for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
                ECS.Character currChar = landmark.charactersAtLocation[j];
                if (currChar.id != character.id) {
                    if (currChar.IsHostileWith(character)) {
                        weight -= 10; //For each Hostile characters in the landmark: -10
                    }
                }
            }
            if (weight > 0) {
                _landmarkWeights.AddElement(landmark, weight);
            }
            
        }
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
    #endregion

    private void StartPillage() {
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartPillage ());
//			return;
//		}

        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Pillage", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _target.AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);

		ChangeStateTo (STATE.PILLAGE);
    }
}
