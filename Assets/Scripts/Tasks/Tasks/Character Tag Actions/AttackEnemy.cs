using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class AttackEnemy : CharacterTask {
	private Character _targetCharacter;

	public AttackEnemy(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.COMBAT) : base(createdBy, TASK_TYPE.ATTACK_ENEMY, stance, defaultDaysLeft) {
		_specificTargetClassification = "character";
		_needsSpecificTarget = true;
		_alignments.Add (ACTION_ALIGNMENT.HOSTILE);
        _filters = new TaskFilter[] {
            new MustBeRelationship(CHARACTER_RELATIONSHIP.ENEMY),
        };
        _states = new Dictionary<STATE, State> {
			{STATE.MOVE, new MoveState(this)},
			{STATE.ATTACK, new AttackState(this, null)}
		};
		SetCombatPriority (28);
	}

	#region overrides
	public override void OnChooseTask(Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if (_targetCharacter == null) {
			_targetCharacter = GetCharacterTarget(character);
		}
		if(_targetCharacter != null){
			_specificTarget = _targetCharacter;
			if (_targetLocation == null) {
				_targetLocation = _targetCharacter.specificLocation;
			}
			if (_targetLocation != null && _targetLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
				ChangeStateTo(STATE.MOVE);
				_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartAttackEnemy());
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone(Character character, ILocation location) {
		if(location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
			for (int i = 0; i < location.charactersAtLocation.Count; i++) {
				Character targetCharacter = location.charactersAtLocation [i];
				if(CanMeetRequirements(targetCharacter, character)){
					return true;
				}
			}
		}
		return base.CanBeDone(character, location);
	}
	public override bool AreConditionsMet(Character character) {
		for (int i = 0; i < character.currentRegion.landmarks.Count; i++) {
			BaseLandmark landmark = character.currentRegion.landmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet(character);
	}
	public override int GetSelectionWeight(Character character) {
		return 30;
	}
	protected override Character GetCharacterTarget(Character character) {
		base.GetCharacterTarget(character);
		int weight = 0;
		for (int i = 0; i < character.currentRegion.landmarks.Count; i++) {
			BaseLandmark landmark = character.currentRegion.landmarks [i];
			for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
				Character targetCharacter = landmark.charactersAtLocation [j];
				Relationship relationship = character.GetRelationshipWith (targetCharacter);
				//if(relationship != null && relationship.HasStatus(CHARACTER_RELATIONSHIP.ENEMY)){
				//	weight = 50;
				//	if(targetCharacter.faction == null || character.faction == null){
				//		weight += 50;
				//	}else if(targetCharacter.faction.id != character.faction.id){
				//		weight += 50;
				//	}
				//	if(relationship.HasCategory(CHARACTER_RELATIONSHIP_CATEGORY.POSITIVE)){
				//		weight -= 20;
				//	}
				//	if(relationship.HasCategory(CHARACTER_RELATIONSHIP_CATEGORY.FAMILIAL)){
				//		weight -= 20;
				//	}

				//	if(weight < 0){
				//		weight = 0;
				//	}
				//	_characterWeights.AddElement (targetCharacter, weight);
				//}
			}
		}
		LogTargetWeights(_characterWeights);
		if (_characterWeights.GetTotalOfWeights() > 0) {
			return _characterWeights.PickRandomElementGivenWeights();
		}
		return null;
	}
	#endregion

	private void StartAttackEnemy(){
		ChangeStateTo (STATE.ATTACK);
	}
}
