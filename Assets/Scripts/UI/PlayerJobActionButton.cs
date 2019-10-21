﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerJobActionButton : MonoBehaviour {

    private Action clickAction;

    public PlayerJobActionSlot actionSlot { get; private set; }

    [SerializeField] private Button btn;
    [SerializeField] private TextMeshProUGUI btnLbl;
    [SerializeField] private TextMeshProUGUI subTextLbl;
    [SerializeField] private TextMeshProUGUI levelLbl;
    [SerializeField] private Image actionIcon;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private GameObject cover;
    [SerializeField] private UIHoverPosition hoverPos;
    //[SerializeField] private GameObject pointer;
    //private Character character;
    private object target;

    private void OnEnable() {
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobActionCooldownActivated);
        Messenger.AddListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobActionCooldownDone);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
        Messenger.AddListener<PlayerJobActionSlot>(Signals.PLAYER_GAINED_INTERVENE_LEVEL, OnJobActionGainLevel);
    }
    private void OnDisable() {
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, OnJobActionCooldownActivated);
        Messenger.RemoveListener<PlayerJobAction>(Signals.JOB_ACTION_COOLDOWN_DONE, OnJobActionCooldownDone);
        Messenger.RemoveListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.RemoveListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
        Messenger.RemoveListener<PlayerJobActionSlot>(Signals.PLAYER_GAINED_INTERVENE_LEVEL, OnJobActionGainLevel);
    }

    public void SetJobAction(PlayerJobActionSlot actionSlot) {
        this.actionSlot = actionSlot;
        if(actionSlot.ability != null) {
            actionIcon.sprite = PlayerManager.Instance.GetJobActionSprite(actionSlot.ability.name);
            UpdateButtonText();
            SetSelectedIconState(false);
            actionIcon.gameObject.SetActive(true);
        } else {
            SetSelectedIconState(false);
            actionIcon.gameObject.SetActive(false);
        }
        UpdateInteractableState();
        UpdateLevel();
    }

    //private void OnSeenActionButtons() {
    //    pointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    //}

    #region Visuals
    public void UpdateInteractableState() {
        SetInteractableState(
            //!action.parentData.hasActionInCooldown &&
            InteriorMapManager.Instance.isAnAreaMapShowing 
            && actionSlot.ability != null
            && PlayerManager.Instance.player.currentActivePlayerJobAction != this.actionSlot.ability && !actionSlot.ability.isInCooldown
        );
    }
    private void SetInteractableState(bool state) {
        btn.interactable = state;
        cover.SetActive(!state);
    }
    private void UpdateButtonText() {
        btnLbl.text = actionSlot.ability.name;
    }
    private void UpdateLevel() {
        levelLbl.text = actionSlot.level.ToString();
    }
    #endregion

    #region Click
    public void SetClickAction(Action action) {
        clickAction = action;
    }
    public void OnClickAction() {
        if (clickAction != null) {
            clickAction();
            UpdateInteractableState();
        }
    }
    public void SetSelectedIconState(bool state) {
        selectedIcon.gameObject.SetActive(state);
    }
    #endregion

    #region Hover
    public void ShowHoverText() {
        if(actionSlot.ability != null) {
            string header = actionSlot.ability.name;
            string message = actionSlot.ability.description;
            UIManager.Instance.ShowSmallInfo(message, hoverPos, header);
            //PlayerUI.Instance.ShowActionBtnTooltip(message, header);
        }
    }
    public void HideHoverText() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Listeners
    private void OnJobActionCooldownActivated(PlayerJobAction jobAction) {
        if (jobAction == this.actionSlot.ability) {
            UpdateInteractableState();
        }
    }
    private void OnJobActionCooldownDone(PlayerJobAction jobAction) {
        if (jobAction == this.actionSlot.ability) {
            UpdateInteractableState();
        }
    }
    private void OnJobActionGainLevel(PlayerJobActionSlot jobActionSlot) {
        if (jobActionSlot == this.actionSlot) {
            UpdateLevel();
        }
    }
    private void OnAreaMapOpened(Area area) {
        UpdateInteractableState();
    }
    private void OnAreaMapClosed(Area area) {
        //Upon closing of area map reset intervention ability cooldowns
        //action.InstantCooldown();
        UpdateInteractableState();
    }
    #endregion

}
