﻿using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;

public class LocationEventManager {
    private readonly NPCSettlement _location;

    private readonly List<SettlementEvent> _activeEvents;

    #region getters
    public List<SettlementEvent> activeEvents => _activeEvents;
    #endregion
    
    public LocationEventManager(NPCSettlement location) {
        _location = location;
        _activeEvents = new List<SettlementEvent>();
    }
    public LocationEventManager(NPCSettlement location, SaveDataLocationEventManager saveData) {
        _location = location;
        _activeEvents = new List<SettlementEvent>();
        for (int i = 0; i < saveData.settlementEvents.Count; i++) {
            SaveDataSettlementEvent saveDataSettlementEvent = saveData.settlementEvents[i];
            SettlementEvent settlementEvent = saveDataSettlementEvent.Load();
            settlementEvent.LoadAdditionalData(location);
            _activeEvents.Add(settlementEvent);
        }
    }


    #region Events
    public void AddNewActiveEvent(SETTLEMENT_EVENT settlementEvent) {
        SettlementEvent newEvent = CreateNewSettlementEvent(settlementEvent);
        activeEvents.Add(newEvent);
        newEvent.ActivateEvent(_location);
    }
    public void DeactivateEvent(SettlementEvent settlementEvent) {
        if (activeEvents.Remove(settlementEvent)) {
            settlementEvent.DeactivateEvent(_location);
        }
    }
    public bool HasActiveEvent(SETTLEMENT_EVENT p_settlementEventType) {
        for (int i = 0; i < activeEvents.Count; i++) {
            SettlementEvent e = activeEvents[i];
            if (e.eventType == p_settlementEventType) {
                return true;
            }
        }
        return false;
    }
    public bool HasActiveEvent<T>(out T p_settlementEvent) where T : SettlementEvent{
        for (int i = 0; i < activeEvents.Count; i++) {
            SettlementEvent e = activeEvents[i];
            if (e is T settlementEvent) {
                p_settlementEvent = settlementEvent;
                return true;
            }
        }
        p_settlementEvent = null;
        return false;
    }
    public bool HasActiveEvent(SettlementEvent p_settlementEvent) {
        return activeEvents.Contains(p_settlementEvent);
    }
    public T GetActiveEvent<T>() where T : SettlementEvent {
        for (int i = 0; i < activeEvents.Count; i++) {
            SettlementEvent e = activeEvents[i];
            if (e is T settlementEvent) {
                return settlementEvent;
            }
        }
        return null;
    }
    private SettlementEvent CreateNewSettlementEvent(SETTLEMENT_EVENT settlementEvent) {
        var typeName = $"Locations.Settlements.Settlement_Events.{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(settlementEvent.ToString())}";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SettlementEvent obj = System.Activator.CreateInstance(type, _location) as SettlementEvent;
            return obj;
        }
        throw new System.Exception($"Could not create new instance of settlement event of type {typeName}");
    }
    private T CreateNewSettlementEvent<T>() where T : SettlementEvent {
        System.Type type = typeof(T);
        if (System.Activator.CreateInstance(type, _location) is T obj) {
            return obj;    
        }
        throw new System.Exception($"Could not create new instance of settlement event of type {type}");
    }
    public void OnResidentAdded(Character character) {
        for (int i = 0; i < activeEvents.Count; i++) {
            SettlementEvent settlementEvent = activeEvents[i];
            settlementEvent.ProcessNewVillager(character);
        }
    }
    public void OnResidentRemoved(Character character) {
        for (int i = 0; i < activeEvents.Count; i++) {
            SettlementEvent settlementEvent = activeEvents[i];
            settlementEvent.ProcessRemovedVillager(character);
        }
    }
    public bool CanHaveEvents() {
        if (_location.owner == null) {
            return false;
        }
        if (_location.owner.isPlayerFaction) {
            return false;
        }
        if (!_location.owner.isMajorFaction) {
            return false;
        }
        return true;
    }
    #endregion

    #region Utilites
    public void OnSettlementDestroyed() {
        List<SettlementEvent> events = new List<SettlementEvent>(_activeEvents);
        for (int i = 0; i < events.Count; i++) {
            DeactivateEvent(events[i]);
        }
    }
    #endregion
}

public class SaveDataLocationEventManager : SaveData<LocationEventManager> {

    public List<SaveDataSettlementEvent> settlementEvents;
    
    public override void Save(LocationEventManager data) {
        base.Save(data);
        settlementEvents = new List<SaveDataSettlementEvent>();
        for (int i = 0; i < data.activeEvents.Count; i++) {
            SettlementEvent settlementEvent = data.activeEvents[i];
            SaveDataSettlementEvent saveData = settlementEvent.Save();
            settlementEvents.Add(saveData);
        }
    }
}
