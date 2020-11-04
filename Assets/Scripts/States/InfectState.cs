using UnityEngine;
using System.Collections;
using System;

public class InfectState : State {
	private Action _infectAction;

	public InfectState(CharacterTask parentTask, string identifier): base (parentTask, STATE.INFECT){
		if(identifier == "Psytoxin"){
			_infectAction = InfectPsytoxinToLandmark;
		}
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		if(_infectAction != null){
			_infectAction ();
		}
		return true;
	}
	#endregion

	private void InfectPsytoxinToLandmark(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => InfectPsytoxinToLandmark ());
//		}
		for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
            ECS.Character character = _targetLandmark.charactersAtLocation[i];
            if (!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)) {
                if (character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)) {
                    continue;
                }
                InfectPsytoxin(character);
            }
   //         if (_targetLandmark.charactersAtLocation[i] is Party){
			//	Party party = (Party)_targetLandmark.charactersAtLocation [i];
			//	for (int j = 0; j < party.partyMembers.Count; j++) {
			//		ECS.Character character = party.partyMembers [j];
			//		if(!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
			//			if(character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)){
			//				continue;
			//			}
			//			InfectPsytoxin (character);
			//		}
			//	}
			//}else if(_targetLandmark.charactersAtLocation[i] is ECS.Character){
			//	ECS.Character character = (ECS.Character) _targetLandmark.charactersAtLocation[i];
			//	if(!character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
			//		if(character.role != null && (character.role.roleType == CHARACTER_ROLE.SLYX || character.role.roleType == CHARACTER_ROLE.CRATER_BEAST)){
			//			continue;
			//		}
			//		InfectPsytoxin (character);
			//	}
			//}
		}
		_assignedCharacter.Death ();
		Log destructLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "CommandInfection", "self_destruct");
		_targetLandmark.AddHistory(destructLog);
	}
	private void InfectPsytoxin(ECS.Character character){
		if(character.HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
			return;	
		}
		ModeratePsytoxin modPsytoxin = (ModeratePsytoxin)character.GetTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
		if(modPsytoxin != null){
			modPsytoxin.TriggerWorsenCase ();
		}else{
			MildPsytoxin mildPsytoxin = (MildPsytoxin)character.GetTag (CHARACTER_TAG.MILD_PSYTOXIN);
			if(mildPsytoxin != null){
				mildPsytoxin.TriggerWorsenCase ();
			}else{
				int chance = Utilities.rng.Next (0, 100);
				if(chance < 80){
					character.AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);	
				}
			}
		}
	}
}
