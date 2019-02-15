﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelLine : MonoBehaviour {

    public GameObject holder;
    public RectTransform rectTransform;

    public Slider progressMeter;
    public Image fillImg, iconImg;
    public Sprite defaultSprite, hoverSprite, clickedSprite;

    private TravelLineParent _travelLineParent;
    private int _currentTick;
    private bool _isDone;

    private Character _character;

    #region getters/setters
    public bool isDone {
        get { return _isDone; }
    }
    public TravelLineParent travelLineParent {
        get { return _travelLineParent; }
    }
    #endregion

    public void Initialize() {
        _currentTick = 0;
        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    private void OnDestroy() {
        Messenger.RemoveListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    public void SetCharacter(Character character) {
        _character = character;
    }
    public void OnHoverTravelLine() {
        if(_character != null) {
            UIManager.Instance.ShowCharacterPortraitHoverInfo(_character);
            if(iconImg.sprite == defaultSprite) {
                iconImg.sprite = hoverSprite;
            }
        }
    }
    public void OnHoverOutTravelLine() {
        if(UIManager.Instance != null) {
            UIManager.Instance.HideCharacterPortraitHoverInfo();
            if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == _character.id) {
                iconImg.sprite = clickedSprite;
            } else {
                iconImg.sprite = defaultSprite;
            }
        }
    }
    public void OnClickTravelLine() {
        if (_character != null) {
            if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == _character.id) {
                iconImg.sprite = clickedSprite;
                return;
            }
            UIManager.Instance.ShowCharacterInfo(_character);
        }
    }
    public void SetColor(Color color) {
        //fillImg.color = new Color(color.r, color.g, color.b, 0);
        //iconImg.color = new Color(color.r, color.g, color.b, 1);
    }

    public void SetLineParent(TravelLineParent lineParent) {
        _travelLineParent = lineParent;
    }

    public void SetActiveMeter(bool state) {
        holder.SetActive(state);
        _travelLineParent.SetActiveBG(state);
    }

    public void AddProgress() {
        iTween.ValueTo(gameObject, iTween.Hash("from", (float) _currentTick, "to", (float) (_currentTick + 1), "time", GameManager.Instance.progressionSpeed, "onupdate", "TraverseLine"));
        _currentTick++;
        if(_currentTick >= _travelLineParent.numOfTicks) {
            _isDone = true;
        }
    }
    public void ReduceProgress() {
        iTween.ValueTo(gameObject, iTween.Hash("from", (float) _currentTick, "to", (float) (_currentTick - 1), "time", GameManager.Instance.progressionSpeed, "onupdate", "TraverseLine"));
        _currentTick--;
        if (_currentTick <= 0) {
            _isDone = true;
        }
    }
    private void TraverseLine(float val) {
        progressMeter.value = val / _travelLineParent.numOfTicks;
    }

    #region Listeners
    private void OnMenuOpened(UIMenu menu) {
        if(menu is CharacterInfoUI && UIManager.Instance.characterInfoUI.activeCharacter.id == _character.id) {
            iconImg.sprite = clickedSprite;
            if(UIManager.Instance.characterInfoUI.previousCharacter != null && !UIManager.Instance.characterInfoUI.previousCharacter.isDead && UIManager.Instance.characterInfoUI.previousCharacter.currentParty.icon.isTravelling) {
                UIManager.Instance.characterInfoUI.previousCharacter.currentParty.icon.travelLine.iconImg.sprite = UIManager.Instance.characterInfoUI.previousCharacter.currentParty.icon.travelLine.defaultSprite;
            }
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is CharacterInfoUI && UIManager.Instance.characterInfoUI.activeCharacter.id == _character.id) {
            iconImg.sprite = defaultSprite;
        }
    }
    #endregion
}
