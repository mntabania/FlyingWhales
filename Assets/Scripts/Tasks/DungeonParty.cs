using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonParty : Party {

	Party encounteredByParty;

	public DungeonParty(ECS.Character partyLeader, bool mustBeAddedToPartyList = true): base (partyLeader, mustBeAddedToPartyList) {
		
	}

	public override void AddPartyMember(ECS.Character member) {
		if (!_partyMembers.Contains(member)) {
			if(member.party != null){
				member.party.DisbandParty ();
				Debug.Log ("DISBANDING " + member.party.name + " before adding " + member.name + " to " + _name);
			}else{
				//if (member.avatar != null) {
				//	member.avatar.RemoveCharacter(member);
				//}
			}
			_partyMembers.Add(member);
			if(member.id != _partyLeader.id){
				_followers.Add (member);
				//if(_partyLeader.avatar != null){
				//	_partyLeader.avatar.AddNewCharacter (member);
				//}
			}
			member.SetParty(this);
			member.specificLocation.RemoveCharacterFromLocation (member);
		}
	}
	/*
     Remove a character from this party.
         */
	public override void RemovePartyMember(ECS.Character member, bool forDeath = false) {
		_partyMembers.Remove(member);
		_followers.Remove (member);
		//if(member.avatar != null) {
		//	member.avatar.RemoveCharacter(member);
		//}
		if(!forDeath){
			member.specificLocation.AddCharacterToLocation(member);
		}
		member.SetParty(null);
		//member.SetCurrentTask (null);
		if(member.isFollower){
			member.SetFollowerState (false);
//			member.isFollowerOf.RemoveFollower (member);
		}
		//if (_partyMembers.Count <= 0) {
		//	this.specificLocation.RemoveCharacterFromLocation(this);
		//}
	}

	//public override void StartEncounter(Party encounteredBy){
	//	this.encounteredByParty = encounteredBy;
	//	ECS.CombatPrototype combat = new ECS.CombatPrototype (encounteredBy, this, this.currLocation);
	//	combat.AddCharacters (ECS.SIDES.A, encounteredBy.partyMembers);
	//	combat.AddCharacters (ECS.SIDES.B, this._partyMembers);
	//	CombatThreadPool.Instance.AddToThreadPool (combat);
	//}

	//public override void ReturnResults (object result){
	//	if(result is ECS.CombatPrototype){
	//		ECS.CombatPrototype combat = (ECS.CombatPrototype)result;
	//		encounteredByParty.currentTask.AddNewLogs(combat.resultsLog);
	//		for (int i = 0; i < encounteredByParty.partyMembers.Count; i++) {
	//			//encounteredByParty.partyMembers [i].AddHistory ("Encountered " + this._name + ".", combat);
	//		}
	//		//if(combat.charactersSideA.Count > 0) {
	//		//	((OldQuest.Quest)encounteredByParty.currentTask).Result (true);
	//		//} else {
	//		//	((OldQuest.Quest)encounteredByParty.currentTask).Result (false);
	//		//}
	//	}
	//}
}
