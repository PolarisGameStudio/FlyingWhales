﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class SpellItem : NameplateItem<SpellData> {
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image cooldownCoverImage;
    [SerializeField] private TextMeshProUGUI currencyLbl;

    public SpellData spellData { get; private set; }

    //private Image _coverImg;

    private Func<SpellData, bool> _shouldBeInteractableChecker;

    public override void SetObject(SpellData spellData) {
        base.SetObject(spellData);
        name = spellData.name;
        button.name = spellData.name;
        toggle.name = spellData.name;
        this.spellData = spellData;
        UpdateData();
        Messenger.AddListener<SpellData>(SpellSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.AddListener<SpellData>(SpellSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SpellData>(SpellSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.AddListener<SpellData>(SpellSignals.ON_EXECUTE_PLAYER_SKILL, OnExecuteSpell);
        Messenger.AddListener<SpellData>(PlayerSignals.CHARGES_ADJUSTED, OnChargesAdjusted);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
        SetAsDefault();

        //_coverImg = coverGO.GetComponent<Image>();
        //_coverImg.type = Image.Type.Filled;
        //_coverImg.fillMethod = Image.FillMethod.Horizontal;
    }
    public void UpdateData() {
        mainLbl.text = spellData.name;
        currencyLbl.text = string.Empty;
        if (spellData.hasCharges) {
            currencyLbl.text += $"{UtilityScripts.Utilities.ChargesIcon()}{spellData.charges.ToString()}  ";
        }
        if (spellData.hasManaCost) {
            currencyLbl.text += $"{UtilityScripts.Utilities.ManaIcon()}{spellData.manaCost.ToString()} ";
        }
        if (spellData.hasCooldown) {
            currencyLbl.text += $"{UtilityScripts.Utilities.CooldownIcon()}{GameManager.GetTimeAsWholeDuration(spellData.cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(spellData.cooldown)}  ";
        }
        if (spellData.threat > 0) {
            currencyLbl.text += $"{UtilityScripts.Utilities.ThreatIcon()}{spellData.threat.ToString()} ";
        }
    }

    #region Listeners
    private void OnPlayerNoActiveSpell(SpellData spellData) {
        if(this.spellData == spellData) {
            if (_toggle.isOn) {
                _toggle.isOn = false;
            }
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnSpellCooldownStarted(SpellData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
            if (spellData is MinionPlayerSkill) {
                //do not check charges if spell is minion, because minion spells always regenerate, even if they have no more charges.
                SetCooldownState(spellData.isInCooldown);
                StartCooldownFill();
            } 
            // else if (spellData.hasCharges && spellData.charges <= 0) {
            //     //if spell uses charges, but has no more, do not show cooldown icon even if it is in cooldown
            //     SetCooldownState(false);
            // } 
            else {
                SetCooldownState(spellData.isInCooldown);
                StartCooldownFill();
            }
        }
    }
    private void OnSpellCooldownFinished(SpellData spellData) {
        if (this.spellData == spellData) {
            SetCooldownState(spellData.isInCooldown);
            UpdateData();
            // UpdateInteractableState();
            StopCooldownFill();
        }
    }
    private void OnExecuteSpell(SpellData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnChargesAdjusted(SpellData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnPlayerAdjustedMana(int adjusted, int mana) {
        UpdateData();
        UpdateInteractableState();
    }
    #endregion

    #region Utilities
    private void SetAsDefault() {
        SetAsToggle();
        ClearAllHoverEnterActions();
        ClearAllHoverExitActions();
        AddHoverEnterAction((spellData) => PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.spellListHoverPosition));
        AddHoverExitAction((spellData) => PlayerUI.Instance.OnHoverOutSpell(spellData));
    }
    #endregion

    #region Interactability
    private void SetCooldownState(bool state) {
        //cooldownImage.gameObject.SetActive(state);
        cooldownCoverImage.gameObject.SetActive(state);
    }
    public void ForceUpdateInteractableState() {
        UpdateInteractableState();
    }
    private void UpdateInteractableState() {
        SetInteractableState(_shouldBeInteractableChecker?.Invoke(spellData) ?? spellData.CanPerformAbility());
    }
    public void OnToggleSpell(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(spellData);
        }
    }
    public void SetInteractableChecker(System.Func<SpellData, bool> p_checker) {
        _shouldBeInteractableChecker = p_checker;
    }
    #endregion

    #region Cooldown
    public void UpdateCooldownFromLastState() {
        SetCooldownState(spellData.isInCooldown);
        if (cooldownCoverImage.gameObject.activeSelf) {
            float fillAmount = ((float) spellData.currentCooldownTick / spellData.cooldown);
            cooldownCoverImage.fillAmount = fillAmount;
            //Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
        }
    }
    private void StartCooldownFill() {
        cooldownCoverImage.fillAmount = 0f;
        PerTickCooldown();
        Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    private void PerTickCooldown() {
        float fillAmount = ((float)spellData.currentCooldownTick / spellData.cooldown);
        cooldownCoverImage.DOFillAmount(fillAmount, 0.4f);
    }
    private void StopCooldownFill() {
        cooldownCoverImage.fillAmount = 0f;
        UpdateInteractableState();
        //cooldownCoverImage.DOFillAmount(0f, 0.4f).OnComplete(UpdateInteractableState);
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    #endregion

    public override void Reset() {
        base.Reset();
        button.name = "Button";
        toggle.name = "Toggle";
        SetInteractableState(true);
        SetCooldownState(false);
        spellData = null;
        cooldownCoverImage.fillAmount = 0f;
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
        Messenger.RemoveListener<SpellData>(SpellSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.RemoveListener<SpellData>(SpellSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.RemoveListener<SpellData>(SpellSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.RemoveListener<SpellData>(SpellSignals.ON_EXECUTE_PLAYER_SKILL, OnExecuteSpell);
        Messenger.RemoveListener<SpellData>(PlayerSignals.CHARGES_ADJUSTED, OnChargesAdjusted);
        Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
    }
}
