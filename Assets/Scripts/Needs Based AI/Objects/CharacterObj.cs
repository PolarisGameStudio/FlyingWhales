﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Object", menuName = "Objects/New Character Object")]
public class CharacterObj : ScriptableObject, ICharacterObject {
    [SerializeField] private OBJECT_TYPE _objectType;
    [SerializeField] private bool _isInvisible;
    [SerializeField] private List<ObjectState> _states;

    private ObjectState _currentState;

    #region getters/setters
    public string objectName {
        get { return this.name; }
    }
    public OBJECT_TYPE objectType {
        get { return _objectType; }
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
    #endregion


    public CharacterObj() {
     
    }

    public void ChangeState(ObjectState state) {

    }

    public IObject Clone() {
        CharacterObj clone = ScriptableObject.CreateInstance<CharacterObj>();
        clone.name = this.objectName;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._states = new List<ObjectState>();
        for (int i = 0; i < this.states.Count; i++) {
            ObjectState currState = this.states[i];
            clone._states.Add(currState.Clone(clone));
        }
        return clone;
    }
}