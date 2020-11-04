using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Attack : CharacterTask {

	private Character _targetCharacter;

	#region getters/setters
	public Character targetCharacter {
		get { return _targetCharacter; }
	}
	#endregion

	public Attack(TaskCreator createdBy, Character targetCharacter, int defaultDaysLeft = -1, Quest parentQuest = null, STANCE stance = STANCE.COMBAT) : base(createdBy, TASK_TYPE.ATTACK, stance, defaultDaysLeft, parentQuest) {
		_alignments.Add(ACTION_ALIGNMENT.HOSTILE);
		_needsSpecificTarget = true;
		_specificTarget = targetCharacter;
		_targetCharacter = targetCharacter;
//		_targetCharacter.AddActionOnDeath (EndTaskSuccess);

		_filters = new TaskFilter[] {
			new MustBeCharacter (new List<Character> (){ _targetCharacter })
		};

		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.ATTACK, new AttackState (this, () => WonAttack()) }
		};
		SetCombatPriority (30);
	}

	#region overrides
	public override CharacterTask CloneTask (){
		Attack clonedTask = new Attack(_createdBy, _targetCharacter, _defaultDaysLeft, _parentQuest, _stance);
		clonedTask.SetForGameOnly (_forGameOnly);
		clonedTask.SetForPlayerOnly (_forPlayerOnly);
		return clonedTask;
	}
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetCharacter != null){
			_targetLocation = _targetCharacter.specificLocation;
			if(_targetLocation != null){
				ChangeStateTo (STATE.MOVE);
				_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartAttack());
			}else{
				EndTaskFail ();
			}
		}else{
			EndTaskFail ();
		}
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		if (!_targetCharacter.isDead) {
			if (_currentState != null) {
				_currentState.PerformStateAction ();
			}
		} else {
			EndTaskSuccess ();
			return;
		}
		if(_daysLeft == 0 || _targetCharacter.specificLocation != _assignedCharacter.specificLocation){
			EndTaskFail ();
			return;
		}
		ReduceDaysLeft(1);
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(_targetCharacter.specificLocation != null){
			if(_targetCharacter.specificLocation == location){
				return true;
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		return true;
	}
	public override int GetSelectionWeight(Character character) {
		if(character.currentRegion.id == _targetCharacter.currentRegion.id){
			if(_targetCharacter.specificLocation != null && _targetCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
				return 200;
			}
		}
		return 30;
	}
	protected override BaseLandmark GetLandmarkTarget(Character character) {
		base.GetLandmarkTarget(character);
		if(_targetCharacter.specificLocation != null && _targetCharacter.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			return _targetCharacter.specificLocation as BaseLandmark;
		}
		return null;
	}
    public override bool HasHostilitiesBecauseOfTask(Character other) {
        if (targetCharacter.id == other.id) {
            return true;
        }
        //if (other is ECS.Character) {
        //    if (targetCharacter.id == (other as ECS.Character).id) {
        //        return true;
        //    }
        //} else if(other is Party) {
        //    if ((other as Party).partyMembers.Contains(targetCharacter)) {
        //        return true;
        //    }
        //}
        return base.HasHostilitiesBecauseOfTask(other);
    }
    #endregion

    private void StartAttack(){
		ChangeStateTo (STATE.ATTACK);
	}

	private void WonAttack(){
		EndTaskSuccess ();
	}
}
