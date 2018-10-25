﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using ECS;

public class StructureObj : IObject {
    protected OBJECT_TYPE _objectType;
    protected LANDMARK_TYPE _specificObjectType;
    protected bool _isInvisible;
    protected int _maxHP;
    protected ActionEvent _onHPReachedZero;
    protected ActionEvent _onHPReachedFull;

    protected string _objectName;
    protected int _currentHP;
    protected bool _isDirty;
    protected RESOURCE _madeOf;
    protected BaseLandmark _objectLocation;
    [NonSerialized] protected ObjectState _currentState;
    protected List<ObjectState> _states;
    protected List<CharacterAttribute> _attributes;
    protected Dictionary<RESOURCE, int> _resourceInventory;

    #region getters/setters
    public string objectName {
        get { return _objectName; }
    }
    public OBJECT_TYPE objectType {
        get { return _objectType; }
    }
    public LANDMARK_TYPE specificObjectType {
        get { return _specificObjectType; }
    }
    public List<ObjectState> states {
        get { return _states; }
    }
    public List<CharacterAttribute> attributes {
        get { return _attributes; }
    }
    public ObjectState currentState {
        get { return _currentState; }
    }
    public bool isInvisible {
        get { return _isInvisible; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public BaseLandmark objectLocation {
        get { return _objectLocation; }
    }
    public ILocation specificLocation {
        get { return objectLocation; }
    }
    public Dictionary<RESOURCE, int> resourceInventory {
        get { return _resourceInventory; }
    }
    public bool isHPFull {
        get { return _currentHP >= _maxHP; }
    }
    public bool isHPZero {
        get { return _currentHP == 0; }
    }
    public RESOURCE madeOf {
        get { return _madeOf; }
    }
    public bool isDirty {
        get { return _isDirty; }
    }
    public bool isRuined {
        get { return currentState.stateName.Equals("Ruined"); }
    }
    #endregion

    public StructureObj() {
        _objectType = OBJECT_TYPE.STRUCTURE;
        _attributes = new List<CharacterAttribute>();
        SetIsDirty(true);
        ConstructResourceInventory();
    }

    #region Virtuals
    public virtual IObject Clone() {
        StructureObj clone = new StructureObj();
        SetCommonData(clone);
        return clone;
    }
    public virtual IObject NewCopyObject(IObject iobject) {
        StructureObj clone = new StructureObj();
        SetCommonData(clone);
        return clone;
    }
    public virtual void AdjustResource(RESOURCE resource, int amount) {
        _resourceInventory[resource] += amount;
        if (_resourceInventory[resource] < 0) {
            _resourceInventory[resource] = 0;
        }
    }
    public virtual void OnAddToLandmark(BaseLandmark newLocation) {
        SetObjectLocation(newLocation);
        GenerateInitialDefenders();
    }
    public virtual void StartState(ObjectState state) {
        if(state.stateName == "Ruined") {
            Messenger.Broadcast(Signals.DESTROY_LANDMARK, this.objectLocation);
            objectLocation.tileLocation.SetLandmarkTileSprite(new LandmarkStructureSprite(LandmarkManager.Instance.ruinedSprite, null));
            objectLocation.MigrateCharactersToAnotherLandmark();
        }
    }
    public virtual void EndState(ObjectState state) {

    }
    public virtual void StartDayAction() {
        GenerateDailyInteraction();
    }
    public virtual void GenerateInitialDefenders() {
        if (_objectLocation.owner == null) {
            return;
        }
        Debug.Log("Generating initial defenders for " + _specificObjectType.ToString());
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_specificObjectType);
        for (int i = 0; i < data.initialDefenderCount; i++) {
            WeightedDictionary<LandmarkDefender> defenderWeights;
            if (i == 0) {
                defenderWeights = data.firstElementDefenderWeights;
            } else {
                defenderWeights = data.defenderWeights;
            }
            if (defenderWeights.GetTotalOfWeights() > 0) {
                LandmarkDefender chosenDefender = defenderWeights.PickRandomElementGivenWeights();
                CharacterArmyUnit defenderUnit = CharacterManager.Instance.CreateCharacterArmyUnit(_objectLocation.owner.race, chosenDefender, _objectLocation.owner, _objectLocation); //_objectLocation.owner.race
                _objectLocation.AddDefender(defenderUnit);
            }
        }
    }
    public virtual void GenerateDailyInteraction() {
        if (_objectLocation.HasActiveInteraction()) {
            return; //the landmark already has an active interaction, other than investigate
        }
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_objectLocation.specificLandmarkType);
        if (data.eventTriggerWeights.GetTotalOfWeights() > 0) {
            if (data.eventTriggerWeights.PickRandomElementGivenWeights()) { //if event trigger weights return true
                if (data.interactionWeights.GetTotalOfWeights() > 0) {
                    INTERACTION_TYPE chosenInteraction = data.interactionWeights.PickRandomElementGivenWeights();
                    //create interaction of type;
                    Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(chosenInteraction, _objectLocation);
                    if (createdInteraction != null) {
                        _objectLocation.AddInteraction(createdInteraction);
                    }
                }
            }
        }
    }
    #endregion

