﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Hibernate : CharacterTask {

    private List<Character> _charactersToRest;

	#region getters/setters
	public List<Character> charactersToRest{
		get { return _charactersToRest; }
	}
	#endregion

	public Hibernate(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.HIBERNATE, stance, defaultDaysLeft) {
		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.REST, new RestState (this) },
		};
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        //Get the characters that will rest
        _charactersToRest = new List<ECS.Character>();
        if (character.party != null) {
            _charactersToRest.AddRange(character.party.partyMembers);
        } else {
            _charactersToRest.Add(character);
        }

		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget (character);
		}
		if(_targetLocation != null){
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartHibernation());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			BaseLandmark home = character.home;
			if(home == null){
				home = character.lair;
			}
			if(home != null && location.tileLocation.landmarkOnTile.id == home.id){
				return true;
			}
//			if(character.faction == null){
//				
//			}else{
//				if(location.tileLocation.landmarkOnTile is Settlement && location.tileLocation.landmarkOnTile.owner != null){
//					Settlement settlement = (Settlement)location.tileLocation.landmarkOnTile;
//					if(settlement.owner.id == character.faction.id){
//						return true;
//					}
//				}
//			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		BaseLandmark home = character.home;
		if(home == null){
			home = character.lair;
		}
		if(home != null){
			return true;
		}
		return base.AreConditionsMet (character);
	}
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        restAction.DoAction(_assignedCharacter);
    //    }
    //}
    //public override void EndTask(TASK_STATUS taskResult) {
    //    SetCanDoDailyAction(false);
    //    base.EndTask(taskResult);
    //}

	protected override BaseLandmark GetLandmarkTarget (Character character){
//		base.GetLandmarkTarget (character);
		BaseLandmark home = character.home;
		if(home == null){
			home = character.lair;
		}
		if(home != null){
			return home;
		}
		return null;
	}
    #endregion
	private void StartHibernation(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartHibernation ());
//			return;
//		}
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Hibernate", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _assignedCharacter.AddHistory(startLog);
        if (_assignedCharacter.specificLocation is BaseLandmark) {
            (_assignedCharacter.specificLocation as BaseLandmark).AddHistory(startLog);
        }
		ChangeStateTo (STATE.REST);
	}
    //private void GoToTargetLocation() {
    //    // The monster will move towards its Lair and then rest there indefinitely
    //    GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
    //    goToLocation.InitializeAction(_assignedCharacter.lair);
    //    goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
    //    goToLocation.onTaskActionDone += StartHibernation;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;

    //    goToLocation.DoAction(_assignedCharacter);
    //}

    //private void StartHibernation() {
    //    //SetCanDoDailyAction(true);
    //    restAction = new RestAction(this);
    //    //restAction.onTaskActionDone += TaskSuccess; //rest indefinitely
    //    restAction.onTaskDoAction += restAction.RestIndefinitely;
    //    restAction.DoAction(_assignedCharacter);
    //}

}
