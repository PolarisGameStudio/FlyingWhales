﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UpgradeGear : CharacterTask {

    private Settlement _settlement;

	public UpgradeGear(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.UPGRADE_GEAR, stance, defaultDaysLeft) {
    }
    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = _assignedCharacter.GetNearestSettlementFromFaction();
		}
		if(_targetLocation != null){
			_settlement = (Settlement)_targetLocation;
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS);
		}else{
			EndTask (TASK_STATUS.SUCCESS);
		}
	}
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();
		PurchaseEquipment ();
    }
	public override bool CanBeDone (ECS.Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && character.faction != null && location.tileLocation.landmarkOnTile is Settlement){
			Settlement settlement = (Settlement)location.tileLocation.landmarkOnTile;
			if(settlement.owner != null && settlement.owner.id == character.faction.id){
				return true;
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (ECS.Character character){
		if(character.faction != null && character.faction.settlements.Count > 0){
			return true;
		}
		return base.AreConditionsMet (character);
	}
    //public override void TaskSuccess() {
    //    if (_assignedCharacter.faction == null) {
    //        _assignedCharacter.UnalignedDetermineAction();
    //    } else {
    //        _assignedCharacter.DetermineAction();
    //    }
    //}
    #endregion

//	private void SchedulePurchaseEquipment(){
//		GameDate newSched = GameManager.Instance.Today ();
//		newSched.AddDays (1);
//		SchedulingManager.Instance.AddEntry (newSched, () => PurchaseEquipment ());
//	}
    private void PurchaseEquipment() {
        List<ECS.Character> charactersToPurchase = new List<ECS.Character>();
        if(_assignedCharacter.party == null) {
            charactersToPurchase.Add(_assignedCharacter);
        } else {
            charactersToPurchase.AddRange(_assignedCharacter.party.partyMembers);
        }
        //Purchase equipment from the settlement
        for (int i = 0; i < charactersToPurchase.Count; i++) {
            ECS.Character currChar = charactersToPurchase[i];
            List<EQUIPMENT_TYPE> neededEquipment = currChar.GetNeededEquipmentTypes();
            for (int j = 0; j < neededEquipment.Count; j++) {
                if (currChar.gold <= 0) {
                    //the curr character no longer has any money
                    break;
                }
                EQUIPMENT_TYPE equipmentToAskFor = neededEquipment[j];
                ECS.Item createdItem = _settlement.ProduceItemForCharacter(equipmentToAskFor, currChar);
                if (createdItem != null) {
					if(!currChar.EquipItem(createdItem)){ //if the character can equip the item, equip it, otherwise, keep in inventory
						currChar.PickupItem(createdItem); //put item in inventory
					}
					//currChar.AddHistory("Bought a " + createdItem.itemName + " from " + _settlement.landmarkName);
                    Debug.Log(currChar.name + " bought a " + createdItem.itemName + " from " + _settlement.landmarkName);
                }
            }
        }
        EndTask(TASK_STATUS.SUCCESS);
    }
}
