using UnityEngine;
using System.Collections;
using ECS;

public class InvadeState : State {

	public InvadeState(CharacterTask parentTask): base (parentTask, STATE.INVADE){
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Invade ();
		return true;
	}
	#endregion

	private void Invade(){
		//KillCivilians ();
		ChangeLandmarkOwnership ();
	}
	private bool AreThereStillHostileInLandmark(){
		for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			Character character = _targetLandmark.charactersAtLocation [i];
            if (character.id != _assignedCharacter.id) {
                if (character.faction == null || _assignedCharacter.faction == null) {
                    return true;
                } else {
                    FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith(character.faction);
                    if (facRel != null) {
                        if (facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                            return true;
                        }
                    } else {
                        return true;
                    }
                }
            }
   //         if (character is Party){
			//	Party party = (Party)character;
			//	if(party.partyLeader.id != _assignedCharacter.id){
			//		if(party.faction == null || _assignedCharacter.faction == null){
			//			return true;
			//		}else{
			//			FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (party.faction);
			//			if(facRel != null){
			//				if(facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
			//					return true;
			//				}
			//			}else{
			//				return true;
			//			}
			//		}
			//	}
			//}else if (character is ECS.Character){
			//	ECS.Character currentCharacter = (ECS.Character)character;
			//	if(currentCharacter.id != _assignedCharacter.id){
			//		if(currentCharacter.faction == null || _assignedCharacter.faction == null){
			//			return true;
			//		}else{
			//			FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (currentCharacter.faction);
			//			if(facRel != null){
			//				if(facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
			//					return true;
			//				}
			//			}else{
			//				return true;
			//			}
			//		}
			//	}
			//}
		}
		return false;
	}
	private void ChangeLandmarkOwnership(){
		if(_parentTask is Invade && !((Invade)_parentTask).canChangeOwnership){
			return;
		}
		if(_targetLandmark is Settlement || _targetLandmark is ResourceLandmark){
			_targetLandmark.ChangeOwner (_assignedCharacter.faction);
		}
	}
}
