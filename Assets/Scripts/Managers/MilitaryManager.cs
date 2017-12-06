﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MilitaryManager {

	private Kingdom _kingdom;

	internal int maxGenerals;
	internal List<General> activeGenerals;

	public MilitaryManager(Kingdom kingdom){
		this._kingdom = kingdom;
		this.activeGenerals = new List<General> ();
		UpdateMaxGenerals ();
		ScheduleCreateGeneral ();
	}

	private void ScheduleCreateGeneral(){
		GameDate newDate = new GameDate (GameManager.Instance.month, 1, GameManager.Instance.year);
		newDate.AddMonths (1);

		SchedulingManager.Instance.AddEntry (newDate, () => CreateGeneral ());
	}
	private void CreateGeneral(){
		if (!this._kingdom.isDead) {
			if(this.activeGenerals.Count < maxGenerals){
				//Create General
				Citizen citizen = this._kingdom.capitalCity.CreateNewAgent(ROLE.GENERAL, this._kingdom.capitalCity.hexTile);
				if(citizen != null){
					General general = (General)citizen.assignedRole;
					general.Initialize (null);
					general.GetTask ();
					activeGenerals.Add (general);
				}
			}
			ScheduleCreateGeneral ();
		}
	}
	internal void UpdateMaxGenerals(){
		if(this._kingdom.kingdomSize == KINGDOM_SIZE.SMALL){
			this.maxGenerals = 2;
		}else if(this._kingdom.kingdomSize == KINGDOM_SIZE.MEDIUM){
			this.maxGenerals = 4;
		}else if(this._kingdom.kingdomSize == KINGDOM_SIZE.LARGE){
			this.maxGenerals = 6;
		}
		if(this._kingdom.king.otherTraits.Contains(TRAIT.MILITANT) || this._kingdom.king.otherTraits.Contains(TRAIT.HOSTILE)){
			this.maxGenerals += 1;
		}else if(this._kingdom.king.otherTraits.Contains(TRAIT.PACIFIST)){
			this.maxGenerals -= 1;
		}
	}

	internal void AssignTaskToGeneral(General general){
		GeneralTask task = CreateTask (general);
		if(task != null){
			general.AssignTask (task);
		}else{
			general.generalTask = null;
			GameDate checkDate = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
			checkDate.AddDays (5);
			SchedulingManager.Instance.AddEntry (checkDate, () => general.GetTask ());
		}
	}
	private GeneralTask CreateTask (General general){
		Dictionary<GENERAL_TASKS, int> tasksToChoose = new Dictionary<GENERAL_TASKS, int> ();
		City defendCity = null;
		City attackCity = null;
		int defendWeight = 0;
		int attackWeight = 0;

		if(this._kingdom.warfareInfo.Count > 0){
			GetDefendCityAndWeight (ref defendCity, ref defendWeight);
			GetAttackCityAndWeight (ref attackCity, ref attackWeight);
			if(attackCity != null){
				tasksToChoose.Add (GENERAL_TASKS.ATTACK_CITY, attackWeight);
			}
			if (defendCity != null) {
				tasksToChoose.Add (GENERAL_TASKS.DEFEND_CITY, defendWeight);
			}
		}else{
			GetDefendCityAndWeight (ref defendCity, ref defendWeight);
			if (defendCity != null) {
				tasksToChoose.Add (GENERAL_TASKS.DEFEND_CITY, defendWeight);
			}
		}

		if(tasksToChoose.Count > 0){
			GENERAL_TASKS task = Utilities.PickRandomElementWithWeights<GENERAL_TASKS> (tasksToChoose);
			if(task == GENERAL_TASKS.ATTACK_CITY){
				return new AttackCityTask (task, general, attackCity.hexTile);
			}else if(task == GENERAL_TASKS.DEFEND_CITY){
				return new DefendCityTask (task, general, defendCity.hexTile);
			}
		}
		return null;
	}

	private void GetDefendCityAndWeight(ref City defendCity, ref int weight){
		Dictionary<City, int> cityWeights = new Dictionary<City, int> ();
		for (int i = 0; i < this._kingdom.cities.Count; i++) {
			City city = this._kingdom.cities [i];
			if(city.assignedDefendGeneralsCount == 0){
				int cityTotalWeight = GetDefendCityWeight (city);
				if(cityTotalWeight > 0){
					cityWeights.Add (city, cityTotalWeight);
				}
			}
		}

		if(cityWeights.Count <= 0){
			for (int i = 0; i < this._kingdom.cities.Count; i++) {
				City city = this._kingdom.cities [i];
				int cityTotalWeight = GetDefendCityWeight (city);
				if (cityTotalWeight > 0) {
					cityWeights.Add (city, cityTotalWeight);
				}
			}
		}

		if (cityWeights.Count > 0) {
			defendCity = Utilities.PickRandomElementWithWeights<City> (cityWeights);
			weight = cityWeights [defendCity];
		}
	}
	private void GetAttackCityAndWeight(ref City attackCity, ref int weight){
		Dictionary<City, int> cityWeights = new Dictionary<City, int> ();
		List<City> enemyCities = new List<City> ();
		for (int i = 0; i < this._kingdom.cities.Count; i++) {
			City city = this._kingdom.cities [i];
			for (int j = 0; j < city.region.connections.Count; j++) {
				if(city.region.connections[j] is Region){
					Region adjacentRegion = (Region)city.region.connections [j];
					if(adjacentRegion.occupant != null && adjacentRegion.occupant.kingdom.id != this._kingdom.id && !cityWeights.ContainsKey(adjacentRegion.occupant)){
						KingdomRelationship kr = this._kingdom.GetRelationshipWithKingdom (adjacentRegion.occupant.kingdom);
						if(kr.sharedRelationship.isAtWar && !HasGeneralTaskToAttackCity(adjacentRegion.occupant)){
							int cityTotalWeight = GetAttackCityWeight (adjacentRegion.occupant);
							if(cityTotalWeight > 0){
								cityWeights.Add (adjacentRegion.occupant, cityTotalWeight);
							}
						}
					}
				}
			}
		}
		if(this._kingdom.alliancePool != null){
			for (int i = 0; i < this._kingdom.alliancePool.kingdomsInvolved.Count; i++) {
				Kingdom allyKingdom = this._kingdom.alliancePool.kingdomsInvolved [i];
				if(this._kingdom.id != allyKingdom.id){
					KingdomRelationship kr = this._kingdom.GetRelationshipWithKingdom (allyKingdom);
					if(kr.sharedRelationship.isAdjacent){
						for (int j = 0; j < allyKingdom.cities.Count; j++) {
							City city = allyKingdom.cities [j];
							for (int k = 0; k < city.region.connections.Count; k++) {
								if(city.region.connections[k] is Region){
									Region adjacentRegion = (Region)city.region.connections [k];
									if(adjacentRegion.occupant != null && adjacentRegion.occupant.kingdom.id != this._kingdom.id && adjacentRegion.occupant.kingdom.id != allyKingdom.id && !cityWeights.ContainsKey(adjacentRegion.occupant)){
										KingdomRelationship krToEnemy = this._kingdom.GetRelationshipWithKingdom (adjacentRegion.occupant.kingdom);
										if(krToEnemy.sharedRelationship.isAtWar && !HasGeneralTaskToAttackCity(adjacentRegion.occupant)){
											int cityTotalWeight = GetAttackCityWeight (adjacentRegion.occupant);
											if(cityTotalWeight > 0){
												cityWeights.Add (adjacentRegion.occupant, cityTotalWeight);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}


		if(cityWeights.Count <= 0){
			for (int i = 0; i < this._kingdom.cities.Count; i++) {
				City city = this._kingdom.cities [i];
				for (int j = 0; j < city.region.connections.Count; j++) {
					if(city.region.connections[j] is Region){
						Region adjacentRegion = (Region)city.region.connections [j];
						if(adjacentRegion.occupant != null && adjacentRegion.occupant.kingdom.id != this._kingdom.id && !cityWeights.ContainsKey(adjacentRegion.occupant)){
							KingdomRelationship kr = this._kingdom.GetRelationshipWithKingdom (adjacentRegion.occupant.kingdom);
							if(kr.sharedRelationship.isAtWar){
								int cityTotalWeight = GetAttackCityWeight (adjacentRegion.occupant);
								if(cityTotalWeight > 0){
									cityWeights.Add (adjacentRegion.occupant, cityTotalWeight);
								}
							}
						}
					}
				}
			}
			if(this._kingdom.alliancePool != null){
				for (int i = 0; i < this._kingdom.alliancePool.kingdomsInvolved.Count; i++) {
					Kingdom allyKingdom = this._kingdom.alliancePool.kingdomsInvolved [i];
					if(this._kingdom.id != allyKingdom.id){
						KingdomRelationship kr = this._kingdom.GetRelationshipWithKingdom (allyKingdom);
						if(kr.sharedRelationship.isAdjacent){
							for (int j = 0; j < allyKingdom.cities.Count; j++) {
								City city = allyKingdom.cities [j];
								for (int k = 0; k < city.region.connections.Count; k++) {
									if(city.region.connections[k] is Region){
										Region adjacentRegion = (Region)city.region.connections [k];
										if(adjacentRegion.occupant != null && adjacentRegion.occupant.kingdom.id != this._kingdom.id && adjacentRegion.occupant.kingdom.id != allyKingdom.id && !cityWeights.ContainsKey(adjacentRegion.occupant)){
											KingdomRelationship krToEnemy = this._kingdom.GetRelationshipWithKingdom (adjacentRegion.occupant.kingdom);
											if(krToEnemy.sharedRelationship.isAtWar){
												int cityTotalWeight = GetAttackCityWeight (adjacentRegion.occupant);
												if(cityTotalWeight > 0){
													cityWeights.Add (adjacentRegion.occupant, cityTotalWeight);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

		}

		if(cityWeights.Count > 0){
			attackCity = Utilities.PickRandomElementWithWeights<City> (cityWeights);
			weight = cityWeights [attackCity];
		}
	}
	private int GetDefendCityWeight(City city){
		int cityTotalWeight = 0;
		if(city.IsBorder()){
			bool isAdjacentToKingdomAtWar = false;
			bool hasGeneralAttackingCity = false;
			bool hasGeneralOnTile = false;
			bool hasIntlIncident = false;

			List<Kingdom> checkedKingdoms = new List<Kingdom> ();

			for (int j = 0; j < city.region.connections.Count; j++) {
				if(city.region.connections[j] is Region){
					Region adjacentRegion = (Region)city.region.connections [j];
					if(adjacentRegion.occupant != null && adjacentRegion.occupant.kingdom.id != this._kingdom.id){
						KingdomRelationship kr = this._kingdom.GetRelationshipWithKingdom (adjacentRegion.occupant.kingdom);
						if(kr.sharedRelationship.isAtWar){
							isAdjacentToKingdomAtWar = true;
//							if(adjacentRegion.occupant.kingdom.militaryManager.activeGenerals.Count > 0){
							if(adjacentRegion.occupant.kingdom.militaryManager.HasGeneralOnTile(adjacentRegion.occupant.hexTile) && !hasGeneralOnTile){
								hasGeneralOnTile = true;
								cityTotalWeight += 100;
							}
							if (this._kingdom.king.otherTraits.Contains(TRAIT.SMART) && adjacentRegion.occupant.kingdom.militaryManager.HasGeneralTaskToAttackCity (city) && !hasGeneralAttackingCity) {
								hasGeneralAttackingCity = true;
								cityTotalWeight += 500;
							}
						}
						if(!checkedKingdoms.Contains(adjacentRegion.occupant.kingdom)){
							checkedKingdoms.Add (adjacentRegion.occupant.kingdom);
							KingdomRelationship krSourceToTarget = this._kingdom.GetRelationshipWithKingdom (adjacentRegion.occupant.kingdom);
							KingdomRelationship krTargetToSource = adjacentRegion.occupant.kingdom.GetRelationshipWithKingdom (this._kingdom);
							if(!hasIntlIncident && krTargetToSource.sharedRelationship.internationalIncidents.Count > 0){
								hasIntlIncident = true;
								cityTotalWeight += 50;
							}
							if(krTargetToSource.totalLike < 0){
								cityTotalWeight = (2 * krTargetToSource.totalLike);
							}else{
								cityTotalWeight -= krTargetToSource.totalLike;
							}
							int threat = krSourceToTarget.targetKingdomThreatLevel;
							if(threat > 0){
								cityTotalWeight += (4 * threat);
							}
						}
					}
				}
			}
			if(!isAdjacentToKingdomAtWar){
				cityTotalWeight += 50;

			}else{
				if(this._kingdom.king.otherTraits.Contains(TRAIT.DEFENSIVE)){
					cityTotalWeight += 50;
				}
				cityTotalWeight += 100 * city.cityLevel;
			}
		}else{
			cityTotalWeight += 30;
		}
		if(cityTotalWeight < 0){
			cityTotalWeight = 0;
		}
		return cityTotalWeight;
	}

	private int GetAttackCityWeight(City city){
		int cityTotalWeight = 0;
		cityTotalWeight += 30 * city.cityLevel;
		if(this._kingdom.king.otherTraits.Contains(TRAIT.IMPERIALIST)){
			cityTotalWeight += 50;
		}
		if (city.region.tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.FOOD) {
			if (this._kingdom.cities.Count > this._kingdom.foodCityCapacity) {
				cityTotalWeight += 100;
			}
		} else if (city.region.tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.MATERIAL) {
			if (this._kingdom.race == RACE.HUMANS && city.region.tileWithSpecialResource.specialResource == RESOURCE.SLATE || city.region.tileWithSpecialResource.specialResource == RESOURCE.GRANITE){
				if(this._kingdom.cities.Count > this._kingdom.materialCityCapacityForHumans){
					cityTotalWeight += 100;
				}
			}else if (this._kingdom.race == RACE.ELVES && city.region.tileWithSpecialResource.specialResource == RESOURCE.OAK || city.region.tileWithSpecialResource.specialResource == RESOURCE.EBONY){
				if(this._kingdom.cities.Count > this._kingdom.materialCityCapacityForElves){
					cityTotalWeight += 100;
				}
			}
		} else if (city.region.tileWithSpecialResource.specialResourceType == RESOURCE_TYPE.ORE) {
			if (this._kingdom.cities.Count > this._kingdom.oreCityCapacity) {
				cityTotalWeight += 100;
			}
		}

		KingdomRelationship krSourceToTarget = this._kingdom.GetRelationshipWithKingdom (city.kingdom);
		KingdomRelationship krTargetToSource = city.kingdom.GetRelationshipWithKingdom (this._kingdom);

		if(krSourceToTarget.totalLike < 0){
			cityTotalWeight += ((2 * krSourceToTarget.totalLike) * -1);
		}else{
			cityTotalWeight -= krSourceToTarget.totalLike;
		}
		cityTotalWeight += (krTargetToSource.relativeStrength * 4);
		cityTotalWeight -= (krSourceToTarget.relativeStrength * 4);
		if(cityTotalWeight < 0){
			cityTotalWeight = 0;
		}
		return cityTotalWeight;
	}

	internal bool HasGeneralOnTile(HexTile hexTile){
		for (int i = 0; i < this.activeGenerals.Count; i++) {
			General general = this.activeGenerals [i];
			if(general.location.id == hexTile.id){
				return true;
			}
		}
		return false;
	}

	internal bool HasGeneralTaskToAttackCity(City city){
		for (int i = 0; i < this.activeGenerals.Count; i++) {
			General general = this.activeGenerals [i];
			if(general.generalTask != null && general.generalTask.task == GENERAL_TASKS.ATTACK_CITY){
				if(((AttackCityTask)general.generalTask).targetCity.id == city.id){
					return true;
				}
			}
		}
		return false;
	}

	internal void DestroyAllGenerals(){
		while(this.activeGenerals.Count > 0){
			this.activeGenerals [0].Death (DEATH_REASONS.ACCIDENT);
		}
	}
}
