﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using ECS;

[Serializable]
public class StructureObj : IObject {
    [SerializeField] protected OBJECT_TYPE _objectType;
    [SerializeField] protected SPECIFIC_OBJECT_TYPE _specificObjectType;
    [SerializeField] protected bool _isInvisible;
    [SerializeField] protected int _maxHP;
    [SerializeField] protected ActionEvent _onHPReachedZero;
    [SerializeField] protected ActionEvent _onHPReachedFull;

    protected List<ObjectState> _states;
    protected Dictionary<RESOURCE, int> _resourceInventory;

    protected string _objectName;
    [NonSerialized] protected ObjectState _currentState;
    protected BaseLandmark _objectLocation;
    protected int _currentHP;

    #region getters/setters
    public string objectName {
        get { return _objectName; }
    }
    public OBJECT_TYPE objectType {
        get { return _objectType; }
    }
    public SPECIFIC_OBJECT_TYPE specificObjectType {
        get { return _specificObjectType; }
    }
    public List<ObjectState> states {
        get { return _states; }
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
    public Dictionary<RESOURCE, int> resourceInventory {
        get { return _resourceInventory; }
    }
    public bool isHPFull {
        get { return _currentHP >= _maxHP; }
    }
    public bool isHPZero {
        get { return _currentHP == 0; }
    }
    #endregion

    public StructureObj() {
        ConstructResourceInventory();
    }

    #region Virtuals
    public virtual IObject Clone() {
        StructureObj clone = new StructureObj();
        clone.SetObjectName(this._objectName);
        clone._specificObjectType = this._specificObjectType;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._maxHP = this.maxHP;
        clone._onHPReachedZero = this._onHPReachedZero;
        clone._onHPReachedFull = this._onHPReachedFull;
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
        return clone;
    }
    public virtual IObject NewCopyObject(IObject iobject) {
        StructureObj clone = new StructureObj();
        clone.SetObjectName(this._objectName);
        clone._specificObjectType = this._specificObjectType;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._maxHP = this.maxHP;
        clone._onHPReachedZero = this._onHPReachedZero;
        clone._onHPReachedFull = this._onHPReachedFull;
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
    }
    #endregion

    #region Interface Requirements
    public void SetStates(List<ObjectState> states) {
        _states = states;
        ChangeState(states[0]);
    }
    public void SetObjectName(string name) {
        _objectName = name;
    }
    public void SetSpecificObjectType(SPECIFIC_OBJECT_TYPE specificObjectType) {
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
        _objectLocation = newLocation;
    }
    public void ChangeState(ObjectState state) {
        if (_currentState != null) {
            _currentState.OnEndState();
        }
        _currentState = state;
        _currentState.OnStartState();
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
        _currentHP = Mathf.Clamp(_currentHP, 0, 100);
        if (previousHP != _currentHP) {
            if (_currentHP == 0 && _onHPReachedZero != null) {
                _onHPReachedZero.Invoke(this);
            } else if (_currentHP == 100 && _onHPReachedFull != null) {
                _onHPReachedFull.Invoke(this);
            }
        }
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
    public int GetTotalCivilians() {
        int total = 0;
        foreach(RESOURCE resource in _resourceInventory.Keys) {
            if (resource.ToString().Contains("CIVILIAN")) {
                total += _resourceInventory[resource];
            }
        }
        //foreach (KeyValuePair<RESOURCE, int> kvp in _resourceInventory) {
        //    if (kvp.Key.ToString().Contains("CIVILIAN")) {
        //        total += kvp.Value;
        //    }
        //}
        return total;
    }
    #endregion
}
