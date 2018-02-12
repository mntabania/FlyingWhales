﻿using UnityEngine;
using System.Collections;

public class ColonistAvatar : CharacterAvatar {

	internal override void NewMove() {
		if(this.targetLocation.tileLocation.isOccupied && ((Expand)_characters [0].currentTask).targetUnoccupiedTile.id == this.targetLocation.tileLocation.id){
			_characters [0].currentTask.EndTask (TASK_STATUS.FAIL);
		}else{
			if (this.path.Count > 0) {
				this.MakeCitizenMove(this.currLocation.tileLocation, this.path[0]);
			}
		}

	}
}
