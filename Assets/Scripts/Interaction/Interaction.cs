﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction {
    protected int _id;
    protected string _name;
    protected int _timeOutTicks;
    protected GameDate _timeDate;
    protected INTERACTION_TYPE _type;
    protected IInteractable _interactable;
    protected Dictionary<string, InteractionState> _states;
    //protected InteractionItem _interactionItem;
    protected bool _isActivated;
    protected bool _isDone;
    protected bool _isFirstTimeOutCancelled;
    protected bool _isSecondTimeOutCancelled;
    protected InteractionState _currentState;
    protected Minion _explorerMinion;

    private bool _hasUsedBaseCreateStates;

    public const int secondTimeOutTicks = 30;

    #region getters/setters
    public INTERACTION_TYPE type {
        get { return _type; }
    }
    public string name {
        get { return _name; }
    }
    public GameDate timeDate {
        get { return _timeDate; }
    }
    public InteractionState currentState {
        get { return _currentState; }
    }
    public Minion explorerMinion {
        get { return _explorerMinion; }
    }
    //public InteractionItem interactionItem {
    //    get { return _interactionItem; }
    //}
    public IInteractable interactable {
        get { return _interactable; }
    }
    public bool isActivated {
        get { return _isActivated; }
    }
    #endregion
    public Interaction(IInteractable interactable, INTERACTION_TYPE type, int timeOutTicks) {
        _id = Utilities.SetID(this);
        _type = type;
        _interactable = interactable;
        _timeOutTicks = timeOutTicks;
        _isFirstTimeOutCancelled = false;
        _isSecondTimeOutCancelled = false;
        _hasUsedBaseCreateStates = false;
        _states = new Dictionary<string, InteractionState>();
        CreateStates();
        //Debug.Log("Created new interaction " + type.ToString() + " at " + interactable.name);
    }

    #region Virtuals
    public virtual void Initialize() {
        ScheduleFirstTimeOut();
    }
    public virtual void CreateStates() {
    }
    public virtual void CreateActionOptions(InteractionState state) { }
    public virtual void EndInteraction() {
        _isDone = true;
        _interactable.RemoveInteraction(this);
        InteractionUI.Instance.HideInteractionUI();
    }
    #endregion

    #region Utilities
    public void SetCurrentState(InteractionState state) {
        if(_currentState != null && _currentState.chosenOption != null) {
            state.SetAssignedMinion(_currentState.chosenOption.assignedMinion);
            _currentState.OnEndState();
        }
        _currentState = state;
        _currentState.OnStartState();
        Messenger.Broadcast(Signals.UPDATED_INTERACTION_STATE, this);
    }
    public void SetActivatedState(bool state) {
        _isActivated = state;
        //if (!state) {
        //    _currentState.SetChosenOption(null);
        //}
        Messenger.Broadcast(Signals.CHANGED_ACTIVATED_STATE, this);
    }
    public void CancelFirstTimeOut() {
        _isFirstTimeOutCancelled = true;
    }
    public void CancelSecondTimeOut() {
        _isSecondTimeOutCancelled = true;
    }
    public void ScheduleFirstTimeOut() {
        GameDate timeOutDate = GameManager.Instance.Today();
        timeOutDate.AddHours(_timeOutTicks);
        _timeDate = timeOutDate;
        SchedulingManager.Instance.AddEntry(_timeDate, () => FirstTimeOut());
    }
    public void ScheduleSecondTimeOut() {
        GameDate timeOutDate = GameManager.Instance.Today();
        timeOutDate.AddHours(secondTimeOutTicks);
        _timeDate = timeOutDate;
        SchedulingManager.Instance.AddEntry(_timeDate, () => SecondTimeOut());
    }
    //public void SetInteractionItem(InteractionItem interactionItem) {
    //    _interactionItem = interactionItem;
    //}
    //protected int GetRemainingDurationFromState(InteractionState state) {
    //    return GameManager.Instance.GetTicksDifferenceOfTwoDates(GameManager.Instance.Today(), state.timeDate);
    //}
    //protected void SetDefaultActionDurationAsRemainingTicks(string optionName, InteractionState stateFrom) {
    //    ActionOption option = stateFrom.GetOption(optionName);
    //    int remainingTicks = GameManager.Instance.GetTicksDifferenceOfTwoDates(GameManager.Instance.Today(), stateFrom.timeDate);
    //    option.duration = remainingTicks;
    //}
    protected void FirstTimeOut() {
        if (!_isFirstTimeOutCancelled) {
            TimedOutRunDefault();
        }
    }
    protected void SecondTimeOut() {
        if (!_isSecondTimeOutCancelled) {
            TimedOutRunDefault();
            _interactable.specificLocation.tileLocation.landmarkOnTile.landmarkInvestigation.ExploreLandmark();
        }
    }
    protected void TimedOutRunDefault() {
        if(_currentState.defaultOption == null) {
            return;
        }
        while (!_isDone) {
            _currentState.ActivateDefault();
        }
    }
    public void SetExplorerMinion(Minion minion) {
        _explorerMinion = minion;
        if(_explorerMinion != null) {
            _currentState.CreateLogs();
            _currentState.SetDescription();
        }
    }
    #endregion

    #region Shared States and Effects
    protected void CreateExploreStates() {
        _hasUsedBaseCreateStates = true;
        InteractionState exploreContinuesState = new InteractionState("Explore Continues", this);
        InteractionState exploreEndsState = new InteractionState("Explore Ends", this);

        exploreContinuesState.SetEndEffect(() => ExploreContinuesRewardEffect(exploreContinuesState));
        exploreEndsState.SetEndEffect(() => ExploreEndsRewardEffect(exploreEndsState));

        _states.Add(exploreContinuesState.name, exploreContinuesState);
        _states.Add(exploreEndsState.name, exploreEndsState);
    }
    protected void CreateWhatToDoNextState(string description) {
        InteractionState whatToDoNextState = new InteractionState("What To Do Next", this);
        //whatToDoNextState.SetDescription(description);

        ActionOption yesPleaseOption = new ActionOption {
            interactionState = whatToDoNextState,
            cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
            name = "Yes, please.",
            duration = 0,
            needsMinion = false,
            effect = () => ExploreContinuesOption(whatToDoNextState),
        };
        ActionOption noWayOption = new ActionOption {
            interactionState = whatToDoNextState,
            cost = new ActionOptionCost { amount = 0, currency = CURRENCY.SUPPLY },
            name = "No way.",
            duration = 0,
            needsMinion = false,
            effect = () => ExploreEndsOption(whatToDoNextState),
        };

        whatToDoNextState.AddActionOption(yesPleaseOption);
        whatToDoNextState.AddActionOption(noWayOption);
        whatToDoNextState.SetDefaultOption(noWayOption);

        _states.Add(whatToDoNextState.name, whatToDoNextState);

        if (!_hasUsedBaseCreateStates) {
            CreateExploreStates();
        }
    }
    protected void WhatToDoNextState() {
        SetCurrentState(_states["What To Do Next"]);
    }
    protected void LeaveAloneEffect(InteractionState state) {
        state.EndResult();
    }
    protected void SupplyRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " discovered a small cache of Supplies.");
        SetCurrentState(_states[effectName]);
        SupplyRewardEffect(_states[effectName]);
    }
    protected void SupplyRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    protected void ManaRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " discovered a source of magical energy. We have converted it into a small amount of Mana.");
        SetCurrentState(_states[effectName]);
        ManaRewardEffect(_states[effectName]);
    }
    protected void ManaRewardEffect(InteractionState state) {
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_1));
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }
    protected void NothingRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has returned with nothing to report.");
        SetCurrentState(_states[effectName]);
        NothingEffect(_states[effectName]);
    }
    protected void NothingEffect(InteractionState state) {
        explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
    }

    #region End Result Share States and Effects
    protected void DemonDisappearsRewardState(InteractionState state, string effectName) {
        //_states[effectName].SetDescription(explorerMinion.name + " has not returned. We can only assume the worst.");
        SetCurrentState(_states[effectName]);
    }
    protected void DemonDisappearsRewardEffect(InteractionState state) {
        explorerMinion.icharacter.Death();
        //PlayerManager.Instance.player.RemoveMinion(explorerMinion);
    }
    protected void ExploreContinuesRewardState(InteractionState state, string stateName) {
        //_states[stateName].SetDescription("We've instructed " + explorerMinion.name + " to continue its surveillance of the area.");
        SetCurrentState(_states[stateName]);
    }
    protected void ExploreContinuesRewardEffect(InteractionState state) {
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            landmark.landmarkInvestigation.ExploreLandmark();
        }
    }
    protected void ExploreEndsRewardState(InteractionState state, string stateName) {
        if (explorerMinion != null) {
            //_states[stateName].SetDescription("We've instructed " + explorerMinion.name + " to return.");
        }
        SetCurrentState(_states[stateName]);
    }
    protected void ExploreEndsRewardEffect(InteractionState state) {
        if(explorerMinion == null) {
            return;
        }
        if (_interactable is BaseLandmark) {
            BaseLandmark landmark = _interactable as BaseLandmark;
            landmark.landmarkInvestigation.RecallMinion();
        }
    }
    #endregion
    #endregion

    #region Shared Action Option
    protected void ExploreContinuesOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Continues", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Continues") {
            ExploreContinuesRewardState(state, chosenEffect);
        }
    }
    protected void ExploreEndsOption(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Explore Ends", 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == "Explore Ends") {
            ExploreEndsRewardState(state, chosenEffect);
        }
    }
    #endregion

    #region Utilities
    public bool AssignedMinionIsOfType(DEMON_TYPE type) {
        return this.explorerMinion != null && this.explorerMinion.type == type;
    }
    public bool AssignedMinionIsOfType(List<DEMON_TYPE> allowedTypes) {
        return this.explorerMinion != null && allowedTypes.Contains(this.explorerMinion.type);
    }
    #endregion

}
