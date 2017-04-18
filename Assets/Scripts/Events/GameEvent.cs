﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameEvent {

	public int eventID;
	public EVENT_TYPES eventType;
	public EVENT_STATUS eventStatus;

	public int startWeek;
	public int startMonth;
	public int startYear;

	public int durationInWeeks;
	public int remainingWeeks;

	public int endWeek;
	public int endMonth;
	public int endYear;

	public string description;
	public string resolution;
	public Citizen startedBy;
	public Kingdom startedByKingdom;
	public City startedByCity;
	public bool isActive;

	public GameEvent(int startWeek, int startMonth, int startYear, Citizen startedBy){
		this.eventID = Utilities.SetID(this);
		this.eventStatus = EVENT_STATUS.EXPOSED;
		this.startWeek = startWeek;
		this.startMonth = startMonth;
		this.startYear = startYear;
		this.startedBy = startedBy;
		this.durationInWeeks = 0;
		this.remainingWeeks = 0;
		this.endWeek = 0;
		this.endMonth = 0;
		this.endYear = 0;
		this.description = "";
		this.resolution = "";
		this.isActive = true;
		if(this.startedBy != null){
			this.startedByKingdom = startedBy.city.kingdom;
			this.startedByCity = startedBy.city;
		}
		Debug.LogError("New Event was created!");
	}

	internal virtual void PerformAction(){}

	internal virtual void DoneCitizenAction(Citizen citizen){}

	internal virtual void DoneEvent(){
		Debug.LogError ("Game Event Ended!");
		EventManager.Instance.onGameEventEnded.Invoke(this);
	}

	internal virtual void CancelEvent(){}

	internal bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}
	internal List<Citizen> GetUnwantedGovernors(Citizen king){
		List<Citizen> unwantedGovernors = new List<Citizen> ();
		for(int i = 0; i < king.civilWars.Count; i++){
			if(king.civilWars[i].isGovernor){
				unwantedGovernors.Add (king.civilWars [i]);
			}
		}
		for(int i = 0; i < king.successionWars.Count; i++){
			if(king.successionWars[i].isGovernor){
				unwantedGovernors.Add (king.successionWars [i]);
			}
		}
		for(int i = 0; i < king.city.kingdom.cities.Count; i++){
			if(king.city.kingdom.cities[i].governor.supportedCitizen != null){
				unwantedGovernors.Add (king.city.kingdom.cities [i].governor);
			}
		}

		return unwantedGovernors;
	}
}