    #region Interface Requirements
    public void SetStates(List<ObjectState> states, bool autoChangeState = true) {
        _states = states;
        if (autoChangeState) {
            ChangeState(states[0]);
        }
    }
    public void SetObjectName(string name) {
        _objectName = name;
    }
    public void SetSpecificObjectType(LANDMARK_TYPE specificObjectType) {
        _specificObjectType = specificObjectType;
    }
    public void SetIsInvisible(bool state) {
        _isInvisible = state;
    }
    public void SetOnHPFullFunction(ActionEvent action) {
        _onHPReachedFull = action;
    }
    public void SetOnHPZeroFunction(ActionEvent action) {
        _onHPReachedZero = action;
    }
    public void SetMaxHP(int amount) {
        _maxHP = amount;
    }
    public void SetObjectLocation(BaseLandmark newLocation) {
        //if (this.icharacterType == ICHARACTER_TYPE.MONSTERDen) {
        //    Debug.Log("Set object location to " + newLocation.ToString() + ". ST: " + StackTraceUtility.ExtractStackTrace());
        //}
        _objectLocation = newLocation;
    }
    public void ChangeState(ObjectState state) {
        if (_currentState != null) {
            _currentState.OnEndState();
        }
        _currentState = state;
        _currentState.OnStartState();
        Messenger.Broadcast(Signals.STRUCTURE_STATE_CHANGED, this, state);
    }
    public ObjectState GetState(string name) {
        for (int i = 0; i < _states.Count; i++) {
            if (_states[i].stateName == name) {
                return _states[i];
            }
        }
        return null;
    }
    public void AdjustHP(int amount) {
        //When hp reaches 0 or 100 a function will be called
        int previousHP = _currentHP;
        _currentHP += amount;
        _currentHP = Mathf.Clamp(_currentHP, 0, maxHP);
        if (this.objectLocation.landmarkVisual != null) {
            this.objectLocation.landmarkVisual.UpdateProgressBar();
        }
        if (previousHP != _currentHP) {
            if (_currentHP == 0 && _onHPReachedZero != null) {
                _onHPReachedZero.Invoke(this);
            } else if (_currentHP == maxHP && _onHPReachedFull != null) {
                _onHPReachedFull.Invoke(this);
            }
        }
    }
    public void SetHP(int amount) {
        _currentHP = amount;
    }
    #endregion

