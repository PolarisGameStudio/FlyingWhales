﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private CharacterAction _action;
    private ECS.Character _character;

    [SerializeField] private Image progressBarImage;

    public void Initialize() {
        Messenger.AddListener<CharacterAction, CharacterParty>(Signals.ACTION_DAY_ADJUSTED, OnActionDayAdjusted);
    }
    public void SetCharacter(ECS.Character character) {
        _character = character;
    }
    public void SetAction(CharacterAction action) {
        _action = action;
        UpdateProgress();
    }

    public void UpdateProgress() {
        if (_character == null || _action == null) {
            return;
        }
        if (_action.actionData.duration == 0) {
            //Color color = progressBarImage.color;
            //color.a = 64f/255f;
            //progressBarImage.color = color;
            progressBarImage.fillAmount = 1f;
        } else {
            //Color color = progressBarImage.color;
            //color.a = 255f/255f;
            //progressBarImage.color = color;
            progressBarImage.fillAmount = (_character.currentParty as CharacterParty).actionData.currentDay / _action.actionData.duration;
        }
    }
    private void OnActionDayAdjusted(CharacterAction action, CharacterParty party) {
        if (_action == null || _character == null) {
            return;
        }
        if (_action == action && party.icharacters.Contains(_character)) {
            UpdateProgress();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (_action != null) {
            UIManager.Instance.ShowSmallInfo(_action.actionData.actionName);
        } else {
            UIManager.Instance.ShowSmallInfo("NONE");
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        UIManager.Instance.HideSmallInfo();
    }
}
