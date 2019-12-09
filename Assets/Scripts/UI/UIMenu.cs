﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIMenu : MonoBehaviour {
    public Button backButton;
    public bool isShowing;
    private Action openMenuAction;

    protected object _data;

    #region virtuals
    internal virtual void Initialize() {
        Messenger.AddListener<UIMenu>(Signals.BEFORE_MENU_OPENED, BeforeMenuOpens);
    }
    /*
     When a menu is opened from being closed
         */
    public virtual void OpenMenu() {
        Messenger.Broadcast(Signals.BEFORE_MENU_OPENED, this);
        isShowing = true;
        this.gameObject.SetActive(true);
        if (openMenuAction != null) {
            openMenuAction();
            openMenuAction = null;
        }
        Messenger.Broadcast(Signals.MENU_OPENED, this);
        UIManager.Instance.AddToUIMenuHistory(_data);
        backButton.interactable = UIManager.Instance.GetLastUIMenuHistory() != null;
        UIManager.Instance.poiTestingUI.HideUI();
    }
    public virtual void CloseMenu() {
        isShowing = false;
        this.gameObject.SetActive(false);
        Messenger.Broadcast(Signals.MENU_CLOSED, this);
    }
    public virtual void GoBack() {
        object data = UIManager.Instance.GetLastUIMenuHistory();
        if(data != null) {
            CloseMenu();
            UIManager.Instance.RemoveLastUIMenuHistory();
            GoBackToPreviousUIMenu(data);
        }
    }
    public virtual void SetData(object data) {
        _data = data;
    }
    public virtual void ShowTooltip(GameObject objectHovered) {

    }
    public void ToggleMenu(bool state) {
        if (state) {
            OpenMenu();
        } else {
            CloseMenu();
        }
    }
    public void OnClickCloseMenu() {
        CloseMenu();
        UIManager.Instance.ClearUIMenuHistory();
    }
    #endregion

    public void SetOpenMenuAction(Action action) {
        openMenuAction = action;
    }
    private void GoBackToPreviousUIMenu(object data) {
        if(data != null) {
            if(data is Character) {
                UIManager.Instance.ShowCharacterInfo(data as Character);
            } else if (data is Area) {
                UIManager.Instance.ShowRegionInfo((data as Area).coreTile.region);
            } else if(data is Faction) {
                UIManager.Instance.ShowFactionInfo(data as Faction);
            } else if (data is TileObject) {
                UIManager.Instance.ShowTileObjectInfo(data as TileObject);
            } else if (data is Region) {
                UIManager.Instance.ShowRegionInfo(data as Region);
            } else if (data is HexTile) {
                UIManager.Instance.ShowRegionInfo((data as HexTile).region);
            } else if (data is SpecialToken) {
                UIManager.Instance.ShowItemInfo(data as SpecialToken);
            }
        }
    }

    private void BeforeMenuOpens(UIMenu menuToOpen) {
        if (this.isShowing && menuToOpen != this) {
            CloseMenu();
        }
    }

}
