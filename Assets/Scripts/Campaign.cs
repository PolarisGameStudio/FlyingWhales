﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Campaign {
	public int id;
	public Citizen leader;
	public City targetCity;
	public List<General> registeredGenerals;
	public List<CampaignCandidates> candidates;
	public CAMPAIGN campaignType;
	public WAR_TYPE warType;
	public HexTile rallyPoint;
	public bool isFull;
	public bool hasStarted;
	public int neededArmyStrength;
	public int expiration;
	public bool isGhost;

	public Campaign(Citizen leader, City targetCity, CAMPAIGN campaignType, WAR_TYPE warType, int neededArmyStrength = 0, int expiration = 8){
		this.id = Utilities.SetID (this);
		this.leader = leader;
		this.targetCity = targetCity;
		this.campaignType = campaignType;
		this.warType = warType;
		this.registeredGenerals = new List<General> ();
		this.candidates = new List<CampaignCandidates> ();
		this.isFull = false;
		this.hasStarted = false;
		this.rallyPoint = null;
		this.neededArmyStrength = neededArmyStrength;
		this.expiration = expiration;
		this.isGhost = false;
		EventManager.Instance.onWeekEnd.AddListener (this.CheckExpiration);
	}

	internal int GetArmyStrength(){
		int total = 0;
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			total += this.registeredGenerals[i].GetArmyHP();
		}
		return total;
	}
	internal void GoToRallyPoint(){
		
	}

	internal void CheckExpiration(){
		if(this.campaignType == CAMPAIGN.DEFENSE){
			if(this.expiration >= 0){
				AdjustExpiration (-1);
			}else{
				if (this.registeredGenerals.Count <= 0) {
					AdjustExpiration (-1);
				} else if (this.targetCity != null) {
					if (this.targetCity.isDead) {
						AdjustExpiration (-1);
					}
				} else if (this.targetCity == null) {
					AdjustExpiration (-1);
				}	
			}
		}else{
			if (this.registeredGenerals.Count <= 0) {
				AdjustExpiration (-1);
			} else if (this.targetCity != null) {
				if (this.targetCity.isDead) {
					AdjustExpiration (-1);
				}
			} else if (this.targetCity == null) {
				AdjustExpiration (-1);
			}	
		}

	}

	private void AdjustExpiration(int amount){
		this.expiration += amount;
		if(this.expiration <= 0){
			if(!this.isGhost){
				Debug.Log (this.leader.name + " " + this.campaignType.ToString () + " campaign for " + this.targetCity.name + " has expired!");
			}else{
				Debug.Log (this.leader.name + " " + this.campaignType.ToString () + " campaign has expired!");
			}
			this.expiration = 0;
			this.leader.campaignManager.CampaignDone (this);
		}
	}

	internal bool AreAllGeneralsOnRallyPoint(){
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			if(this.registeredGenerals[i].location != this.rallyPoint){
				return false;
			}
		}
		return true;
	}
	internal bool AreAllGeneralsOnDefenseCity(){
		for(int i = 0; i < this.registeredGenerals.Count; i++){
			if(this.registeredGenerals[i].location != this.targetCity.hexTile){
				return false;
			}
		}
		return true;
	}
	internal void AttackCityNow(){
		List<HexTile> path = PathGenerator.Instance.GetPath (this.rallyPoint, this.targetCity.hexTile, PATHFINDING_MODE.COMBAT);

		if (path != null) {
			for(int i = 0; i < this.registeredGenerals.Count; i++){
				this.registeredGenerals[i].targetLocation = this.targetCity.hexTile;
				this.registeredGenerals [i].roads.Clear ();
				this.registeredGenerals [i].roads = new List<HexTile>(path);
				this.registeredGenerals [i].daysBeforeArrival = path.Sum(x => x.movementDays);
				this.registeredGenerals [i].generalAvatar.transform.parent = this.registeredGenerals [i].location.transform;
				this.registeredGenerals [i].generalAvatar.transform.localPosition = Vector3.zero;
				this.registeredGenerals [i].generalAvatar.GetComponent<GeneralObject> ().path.Clear ();
				this.registeredGenerals [i].generalAvatar.GetComponent<GeneralObject> ().path = new List<HexTile>(path);
				this.targetCity.incomingGenerals.Add (this.registeredGenerals[i]);
//				if(this.registeredGenerals[i].generalAvatar == null){
//					this.registeredGenerals [i].generalAvatar = GameObject.Instantiate (Resources.Load ("GameObjects/GeneralAvatar"), this.registeredGenerals [i].location.transform) as GameObject;
//					this.registeredGenerals [i].generalAvatar.transform.localPosition = Vector3.zero;
//					this.registeredGenerals [i].generalAvatar.GetComponent<GeneralObject>().general = this.registeredGenerals [i];
//					this.registeredGenerals [i].generalAvatar.GetComponent<GeneralObject> ().Init();
//				}else{
//					
//				}
			}
		}

		//remove from rally point
//		if(this.rallyPoint != null){
//			if(!this.rallyPoint.isOccupied){
//				for(int i = 0; i < this.registeredGenerals.Count; i++){
//					this.rallyPoint.city.incomingGenerals.Remove(this.registeredGenerals[i]);
//				}
//			}
//		}
	}
	internal void AddCandidate(General general, List<HexTile> path){
		int armyHp = general.GetArmyHP ();
		this.candidates.Add (new CampaignCandidates (general, path, armyHp));
	}
	internal void RegisterGenerals(){
		if(this.candidates.Count > 0){
			this.candidates.OrderBy (x => x.path.Count).ToList ();
			if(this.campaignType == CAMPAIGN.OFFENSE){
				for(int i = 0; i < this.candidates.Count; i++){
					if(this.GetArmyStrength() < this.neededArmyStrength){
						this.candidates [i].general.AssignCampaign (this, this.candidates [i].path);
					}else{
						break;
					}
				}
			}else{
				if(this.expiration >= 0){
					for(int i = 0; i < this.candidates.Count; i++){
						if(this.candidates[i].path.Count <= (this.expiration - 1)){
							if(this.GetArmyStrength() < this.neededArmyStrength){
								this.candidates [i].general.AssignCampaign (this, this.candidates [i].path);
							}else{
								break;
							}
						}else{
							if(this.candidates[i].general.location == this.targetCity.hexTile){
								if (this.GetArmyStrength () < this.neededArmyStrength) {
									this.candidates [i].general.AssignCampaign (this, this.candidates [i].path);
								}
							}
						}
					}
				}else{
					for(int i = 0; i < this.candidates.Count; i++){
						if(this.GetArmyStrength() < this.neededArmyStrength){
							this.candidates [i].general.AssignCampaign (this, this.candidates [i].path);
						}else{
							break;
						}
					}
				}
			}
		}
	}
}
