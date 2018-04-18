﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackCity : GameEvent {

	internal General general;
	internal Battle battle;
	internal City sourceCity;
	internal City targetCity;
	internal Kingdom sourceKingdom;
	internal List<ReinforceCity> reinforcements;

	public AttackCity(int startDay, int startMonth, int startYear, Citizen startedBy, Battle battle, City targetCity) : base (startDay, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ATTACK_CITY;
		this.name = "Attack City";
		this.general = (General)startedBy.assignedRole;
		this.battle = battle;
		this.sourceCity = startedBy.city;
		this.targetCity = targetCity;
		this.sourceKingdom = this.sourceCity.kingdom;
		this.reinforcements = new List<ReinforceCity> ();
	}

	internal void AddReinforcements(ReinforceCity reinforceCity){
		this.reinforcements.Add (reinforceCity);
		reinforceCity.attackCity = this;
		reinforceCity.battle = this.battle;
	}
	internal void RemoveReinforcements(ReinforceCity reinforceCity, bool skipAttack = false){
		this.reinforcements.Remove (reinforceCity);
		if(!skipAttack){
			if(this.reinforcements.Count <= 0){
				Attack ();
			}
		}
	}
	internal void TransferReinforcements(ReinforceCity reinforceCity){
		this.general.AdjustSoldiers (reinforceCity.general.soldiers);
		RemoveReinforcements (reinforceCity);
	}
	internal void Attack(){
		this.general.isIdle = false;
		if(this.general.path != null && this.general.path.Count > 0){
			this.general.citizenAvatar.StartMoving ();
		}
		this.battle.Attack ();
	}
	internal void ReturnRemainingSoldiers (){
		if(this.isActive){
			if(this.general.location.id == this.sourceCity.hexTile.id){
//				if(!this.sourceCity.isDead){
//					this.sourceCity.AdjustSoldiers (this.general.soldiers);
//				}
				this.DoneEvent ();
			}else{
				if(!this.sourceCity.isDead){
					ReturnSoldiers (this.sourceCity);
				}else{
					if(!this.sourceKingdom.isDead){
						for (int i = 0; i < this.sourceCity.region.connections.Count; i++) {
							if(this.sourceCity.region.connections[i] is Region){
								City city = ((Region)this.sourceCity.region.connections [i]).occupant;
								if(city != null && city.kingdom.id == this.sourceKingdom.id && Utilities.HasPath(this.general.location, city.hexTile, PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM, this.sourceKingdom)){
									ReturnSoldiers (city);
									return;
								}
							}
						}
						this.DoneEvent ();
					}else{
						this.DoneEvent ();
					}
				}
			}
		}
	}
	private void ReturnSoldiers(City targetCity){
		this.general.isReturning = true;
		this.general.targetCity = targetCity;
		this.general.targetLocation = targetCity.hexTile;
		this.general.citizenAvatar.SetHasArrivedState (false);
		this.general.citizenAvatar.CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
	}
	private void CancelAllReinforcements(){
		if(this.reinforcements.Count > 0){
			for (int i = 0; i < this.reinforcements.Count; i++) {
				this.reinforcements [i].CancelEvent ();
			}
		}
	}
	#region Overrides
	internal override void DoneCitizenAction(Citizen citizen){
//		if(this.general.willDropSoldiersAndDisappear){
//			this.general.DropSoldiersAndDisappear ();
//		}
//		if(citizen.id == this.general.citizen.id){
//			if(this.general.targetLocation.city == null || (this.general.targetLocation.city != null && this.general.targetLocation.city.id != this.general.targetCity.id)){
//				CancelEvent ();
//				return;
//			}

//			if(!this.general.isReturning){
//				if(!this.targetCity.isDead){
//					battle.Combat ();
//				}else{
//					CancelEvent ();
//				}	
//			}else{
//				if(!this.general.targetCity.isDead){
//					this.general.targetCity.AdjustSoldiers (this.general.soldiers);
//					this.DoneEvent ();
//				}else{
//					CancelEvent ();
//				}
//			}
//		}
	}
	internal override void DoneEvent(){
		base.DoneEvent();
		this.general.citizen.Death(DEATH_REASONS.BATTLE);
	}
	internal override void CancelEvent (){
		ReturnRemainingSoldiers ();
		CancelAllReinforcements ();
	}
	#endregion
}