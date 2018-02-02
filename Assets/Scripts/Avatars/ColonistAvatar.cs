﻿using UnityEngine;
using System.Collections;

public class ColonistAvatar : CharacterAvatar {

	internal override void NewMove() {
		if(this.targetLocation.isOccupied && ((Expand)_characters [0].currentTask).targetUnoccupiedTile.id == this.targetLocation.id){
			_characters [0].currentTask.EndTask (TASK_STATUS.FAIL);
		}else{
			if (this.path.Count > 0) {
				this.MakeCitizenMove(this.currLocation, this.path[0]);
			}
		}

	}
}
