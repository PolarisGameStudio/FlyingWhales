﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ECS;

public class Attribute {
    [SerializeField] protected string _name;
    [SerializeField] protected bool _isHidden;
    [SerializeField] protected ATTRIBUTE_CATEGORY _category;
    [SerializeField] protected List<ATTRIBUTE_BEHAVIOR> _tickBehaviors;
    [SerializeField] protected List<ATTRIBUTE_BEHAVIOR> _onAddedBehaviors;
    [SerializeField] protected List<ATTRIBUTE_BEHAVIOR> _onRemovedBehaviors;

    public event Action<Character> onTickBehavior;
    public event Action<Character> onAddBehavior;
    public event Action<Character> onRemoveBehavior;

    private Character _character;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public bool isHidden {
        get { return _isHidden; }
    }
    public ATTRIBUTE_CATEGORY category {
        get { return _category; }
    }
    public Character character {
        get { return _character; }
    }
    public List<ATTRIBUTE_BEHAVIOR> tickBehaviors {
        get { return _tickBehaviors; }
    }
    public List<ATTRIBUTE_BEHAVIOR> onAddedBehaviors {
        get { return _onAddedBehaviors; }
    }
    public List<ATTRIBUTE_BEHAVIOR> onRemovedBehaviors {
        get { return _onRemovedBehaviors; }
    }
    #endregion

    public void SetDataFromAttributePanelUI() {
        _name = AttributePanelUI.Instance.nameInput.text;
        _isHidden = AttributePanelUI.Instance.hiddenToggle.isOn;
        _category = (ATTRIBUTE_CATEGORY) System.Enum.Parse(typeof(ATTRIBUTE_CATEGORY), AttributePanelUI.Instance.categoryOptions.options[AttributePanelUI.Instance.categoryOptions.value].text);
        _tickBehaviors = new List<ATTRIBUTE_BEHAVIOR>();
        _onAddedBehaviors = new List<ATTRIBUTE_BEHAVIOR>();
        _onRemovedBehaviors = new List<ATTRIBUTE_BEHAVIOR>();

        for (int i = 0; i < AttributePanelUI.Instance.tickBehaviors.Count; i++) {
            _tickBehaviors.Add((ATTRIBUTE_BEHAVIOR) System.Enum.Parse(typeof(ATTRIBUTE_BEHAVIOR), AttributePanelUI.Instance.tickBehaviors[i]));
        }
        for (int i = 0; i < AttributePanelUI.Instance.addBehaviors.Count; i++) {
            _onAddedBehaviors.Add((ATTRIBUTE_BEHAVIOR) System.Enum.Parse(typeof(ATTRIBUTE_BEHAVIOR), AttributePanelUI.Instance.addBehaviors[i]));
        }
        for (int i = 0; i < AttributePanelUI.Instance.removeBehaviors.Count; i++) {
            _onRemovedBehaviors.Add((ATTRIBUTE_BEHAVIOR) System.Enum.Parse(typeof(ATTRIBUTE_BEHAVIOR), AttributePanelUI.Instance.removeBehaviors[i]));
        }
    }

    #region Utilities
    public void Initialize() {
        ConstructBehaviors();
    }
    public void PerformTickBehaviors() {
        if(onTickBehavior != null) {
            onTickBehavior(_character);
        }
    }
    public void OnAddAttribute(Character character) {
        _character = character;
        if (onAddBehavior != null) {
            onAddBehavior(_character);
        }
    }
    public void OnRemoveAttribute() {
        if (onRemoveBehavior != null) {
            onRemoveBehavior(_character);
        }
    }
    private void ConstructBehaviors() {
        for (int i = 0; i < _tickBehaviors.Count; i++) {
            Action<Character> action = AttributeManager.Instance.GetBehavior(_tickBehaviors[i]);
            onTickBehavior += action;
        }
        for (int i = 0; i < _onAddedBehaviors.Count; i++) {
            Action<Character> action = AttributeManager.Instance.GetBehavior(_onAddedBehaviors[i]);
            onAddBehavior += action;
        }
        for (int i = 0; i < _onRemovedBehaviors.Count; i++) {
            Action<Character> action = AttributeManager.Instance.GetBehavior(_onRemovedBehaviors[i]);
            onRemoveBehavior += action;
        }
    }
    #endregion
}
