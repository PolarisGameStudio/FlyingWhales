﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Steal : CharacterTask {

	private Character _targetCharacter;

	#region getters/setters
	public Character targetCharacter{
		get { return _targetCharacter; }
	}
	#endregion

	public Steal(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.STEALTHY) : base(createdBy, TASK_TYPE.STEAL, stance, defaultDaysLeft) {
		_alignments.Add (ACTION_ALIGNMENT.UNLAWFUL);
		_specificTargetClassification = "character";
		_needsSpecificTarget = true;

		_filters = new TaskFilter[] {
			new MustHaveItem ()
		};

		_states = new Dictionary<STATE, State> {
			{STATE.MOVE, new MoveState(this)},
			{STATE.STEAL, new StealState(this)}
		};
	}

	#region overrides
	public override void OnChooseTask(Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if (_specificTarget == null) {
			_specificTarget = GetCharacterTarget(character);
		}
		if(_specificTarget != null && _specificTarget is Character){
			_targetCharacter = _specificTarget as Character;
			if (_targetLocation == null) {
				_targetLocation = _targetCharacter.specificLocation;
			}
			if (_targetLocation != null && _targetLocation is BaseLandmark) {
				ChangeStateTo(STATE.MOVE);
				_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, () => StartSteal());
			}else{
				EndTask (TASK_STATUS.FAIL);
			}
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
	public override bool CanBeDone(Character character, ILocation location) {
		if(location is BaseLandmark) {
			for (int i = 0; i < location.charactersAtLocation.Count; i++) {
				ICombatInitializer initializer = location.charactersAtLocation [i];
				if(initializer is Party){
					Party party = initializer as Party;
					for (int j = 0; j < party.partyMembers.Count; j++) {
						Character currCharacter = party.partyMembers [j];
						if(CanMeetRequirements(currCharacter)){
							return true;
						}
					}
				}else if(initializer is Character){
					Character currCharacter = initializer as Character;
					if(CanMeetRequirements(currCharacter)){
						return true;
					}
				}
			}
		}
		return base.CanBeDone(character, location);
	}
	public override bool AreConditionsMet(Character character) {
		for (int i = 0; i < character.currentRegion.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.currentRegion.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet(character);
	}
	public override int GetSelectionWeight(Character character) {
		return 30;
	}
	protected override Character GetCharacterTarget(Character character) {
		base.GetCharacterTarget(character);
		int weight = 0;
		for (int i = 0; i < character.currentRegion.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.currentRegion.allLandmarks [i];
			for (int j = 0; j < landmark.charactersAtLocation.Count; j++) {
				ICombatInitializer initializer = landmark.charactersAtLocation [j];
				weight = 0;
				if(initializer is Party){
					Party party = initializer as Party;
					for (int k = 0; k < party.partyMembers.Count; k++) {
						Character currCharacter = party.partyMembers [k];
						weight += (currCharacter.inventory.Count * 10);
					}
				}else if(initializer is Character){
					Character currCharacter = initializer as Character;
					weight += (currCharacter.inventory.Count * 10);
				}
				if(initializer.mainCharacter.faction == null || character.faction == null){
					weight += 100;
				}else if(initializer.mainCharacter.faction.id != character.faction.id){
					weight += 100;
				}
				if(weight > 0){
					_characterWeights.AddElement (initializer.mainCharacter, weight);
				}
			}
		}
		if (_characterWeights.GetTotalOfWeights() > 0) {
			return _characterWeights.PickRandomElementGivenWeights();
		}
		return null;
	}
	#endregion

	private void StartSteal(){
		ChangeStateTo (STATE.STEAL);
	}
}
