using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class CommandInfection : CharacterTask {
	//private CraterBeast craterBeast;
	//private BaseLandmark _targetLandmark;
	private Character _chosenSlyx;
	private WeightedDictionary<Region> _regionWeights;

	#region getters/setters
	public Character chosenSlyx{
		get { return _chosenSlyx; }
	}
	#endregion
	public CommandInfection(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) : base(createdBy, TASK_TYPE.COMMAND_INFECTION, stance, defaultDaysLeft) {
		_regionWeights = new WeightedDictionary<Region> ();
		_states = new Dictionary<STATE, State>{
			{STATE.COMMAND_INFECTION, new CommandInfectionState(this)}
		};
	}

	#region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget (character);
		}
		if(_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
				if(character.specificLocation.charactersAtLocation[i] is Character){
					Character currCharacter = character.specificLocation.charactersAtLocation[i] as Character;
					//if(currCharacter.role != null && currCharacter.role.roleType == CHARACTER_ROLE.SLYX){
					//	_chosenSlyx = currCharacter;
					//	break;
					//}
				}
			}
			//_targetLandmark = (BaseLandmark)_targetLocation;
			//craterBeast = (CraterBeast)_assignedCharacter.role;
			ChangeStateTo (STATE.COMMAND_INFECTION, true);
		}else{
			EndTask(TASK_STATUS.FAIL);
		}
	}
	public override void EndTaskSuccess (){
		_currentState.SetIsHalted (false);
		_currentState.PerformStateAction ();
		base.EndTaskSuccess ();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			BaseLandmark landmark = (BaseLandmark)location;
			if(landmark is Settlement){
				for (int i = 0; i < character.specificLocation.charactersAtLocation.Count; i++) {
					if(character.specificLocation.charactersAtLocation[i] is Character){
						Character currCharacter = (Character) character.specificLocation.charactersAtLocation[i];
						//if(currCharacter.role != null && currCharacter.role.roleType == CHARACTER_ROLE.SLYX){
						//	return true;
						//}
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			HexTile center = GridMap.Instance.allRegions [i].centerOfMass;
			if(center.landmarkOnTile != null && center.landmarkOnTile.charactersAtLocation.Count > 0){
				if(CanBeDone(character, center.landmarkOnTile)){
					return true;
				}
			}
		}
		return base.AreConditionsMet (character);
	}
	public override int GetSelectionWeight (Character character){
		return 100;
	}
	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		_regionWeights.Clear ();
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region region = GridMap.Instance.allRegions [i];
			if(region.id != character.specificLocation.tileLocation.region.id){
				int numOfCharacters = region.numOfCharactersInLandmarks;
				if(numOfCharacters > 0){
					_regionWeights.AddElement (region, (numOfCharacters * 5));
				}
			}
		}
		if(_regionWeights.Count > 0){
			Region pickedRegion = _regionWeights.PickRandomElementGivenWeights ();
			for (int i = 0; i < pickedRegion.landmarks.Count; i++) {
				BaseLandmark landmark = pickedRegion.landmarks [i];
				if(landmark.charactersAtLocation.Count > 0){
					int landmarkWeight = 0;
					for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
                        Character currCharacter = landmark.charactersAtLocation[j];
                        landmarkWeight += CharacterWeight(currCharacter);
      //                  if (landmark.charactersAtLocation[j] is Party){
						//	Party party = (Party)landmark.charactersAtLocation [j];
						//	for (int k = 0; k < party.partyMembers.Count; k++) {
						//		landmarkWeight += CharacterWeight (party.partyMembers [k]);
						//	}
						//}else if(landmark.charactersAtLocation[j] is Character){
						//	Character currCharacter = (Character)landmark.charactersAtLocation [j];
						//	landmarkWeight += CharacterWeight (currCharacter);
						//}
					}
					_landmarkWeights.AddElement (landmark, landmarkWeight);
				}
			}
            LogTargetWeights(_landmarkWeights);
            if (_landmarkWeights.Count > 0){
				return _landmarkWeights.PickRandomElementGivenWeights ();
			}
		}
		return null;
	}
	#endregion

	private int CharacterWeight(Character character){
		if(character.HasTag(CHARACTER_TAG.MILD_PSYTOXIN)){
			return 3;
		}else if(character.HasTag(CHARACTER_TAG.MODERATE_PSYTOXIN)){
			return 8;
		}else if(character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
			return -2;
		}
		return 5;
	}
}
