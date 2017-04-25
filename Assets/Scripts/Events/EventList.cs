﻿using UnityEngine.Events;

//Generic Events
public class WeekEndedEvent : UnityEvent{}

//Kingdom Events
public class NewKingdomEvent : UnityEvent<Kingdom>{}

//Citizen Events
public class CitizenTurnActions: UnityEvent{}
public class CitizenDiedEvent : UnityEvent{}
public class CheckCitizensSupportingMe : UnityEvent<Citizen>{}

//City Events
public class CityEverydayTurnActions: UnityEvent{}
public class CitizenMove: UnityEvent<bool>{}

//Campaign
public class RegisterOnCampaign: UnityEvent<Campaign>{}
public class DeathArmy: UnityEvent{}
public class UnsupportCitizen: UnityEvent<Citizen>{}
public class RemoveSuccessionWarCity: UnityEvent<City>{}


//Game Events
public class GameEventAction: UnityEvent<GameEvent, int>{}
public class RecruitCitizensForExpansion: UnityEvent<Expansion, Kingdom>{}
public class GameEventEnded: UnityEvent<GameEvent>{}

//UI
public class UpdateUI: UnityEvent{}
public class ShowEventsOfType: UnityEvent<EVENT_TYPES>{}
public class HideEvents: UnityEvent{}