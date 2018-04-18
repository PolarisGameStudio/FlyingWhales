﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventManager : MonoBehaviour {
	public static EventManager Instance;

    public EVENT_TYPES[] playerPlacableEvents;

    [SerializeField] private Sprite[] eventAvatarSprites;

    private Dictionary <string, UnityEvent> eventDictionary;

	public Dictionary<EVENT_TYPES, List<GameEvent>> allEvents;

	/*
	 * Generic Events
	 * */
	public WeekEndedEvent onWeekEnd = new WeekEndedEvent();

	/*
	 * Kingdom Events
	 * */
	public NewKingdomEvent onCreateNewKingdomEvent = new NewKingdomEvent();
	public KingdomDiedEvent onKingdomDiedEvent = new KingdomDiedEvent();


	/*
	 * Citizen Events
	 * */
	public CitizenTurnActions onCitizenTurnActions = new CitizenTurnActions ();
	public CitizenDiedEvent onCitizenDiedEvent =  new CitizenDiedEvent();

	/*
	 * City Events
	 * */
	public CityEverydayTurnActions onCityEverydayTurnActions = new CityEverydayTurnActions();
	public CitizenMove onCitizenMove =  new CitizenMove();

	/*
	 * Game Events
	 * */
	public GameEventAction onGameEventAction = new GameEventAction();
	public GameEventEnded onGameEventEnded = new GameEventEnded();

	/*
	 * UI Events
	 * */
	public UpdateUI onUpdateUI = new UpdateUI();

	public Dictionary<EVENT_TYPES, int> eventDuration = new Dictionary<EVENT_TYPES, int>(){
		{EVENT_TYPES.BORDER_CONFLICT, 30},
		{EVENT_TYPES.DIPLOMATIC_CRISIS, 30},
		{EVENT_TYPES.INVASION_PLAN, 60},
		{EVENT_TYPES.JOIN_WAR_REQUEST, -1},
		{EVENT_TYPES.STATE_VISIT, 10},
		{EVENT_TYPES.ASSASSINATION, -1},
		{EVENT_TYPES.RAID, 5},
		{EVENT_TYPES.EXPANSION, -1},
        {EVENT_TYPES.TRADE, -1},
		{EVENT_TYPES.ATTACK_CITY, -1},
		{EVENT_TYPES.SABOTAGE, -1},
		{EVENT_TYPES.REINFORCE_CITY, -1},
		{EVENT_TYPES.SECESSION, 60},
		{EVENT_TYPES.RIOT_WEAPONS, 30},
		{EVENT_TYPES.REBELLION, -1},
        {EVENT_TYPES.REQUEST_PEACE, -1},
        {EVENT_TYPES.PLAGUE, -1},
		{EVENT_TYPES.SCOURGE_CITY, -1},
		{EVENT_TYPES.BOON_OF_POWER, -1},
		{EVENT_TYPES.PROVOCATION, -1},
		{EVENT_TYPES.EVANGELISM, -1},
		{EVENT_TYPES.SPOUSE_ABDUCTION, -1},
        {EVENT_TYPES.LYCANTHROPY, -1},
		{EVENT_TYPES.FIRST_AND_KEYSTONE, -1},
		{EVENT_TYPES.RUMOR, -1},
		{EVENT_TYPES.SLAVES_MERCHANT, -1},
		{EVENT_TYPES.HIDDEN_HISTORY_BOOK, -1},
        {EVENT_TYPES.HYPNOTISM, -1},
        {EVENT_TYPES.KINGDOM_HOLIDAY, -1},
		{EVENT_TYPES.SERUM_OF_ALACRITY, 30},
        {EVENT_TYPES.DEVELOP_WEAPONS, -1},
        {EVENT_TYPES.KINGS_COUNCIL, -1},
		{EVENT_TYPES.ALTAR_OF_BLESSING, -1},
        {EVENT_TYPES.ADVENTURE, -1},
        {EVENT_TYPES.EVIL_INTENT, -1},
        {EVENT_TYPES.ATTACK_LAIR, -1},
		{EVENT_TYPES.GREAT_STORM, -1},
		{EVENT_TYPES.SEND_RELIEF_GOODS, -1},
		{EVENT_TYPES.HUNT_LAIR, -1},
		{EVENT_TYPES.ANCIENT_RUIN, -1},
        {EVENT_TYPES.MILITARY_ALLIANCE_OFFER, -1},
    };

	public int expansionEventCarriedPopulation;

	void Awake(){
		Instance = this;
		this.Init();
	}

	void Init (){
		if (eventDictionary == null){
			eventDictionary = new Dictionary<string, UnityEvent>();
		}
		if (allEvents == null) {
			allEvents = new Dictionary<EVENT_TYPES, List<GameEvent>>();
		}
	}

	/*
	 * Register an event to the allEvents Dictionary.
	 * */
	public void AddEventToDictionary(GameEvent gameEvent){
		if (allEvents.ContainsKey (gameEvent.eventType)) {
			allEvents [gameEvent.eventType].Add(gameEvent);
		} else {
			allEvents.Add (gameEvent.eventType, new List<GameEvent> (){ gameEvent });
		}
	}

	/*
	 * Get a list of all the events of a specific type (including done events).
	 * */
	public List<GameEvent> GetEventsOfType(EVENT_TYPES eventType){
		List<GameEvent> eventsOfType = new List<GameEvent>();
		if (this.allEvents.ContainsKey (eventType)) {
			eventsOfType = this.allEvents[eventType];
		}
		return eventsOfType;
	}

	/*
	 * Get a list of all the events started by a kingdom, 
	 * can pass event types to only get events of that type.
	 * */
	public List<GameEvent> GetEventsStartedByKingdom(Kingdom kingdom, EVENT_TYPES[] eventTypes, bool isActiveOnly = true){
		List<GameEvent> gameEventsOfTypePerKingdom = new List<GameEvent>();
		if (eventTypes.Contains (EVENT_TYPES.ALL)) {
			for (int i = 0; i < allEvents.Keys.Count; i++) {
				EVENT_TYPES currKey = allEvents.Keys.ElementAt(i);
				List<GameEvent> eventsOfType = allEvents[currKey];
				for (int j = 0; j < eventsOfType.Count; j++) {
					GameEvent currEvent = eventsOfType [j];
					if (currEvent.startedByKingdom != null && currEvent.startedByKingdom.id == kingdom.id) {
                        if (isActiveOnly) {
                            if (currEvent.isActive) {
                                gameEventsOfTypePerKingdom.Add(currEvent);
                            }
                        } else {
                            gameEventsOfTypePerKingdom.Add(currEvent);
                        }
					}
				}
			}
		} else {
			for (int i = 0; i < eventTypes.Length; i++) {
				EVENT_TYPES currentEvent = eventTypes [i];
				if (this.allEvents.ContainsKey (currentEvent)) {
					List<GameEvent> eventsOfType = this.allEvents [currentEvent];
					for (int j = 0; j < eventsOfType.Count; j++) {
                        GameEvent currEvent = eventsOfType[j];
                        if (currEvent.startedByKingdom != null && currEvent.startedByKingdom.id == kingdom.id) {
                            if (isActiveOnly) {
                                if (currEvent.isActive) {
                                    gameEventsOfTypePerKingdom.Add(currEvent);
                                }
                            } else {
                                gameEventsOfTypePerKingdom.Add(currEvent);
                            }
                        }
					}
				}
			}
		}
		return gameEventsOfTypePerKingdom;
	}


	/*
	 * Get a list of events started by a city
	 * */
	public List<GameEvent> GetAllEventsPerCity(City city){
		List<GameEvent> gameEventsOfCity = new List<GameEvent>();
		for (int i = 0; i < this.allEvents.Keys.Count; i++) {
			EVENT_TYPES currentKey = this.allEvents.Keys.ElementAt(i);
			List<GameEvent> gameEventsOfType = this.allEvents [currentKey];
			for (int j = 0; j < gameEventsOfType.Count; j++) {
				if (gameEventsOfType [j].startedByCity != null) {
					if (gameEventsOfType [j].startedByCity.id == city.id) {
						gameEventsOfCity.Add (gameEventsOfType [j]);
					}
				}
			}
		}
		return gameEventsOfCity;
	}

	/*
	 * Get a list of events that a citizen started
	 * */
	public List<GameEvent> GetAllEventsStartedByCitizen(Citizen citizen){
		List<GameEvent> gameEventsOfCitizen = new List<GameEvent>();
		for (int i = 0; i < this.allEvents.Keys.Count; i++) {
			EVENT_TYPES currentKey = this.allEvents.Keys.ElementAt (i);
			List<GameEvent> gameEventsOfType = this.allEvents [currentKey];
			for (int j = 0; j < gameEventsOfType.Count; j++) {
				if (gameEventsOfType[j].startedBy.id == citizen.id) {
					gameEventsOfCitizen.Add(gameEventsOfType [j]);
				}
			}
		}
		return gameEventsOfCitizen;
	}

	/*
	 * Get a list of events of type that a citizen started
	 * */
	public List<GameEvent> GetAllEventsStartedByCitizenByType(Citizen citizen, EVENT_TYPES eventType){
		List<GameEvent> gameEventsOfCitizen = new List<GameEvent>();
		if (this.allEvents.ContainsKey (eventType)) {
			List<GameEvent> gameEventsOfType = this.allEvents [eventType];
			for (int i = 0; i < gameEventsOfType.Count; i++) {
				if (gameEventsOfType[i].startedBy.id == citizen.id) {
					gameEventsOfCitizen.Add(gameEventsOfType[i]);
				}
			}
		}
		return gameEventsOfCitizen;
	}

	/*internal Citizen GetSpy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		List<Citizen> spies = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.SPY) {
							if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
								spies.Add (kingdom.cities [i].citizens [j]);
							}
						}
					}
				}
			}
		}

		if(spies.Count > 0){
			int random = UnityEngine.Random.Range (0, spies.Count);
			((Spy)spies [random].assignedRole).inAction = true;
			return spies [random];
		}else{
//			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}*/

	internal bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}

    internal Sprite GetEventAvatarSprite(EVENT_TYPES eventType) {
        return eventAvatarSprites.Where(x => x.name == eventType.ToString()).FirstOrDefault();
    }
//	public static void StartListening (string eventName, UnityAction listener){
//		UnityEvent thisEvent = null;
//		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent)){
//			thisEvent.AddListener (listener);
//		} else {
//			thisEvent = new UnityEvent ();
//			thisEvent.AddListener (listener);
//			Instance.eventDictionary.Add (eventName, thisEvent);
//		}
//	}
//
//	public static void StopListening (string eventName, UnityAction listener){
//		if (Instance == null) return;
//		UnityEvent thisEvent = null;
//		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent)){
//			thisEvent.RemoveListener (listener);
//		}
//	}
//
//	public static void TriggerEvent (string eventName){
//		UnityEvent thisEvent = null;
//		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent)){
//			thisEvent.Invoke ();
//		}
//	}
}