﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Patrol : CharacterTask {

	private BaseLandmark _landmarkToPatrol;

	#region getters/setters
	public BaseLandmark landmarkToPatrol {
		get { return _landmarkToPatrol; }
	}
	#endregion

	public Patrol(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.COMBAT) 
		: base(createdBy, TASK_TYPE.PATROL, stance, defaultDaysLeft) {

		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.PATROL, new PatrolState (this) }
		};
		SetCombatPriority (5);
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetLandmarkTarget(character);
		}
		if (_targetLocation != null && _targetLocation is BaseLandmark) {
			ChangeStateTo (STATE.MOVE);
			_landmarkToPatrol = (BaseLandmark)_targetLocation;
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartPatrol ());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone (ECS.Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			if (location.tileLocation.landmarkOnTile is Settlement || location.tileLocation.landmarkOnTile is ResourceLandmark) {			
				if (character.faction != null && location.tileLocation.landmarkOnTile.owner != null) {
					if (location.tileLocation.landmarkOnTile.owner.id == character.faction.id) {
						return true;
					}					
				}else{
					if(character.home != null && character.home.id == location.tileLocation.landmarkOnTile.id){
						return true;
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
			}
//			if(landmark is Settlement || landmark is ResourceLandmark){
//				if(landmark.owner != null && landmark.owner.id == _assignedCharacter.faction.id){
//					_landmarkWeights.AddElement (landmark, 100);
//				}
//			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
	public override int GetSelectionWeight (Character character){
		return 20;
	}
	#endregion

	private void StartPatrol(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartPatrol ());
//			return;
//		}
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Patrol", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startLog.AddToFillers(_landmarkToPatrol, _landmarkToPatrol.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
        _landmarkToPatrol.AddHistory(startLog);
        _assignedCharacter.AddHistory(startLog);
        
		ChangeStateTo (STATE.PATROL);
	}
}
