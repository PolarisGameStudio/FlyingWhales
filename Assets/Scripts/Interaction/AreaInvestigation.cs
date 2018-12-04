﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class AreaInvestigation {
    private Area _area;
    private Minion _assignedMinion;
    private Minion _assignedMinionAttack;
    private BaseLandmark _currentlyExploredLandmark;
    private BaseLandmark _currentlyAttackedLandmark;
    private bool _isExploring;
    private bool _isAttacking;
    private bool _isMinionRecalledAttack;
    private bool _isMinionRecalledExplore;

    //Explore
    private int _duration;
    private int _currentTick;
    private Interaction _currentInteraction;

    #region getters/setters
    public Area area {
        get { return _area; }
    }
    public Minion assignedMinion {
        get { return _assignedMinion; }
    }
    public Minion assignedMinionAttack {
        get { return _assignedMinionAttack; }
    }
    public bool isExploring {
        get { return _isExploring; }
    }
    public bool isAttacking {
        get { return _isAttacking; }
    }
    public bool isMinionRecalledAttack {
        get { return _isMinionRecalledAttack; }
    }
    public bool isMinionRecalledExplore {
        get { return _isMinionRecalledExplore; }
    }
    #endregion

    public AreaInvestigation(Area area) {
        _area = area;
        //Messenger.AddListener<BaseLandmark>(Signals.CLICKED_INTERACTION_BUTTON, ClickedInteractionTimerButton);
    }

    public void SetAssignedMinion(Minion minion) {
        _assignedMinion = minion;
    }
    public void SetAssignedMinionAttack(Minion minion) {
        _assignedMinionAttack = minion;
    }
    public void InvestigateLandmark(Minion minion) {
        SetAssignedMinion(minion);
        _assignedMinion.SetEnabledState(false);
        _assignedMinion.SetExploringArea(_area);

        MinionGoToAssignment(ExploreArea, "explore");

        _isExploring = true;
    }
    public void AttackRaidLandmark(string whatTodo, Minion[] minion, BaseLandmark targetLandmark) {
        _assignedMinionAttack = null;
        for (int i = 0; i < minion.Length; i++) {
            if (minion[i] != null) {
                if (_assignedMinionAttack == null) {
                    SetAssignedMinionAttack(minion[i]);
                } else {
                    _assignedMinionAttack.character.ownParty.AddCharacter(minion[i].character);
                }
            }
        }
        
        _assignedMinionAttack.SetEnabledState(false);
        _assignedMinionAttack.SetAttackingArea(_area);
        _currentlyAttackedLandmark = targetLandmark;
        MinionGoToAssignment(AttackLandmark, "attack");
        _isAttacking = true;
    }
    private void MinionGoToAssignment(Action action, string whatToDo) {
        if (whatToDo == "explore") {
            _assignedMinion.TravelToAssignment(_area.coreTile.landmarkOnTile, action);
        } else if (whatToDo == "attack") {
            _assignedMinionAttack.TravelToAssignment(_currentlyAttackedLandmark, action);
        }
    }
    public void RecallMinion(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.TravelBackFromAssignment(() => SetMinionRecallExploreState(false));
            _assignedMinion.character.job.StopJobAction();
            _assignedMinion.character.job.StopCreatedInteraction();
            //Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            UnexploreLandmark();
            SetMinionRecallExploreState(true);
            UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("explore");
        }
        if (_isAttacking && action == "attack") {
            _assignedMinionAttack.TravelBackFromAssignment(() => SetMinionRecallAttackState(false));
            UnattackLandmark();
            SetMinionRecallAttackState(true);
            UIManager.Instance.landmarkInfoUI.OnUpdateLandmarkInvestigationState("attack");
        }
    }
    public void CancelInvestigation(string action) {
        if (_isExploring && action == "explore") {
            _assignedMinion.SetEnabledState(true);
            _assignedMinion.character.job.StopJobAction();
            //character.job.StopCreatedInteraction();
            if (!_assignedMinion.character.isDead) {
                _assignedMinion.character.job.StopCreatedInteraction();
            }

            //if (_currentlyExploredLandmark != null) {
            //    _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
            //    _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
            //}
            //Messenger.RemoveListener(Signals.HOUR_STARTED, OnExploreTick);
            UnexploreLandmark();
        }
        if (_isAttacking && action == "attack") {
            _assignedMinionAttack.SetEnabledState(true);
            UnattackLandmark();
        }
    }
    public void SetMinionRecallExploreState(bool state) {
        _isMinionRecalledExplore = state;
    }
    public void SetMinionRecallAttackState(bool state) {
        _isMinionRecalledAttack = state;
    }
    public void SetCurrentInteraction(Interaction interaction) {
        _currentInteraction = interaction;
    }

    #region Explore
    public void ExploreArea() {
        if (_assignedMinion == null) {
            return;
        }
        if (!_area.hasBeenInspected) {
            _area.SetHasBeenInspected(true);
        }
        _assignedMinion.character.job.StartJobAction();
        //_duration = 30;
        //_currentTick = 0;
        //Messenger.AddListener(Signals.HOUR_STARTED, OnExploreTick);
        //if (_currentlyExploredLandmark != null) {
        //    _currentlyExploredLandmark.landmarkVisual.StopInteractionTimer();
        //    _currentlyExploredLandmark.landmarkVisual.HideInteractionTimer();
        //}
        //_area.coreTile.landmarkOnTile.landmarkVisual.SetAndStartInteractionTimer(_duration);
        //_area.coreTile.landmarkOnTile.landmarkVisual.ShowNoInteractionForeground();
        //_area.coreTile.landmarkOnTile.landmarkVisual.ShowInteractionTimer();
    }
    public void UnexploreLandmark() {
        //if (_area.isBeingInspected) {
        //    _area.SetIsBeingInspected(false);
        //    _area.EndedInspection();
        //}
        _isExploring = false;
        _assignedMinion.SetExploringArea(null);
        SetAssignedMinion(null);
        _currentlyExploredLandmark = null;
        UIManager.Instance.areaInfoUI.ResetMinionAssignment();
    }
    public void UnattackLandmark() {
        _isAttacking = false;
        _assignedMinionAttack.SetAttackingArea(null);
        SetAssignedMinionAttack(null);
        _currentlyAttackedLandmark = null;
        UIManager.Instance.areaInfoUI.ResetMinionAssignmentParty();
    }
    //private void ClickedInteractionTimerButton(BaseLandmark landmark) {
    //    if (_assignedMinion != null && landmark.tileLocation.areaOfTile == _area) {
    //        Character character = _assignedMinion.icharacter as Character;
    //        character.job.createdInteraction.CancelSecondTimeOut();
    //        //_currentInteraction.SetExplorerMinion(_assignedMinion);
    //        character.job.createdInteraction.OnInteractionActive();
    //        InteractionUI.Instance.OpenInteractionUI(character.job.createdInteraction);
    //    }
    //}
    #endregion


    #region Attack
    private void AttackLandmark() {
        DefenderGroup defender = _area.GetFirstDefenderGroup();
        if (defender != null) {
            //_assignedMinionAttack.icharacter.currentParty.specificLocation.RemoveCharacterFromLocation(_assignedMinionAttack.icharacter.currentParty);
            //_currentlyAttackedLandmark.AddCharacterToLocation(_assignedMinionAttack.icharacter.currentParty);
            Combat combat = _assignedMinionAttack.character.currentParty.CreateCombatWith(defender.party);
            combat.Fight(() => AttackCombatResult(combat));
        } else {
            RecallMinion("attack");
            //Destroy Area
        }
    }
    private void AttackCombatResult(Combat combat) {
        if (_isAttacking) { //when the minion dies, isActivated will become false, hence, it must not go through the result
            if (combat.winningSide == _assignedMinionAttack.character.currentSide) {
                RecallMinion("attack");
                if(_area.GetFirstDefenderGroup() == null) {
                    //Destroy Area
                }
            }
        }
    }
    #endregion
}