    #region Resource Inventory
    private void ConstructResourceInventory() {
        _resourceInventory = new Dictionary<RESOURCE, int>();
        RESOURCE[] allResources = Utilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < allResources.Length; i++) {
            if (allResources[i] != RESOURCE.NONE) {
                _resourceInventory.Add(allResources[i], 0);
            }
        }
    }
    public void TransferResourceTo(RESOURCE resource, int amount, StructureObj target) {
        AdjustResource(resource, -amount);
        target.AdjustResource(resource, amount);
    }
    public void TransferResourceTo(RESOURCE resource, int amount, CharacterObj target) {
        AdjustResource(resource, -amount);
        target.AdjustResource(resource, amount);
    }
    public void TransferResourceTo(RESOURCE resource, int amount, LandmarkObj target) {
        AdjustResource(resource, -amount);
        target.AdjustResource(resource, amount);
    }
    //public int GetTotalCivilians() {
    //    int total = 0;
    //    foreach(RESOURCE resource in _resourceInventory.Keys) {
    //        if (resource.ToString().Contains("CIVILIAN")) {
    //            total += _resourceInventory[resource];
    //        }
    //    }
    //    //foreach (KeyValuePair<RESOURCE, int> kvp in _resourceInventory) {
    //    //    if (kvp.Key.ToString().Contains("CIVILIAN")) {
    //    //        total += kvp.Value;
    //    //    }
    //    //}
    //    return total;
    //}
    //public void CiviliansDeath(RACE race, int amount) {
    //    RESOURCE civilianResource = RESOURCE.ELF_CIVILIAN;
    //    switch (race) {
    //        case RACE.HUMANS:
    //        civilianResource = RESOURCE.HUMAN_CIVILIAN;
    //        break;
    //    }
    //    AdjustResource(civilianResource, -amount);
    //    Messenger.Broadcast<StructureObj, int>("CiviliansDeath", this, amount);
    //}
    public virtual RESOURCE GetMainResource() {
        return RESOURCE.NONE;
    }
    #endregion

    #region Utilities
    public void SetCommonData(StructureObj clone) {
        clone.SetObjectName(this._objectName);
        clone._specificObjectType = this._specificObjectType;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._maxHP = this.maxHP;
        clone._currentHP = this.maxHP;
        clone._onHPReachedZero = this._onHPReachedZero;
        clone._onHPReachedFull = this._onHPReachedFull;
        clone._madeOf = this._madeOf;
        List<ObjectState> states = new List<ObjectState>();
        for (int i = 0; i < this.states.Count; i++) {
            ObjectState currState = this.states[i];
            ObjectState clonedState = currState.Clone(clone);
            states.Add(clonedState);
            //if (this.currentState == currState) {
            //    clone.ChangeState(clonedState);
            //}
        }
        clone.SetStates(states);
    }
    public void SetIsMadeOf(RESOURCE resource) {
        _madeOf = resource;
    }
    public CharacterAttribute GetAttribute(ATTRIBUTE attribute) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].attribute == attribute) {
                return _attributes[i];
            }
        }
        return null;
    }
    public bool RemoveAttribute(ATTRIBUTE attributeType) {
        for (int i = 0; i < _attributes.Count; i++) {
            if (_attributes[i].attribute == attributeType) {
                _attributes.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public bool RemoveAttribute(CharacterAttribute attribute) {
        return _attributes.Remove(attribute);
    }
    public void SetIsDirty(bool state) {
        //if(_currentState.stateName != "Ruined") {
            _isDirty = state;
        //}
    }
    #endregion

    #region Attack Landmark
    public void AttackLandmark(BaseLandmark targetLandmark) {
        //int armyCount = 0;
        //if (this is Garrison) {
        //    armyCount = (this as Garrison).armyStrength;
        //} else if (this is DemonicPortal) {
        //    //armyCount = (this as DemonicPortal).armyStrength;
        //    armyCount = 100;
        //}
        //if (armyCount > 0) {
        //    this.objectLocation.SetIsAttackingAnotherLandmarkState(true);
        //    Army newArmy = new Army(this.objectLocation, armyCount);
        //    newArmy.SetTarget(targetLandmark);
        //}

    }
    public bool CanAttack(BaseLandmark landmark) {
        //if(this.objectLocation.owner != null && landmark.owner != null) {
        //    if(this.objectLocation.owner.id == landmark.owner.id) {
        //        return false;
        //    }
        //}
        if (this is Garrison || this is DemonicPortal) {
            return true;
        }
        return false;
    }

    //Gets the total number of civilians and multiply it with army percentage to get the army count needed to attack
    //private int GetArmyTotal() {
    //    return Mathf.CeilToInt((0.25f * (float) GetTotalCivilians()));
    //}
    #endregion

    #region For Testing: Imp Triggered Events
    public void StartImpTriggeredEvent() {
        GameDate impEventDate = GameManager.Instance.Today();
        int ticksToTriggerEvent = UnityEngine.Random.Range(10, 31);
        impEventDate.AddHours(ticksToTriggerEvent);
        SchedulingManager.Instance.AddEntry(impEventDate, () => ImpTriggeredEvent());
    }
    private void ImpTriggeredEvent() {
        if (_objectLocation.HasActiveInteraction()) {
            return; //the landmark already has an active interaction, other than investigate
        }
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(_objectLocation.specificLandmarkType);
        if (data.interactionWeights.GetTotalOfWeights() > 0) {
            INTERACTION_TYPE chosenInteraction = data.interactionWeights.PickRandomElementGivenWeights();
            //create interaction of type;
            Interaction createdInteraction = InteractionManager.Instance.CreateNewInteraction(chosenInteraction, _objectLocation);
            if (createdInteraction != null) {
                _objectLocation.AddInteraction(createdInteraction);
            }
        }
    }
    #endregion
}
