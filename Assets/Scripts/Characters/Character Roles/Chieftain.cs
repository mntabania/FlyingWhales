﻿/*
 The head of a tribe. 
 The chieftain performs various actions like defending his cities and attacking enemy cities. 
 He also explores his tribe territory. He creates various Quests for other characters.
 This role is considered to be the highest of roles (Like a King).
 Place functions unique to chieftains here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chieftain : CharacterRole {

	public Chieftain(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.CHIEFTAIN;
        _allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        _canPassHiddenRoads = true;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.ATTACK,
            QUEST_TYPE.DEFEND,
            QUEST_TYPE.EXPLORE_TILE,
			QUEST_TYPE.EXPEDITION,
			QUEST_TYPE.SAVE_LANDMARK,
        };

		_roleTasks.Add (new DoNothing (this._character));
		_roleTasks.Add (new Rest (this._character));
		_roleTasks.Add (new ExploreTile (this._character, 5));
		_roleTasks.Add (new UpgradeGear (this._character));
		_roleTasks.Add (new MoveTo (this._character));
		_roleTasks.Add (new TakeQuest (this._character));
		_roleTasks.Add (new RecruitFollowers (this._character, 5));
    }

	#region Overrides
//    internal override int GetExploreTileWeight(ExploreTile exploreTileQuest) {
//        int weight = 0;
//        weight += 100; //Change algo if needed
//        return weight;
//    }
//    internal override int GetExpeditionWeight(Expedition expedition) {
//        return 100;
//    }
//	internal override int GetSaveLandmarkWeight(ObtainMaterial obtainMaterial) {
//		return 200;
//	}
	#endregion
}
