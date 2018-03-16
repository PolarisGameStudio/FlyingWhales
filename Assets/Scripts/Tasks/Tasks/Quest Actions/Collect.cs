﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Collect : CharacterTask {

	private string _itemNameToCollect;
	private int _quantityToCollect;
	private int _quantityAlreadyCollected;

	public Collect(TaskCreator createdBy, string itemName, int quantity, ILocation targetLocation, int defaultDaysLeft = -1, Quest parentQuest = null) : base(createdBy, TASK_TYPE.COLLECT, defaultDaysLeft, parentQuest) {
		SetStance(STANCE.COMBAT);
		_targetLocation = targetLocation;
		_itemNameToCollect = itemName;
		_quantityToCollect = quantity;
		_quantityAlreadyCollected = 0;
	}

	#region overrides
	public override CharacterTask CloneTask() {
		Collect clonedTask = new Collect(_createdBy, _itemNameToCollect, _quantityToCollect, _targetLocation, _defaultDaysLeft, _parentQuest);
		return clonedTask;
	}
	public override bool CanBeDone(Character character, ILocation location) {
		if(location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
			BaseLandmark landmark = (BaseLandmark)location;
			if(landmark.HasItem(_itemNameToCollect)){
				return true;
			}
		}
		return false;
	}
	public override void OnChooseTask(Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if (_targetLocation == null) {
			WeightedDictionary<BaseLandmark> landmarkWeights = GetLandmarkTargetWeights(character);
			if (landmarkWeights.GetTotalOfWeights() > 0) {
				_targetLocation = landmarkWeights.PickRandomElementGivenWeights();
			} else {
				EndTask(TASK_STATUS.FAIL); //the character could not search anywhere, fail this task
				return;
			}
		}
		_assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartCollect());
	}
	public override bool AreConditionsMet(Character character) {
		//check if there are any landmarks in region with characters
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.allLandmarks[i];
			if(CanBeDone(character, currLandmark)) {
				return true;
			}
		}
		return base.AreConditionsMet(character);
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();
		CollectItem();
		if (_daysLeft == 0) {
			//EndRecruitment();
			EndTask(TASK_STATUS.FAIL);
			return;
		}
		ReduceDaysLeft(1);
	}
	public override int GetSelectionWeight(ECS.Character character) {
		if (_parentQuest is FindLostHeir) {
			return 80;
		}
		return 0;
	}
	protected override WeightedDictionary<BaseLandmark> GetLandmarkTargetWeights(ECS.Character character) {
		WeightedDictionary<BaseLandmark> landmarkWeights = new WeightedDictionary<BaseLandmark>();
		Region regionLocation = character.specificLocation.tileLocation.region;
		for (int i = 0; i < regionLocation.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = regionLocation.allLandmarks[i];
			if(CanBeDone(character, currLandmark)){
				int weight = 0;
				weight += currLandmark.charactersAtLocation.Count * 20;//For each character in a landmark in the current region: +20
				if (currLandmark.HasHostilitiesWith(character.faction)) {
					weight -= 50;//If landmark has hostile characters: -50
				}
				//If this character has already Searched in the landmark within the past 6 months: -60
				if (weight > 0) {
					landmarkWeights.AddElement(currLandmark, weight);
				}
			}
		}
		return landmarkWeights;
	}
	#endregion

	private void StartCollect(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartCollect ());
			return;
		}
		_assignedCharacter.DestroyAvatar ();
		if(_targetLocation is BaseLandmark){
			(targetLocation as BaseLandmark).AddHistory(_assignedCharacter.name + " started searching for " + _itemNameToCollect + " to collect!");
		}
	}

	private void CollectItem(){
		int collectedAmount = 0;
		int chance = 0;
		BaseLandmark _targetLandmark = (BaseLandmark)_targetLocation;
		for (int i = 0; i < _targetLandmark.itemsInLandmark.Count; i++) {
			ECS.Item item = _targetLandmark.itemsInLandmark [i];
			if(item.itemName == _itemNameToCollect){
				if(item.isUnlimited){
					int alreadyCollected = _quantityAlreadyCollected;
					for (int j = alreadyCollected; j < _quantityToCollect; j++) {
						chance = UnityEngine.Random.Range (0, 100);
						if(chance < item.collectChance){
							_assignedCharacter.PickupItem (item);
							_targetLandmark.RemoveItemInLandmark (item);
							_quantityAlreadyCollected++;
							collectedAmount++;
						}
					}
				}else{
					chance = UnityEngine.Random.Range (0, 100);
					if (chance < item.collectChance) {
						_assignedCharacter.PickupItem (item);
						_targetLandmark.RemoveItemInLandmark (item);
						_quantityAlreadyCollected++;
						collectedAmount++;
					}
				}
			}
		}

		_assignedCharacter.AddHistory ("Collected " + collectedAmount + " " + _itemNameToCollect + " in " + _targetLandmark.landmarkName + ".");
		_targetLandmark.AddHistory (_assignedCharacter.name + " collected " + collectedAmount + " " + _itemNameToCollect + ".");

		if(_quantityAlreadyCollected >= _quantityToCollect){
			EndTask (TASK_STATUS.SUCCESS);
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
}
