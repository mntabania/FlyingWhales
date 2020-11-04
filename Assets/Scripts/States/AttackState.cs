using UnityEngine;
using System.Collections;
using System;
using ECS;

public class AttackState : State {
	Action _wonInCombatAction;
	Character _targetCharacter;

	public AttackState(CharacterTask parentTask, Action wonInCombatAction) : base(parentTask, STATE.ATTACK) {
		_wonInCombatAction = wonInCombatAction;
    }

    #region overrides
	public override void OnChooseState (Character character){
		base.OnChooseState (character);
		if(_parentTask.specificTarget != null){
			_targetCharacter = _parentTask.specificTarget as Character;
		}
	}
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        InitiateCombat();
        return true;
    }
	protected override void ResetState (){
		base.ResetState ();
		_targetCharacter = null;
	}
    #endregion

    private void InitiateCombat() {
        if (!_assignedCharacter.isInCombat && !_targetCharacter.isInCombat) {
			Character source = _assignedCharacter;
			Character target = _targetCharacter;
			//if(_assignedCharacter.party != null){
			//	source = _assignedCharacter.party;
			//}
			//if(_targetCharacter.party != null){
			//	target = _targetCharacter.party;
			//}
			_assignedCharacter.specificLocation.StartCombatBetween(source, target);
			//if(_wonInCombatAction != null){
			//	source.SetCurrentFunction (() => _wonInCombatAction ());
			//}
        }
    }
}
