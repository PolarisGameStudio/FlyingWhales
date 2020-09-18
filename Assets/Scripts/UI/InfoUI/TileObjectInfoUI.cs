﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Inner_Maps;
using Traits;

public class TileObjectInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI quantityLbl;
    [SerializeField] private TextMeshProUGUI ownerLbl;
    [SerializeField] private EventLabel ownerEventLbl;
    [SerializeField] private TextMeshProUGUI carriedByLbl;
    [SerializeField] private EventLabel carriedByEventLbl;
    [SerializeField] private TextMeshProUGUI statusTraitsLbl;
    [SerializeField] private TextMeshProUGUI normalTraitsLbl;
    [SerializeField] private EventLabel statusTraitsEventLbl;
    [SerializeField] private EventLabel normalTraitsEventLbl;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private Toggle charactersToggle;
    [SerializeField] private TextMeshProUGUI charactersToggleLbl;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject charactersGO;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    private List<LogHistoryItem> logHistoryItems;

    public TileObject activeTileObject { get; private set; }

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Log>(Signals.LOG_ADDED, UpdateLogsFromSignal);
        Messenger.AddListener<Log>(Signals.LOG_IN_DATABASE_UPDATED, UpdateLogsFromSignal);
        Messenger.AddListener<TileObject, Character>(Signals.ADD_TILE_OBJECT_USER, UpdateUsersFromSignal);
        Messenger.AddListener<TileObject, Character>(Signals.REMOVE_TILE_OBJECT_USER, UpdateUsersFromSignal);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_ADDED, UpdateTraitsFromSignal);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_REMOVED, UpdateTraitsFromSignal);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_STACKED, UpdateTraitsFromSignal);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_UNSTACKED, UpdateTraitsFromSignal);

        ownerEventLbl.SetOnClickAction(OnClickOwner);
        carriedByEventLbl.SetOnClickAction(OnClickCarriedBy);

        logHistoryItems = new List<LogHistoryItem>();
    }
    public override void CloseMenu() {
        base.CloseMenu();
        Selector.Instance.Deselect();
        if(activeTileObject != null && activeTileObject.mapVisual != null) {
            // activeTileObject.mapVisual.UnlockHoverObject();
            // activeTileObject.mapVisual.SetHoverObjectState(false);
            activeTileObject.mapVisual.UpdateSortingOrders(activeTileObject);
            if (InnerMapCameraMove.Instance.target == activeTileObject.mapObjectVisual.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeTileObject = null;
    }
    public override void OpenMenu() {
        TileObject previousTileObject = activeTileObject;
        // if (previousTileObject != null && previousTileObject.mapVisual != null) {
        //     // previousTileObject.mapVisual.UnlockHoverObject();
        //     // previousTileObject.mapVisual.SetHoverObjectState(false);
        //     Selector.Instance.Deselect();
        // }
        
        activeTileObject = _data as TileObject;
        if (previousTileObject != null && previousTileObject.mapVisual != null) {
            previousTileObject.mapVisual.UpdateSortingOrders(previousTileObject);
        }
        if(activeTileObject.gridTileLocation != null && activeTileObject.mapObjectVisual != null) {
            bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(activeTileObject.currentRegion);
            InnerMapCameraMove.Instance.CenterCameraOn(activeTileObject.mapObjectVisual.gameObject, instantCenter);
        }
        // activeTileObject.mapVisual.SetHoverObjectState(true);
        // activeTileObject.mapVisual.LockHoverObject();
        base.OpenMenu();
        if (activeTileObject.mapObjectVisual != null) {
            Selector.Instance.Select(activeTileObject, activeTileObject.mapObjectVisual.transform);    
            activeTileObject.mapVisual.UpdateSortingOrders(activeTileObject);
        }
        UIManager.Instance.HideObjectPicker();
        UpdateTabs();
        UpdateBasicInfo();
        UpdateInfo();
        UpdateTraits();
        UpdateUsers();
        UpdateLogs();
    }
    #endregion

    #region General
    public void UpdateTileObjectInfo() {
        if(activeTileObject == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateInfo();
    }
    private void UpdateTabs() {
        //Do not show Characters tab is Tile Object cannot have users/characters
        // if(activeTileObject.tileObjectType == TILE_OBJECT_TYPE.BED || activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TABLE || activeTileObject.tileObjectType == TILE_OBJECT_TYPE.TOMBSTONE) {
        if (activeTileObject.users != null) {
            charactersToggle.interactable = true;
            charactersToggleLbl.gameObject.SetActive(true);
        } else {
            charactersToggle.isOn = false;
            charactersToggle.interactable = false;
            charactersToggleLbl.gameObject.SetActive(false);
        }
    }
    private void UpdateBasicInfo() {
        nameLbl.text = activeTileObject.name;
    }
    private void UpdateInfo() {
        hpLbl.text = $"{activeTileObject.currentHP}/{activeTileObject.maxHP}";

        int quantity = 1;
        if(activeTileObject is ResourcePile) {
            quantity = (activeTileObject as ResourcePile).resourceInPile;
        } else if (activeTileObject is Table) {
            quantity = activeTileObject.storedResources[RESOURCE.FOOD];
        }
        quantityLbl.text = $"{quantity}";

        ownerLbl.text = activeTileObject.characterOwner != null ? $"<link=\"1\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeTileObject.characterOwner.name)}</link>" : "None";
        carriedByLbl.text = activeTileObject.isBeingCarriedBy != null ? $"<link=\"1\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeTileObject.isBeingCarriedBy.name)}</link>" : "None";
    }
    private void UpdateTraits() {
        string statusTraits = string.Empty;
        string normalTraits = string.Empty;

        for (int i = 0; i < activeTileObject.traitContainer.statuses.Count; i++) {
            Status currStatus = activeTileObject.traitContainer.statuses[i];
            if (currStatus.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (!string.IsNullOrEmpty(statusTraits)) {
                statusTraits = $"{statusTraits}, ";
            }
            statusTraits = $"{statusTraits}<b><color={color}><link=\"{i}\">{currStatus.GetNameInUI(activeTileObject)}</link></color></b>";
        }
        for (int i = 0; i < activeTileObject.traitContainer.traits.Count; i++) {
            Trait currTrait = activeTileObject.traitContainer.traits[i];
            if (currTrait.isHidden) {
                continue; //skip
            }
            string color = UIManager.normalTextColor;
            if (currTrait.type == TRAIT_TYPE.BUFF) {
                color = UIManager.buffTextColor;
            } else if (currTrait.type == TRAIT_TYPE.FLAW) {
                color = UIManager.flawTextColor;
            }
            if (!string.IsNullOrEmpty(normalTraits)) {
                normalTraits = $"{normalTraits}, ";
            }
            normalTraits = $"{normalTraits}<b><color={color}><link=\"{i}\">{currTrait.GetNameInUI(activeTileObject)}</link></color></b>";
        }

        statusTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(statusTraits) == false) {
            //character has status traits
            statusTraitsLbl.text = statusTraits;
        }
        normalTraitsLbl.text = string.Empty;
        if (string.IsNullOrEmpty(normalTraits) == false) {
            //character has normal traits
            normalTraitsLbl.text = normalTraits;
        }
    }
    private void UpdateUsers() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        Character[] users = activeTileObject.users;
        if (users != null && users.Length > 0) {
            for (int i = 0; i < users.Length; i++) {
                Character character = users[i];
                if(character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
    public void UpdateLogs() {
        List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetLogsMentioning(activeTileObject.persistentID);
        int historyCount = logs.Count;
        int historyLastIndex = historyCount - 1;
        int missingItems = historyCount - logHistoryItems.Count;
        for (int i = 0; i < missingItems; i++) {
            CreateNewLogHistoryItem();
        }
        for (int i = 0; i < logHistoryItems.Count; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if(i < historyCount) {
                Log currLog = logs[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
    }
    private LogHistoryItem CreateNewLogHistoryItem() {
        GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
        newLogItem.transform.localScale = Vector3.one;
        newLogItem.SetActive(true);
        LogHistoryItem logHistoryItem = newLogItem.GetComponent<LogHistoryItem>();
        logHistoryItems.Add(logHistoryItem);
        return logHistoryItem;
    }
    #endregion

    #region Listeners
    private void UpdateLogsFromSignal(Log log) {
        if(isShowing && log.IsInvolved(activeTileObject)) {
            UpdateLogs();
        }
    }
    private void UpdateUsersFromSignal(TileObject tileObject, Character user) {
        if(isShowing && activeTileObject == tileObject) {
            UpdateUsers();
        }
    }
    private void UpdateTraitsFromSignal(TileObject tileObject, Trait trait) {
        if (isShowing && activeTileObject == tileObject) {
            UpdateTraits();
        }
    }
    #endregion

    #region Event Labels
    private void OnClickOwner(object obj) {
        if(activeTileObject.characterOwner != null) {
            UIManager.Instance.ShowCharacterInfo(activeTileObject.characterOwner, true);
        }
    }
    private void OnClickCarriedBy(object obj) {
        if (activeTileObject.isBeingCarriedBy != null) {
            UIManager.Instance.ShowCharacterInfo(activeTileObject.isBeingCarriedBy, true);
        }
    }
    public void OnHoverTrait(object obj) {
        if (obj is string) {
            string text = (string) obj;
            int index = int.Parse(text);
            Trait trait = activeTileObject.traitContainer.traits[index];
            UIManager.Instance.ShowSmallInfo(trait.descriptionInUI);
        }
    }
    public void OnHoverStatus(object obj) {
        if (obj is string) {
            string text = (string) obj;
            int index = int.Parse(text);
            Trait trait = activeTileObject.traitContainer.statuses[index];
            UIManager.Instance.ShowSmallInfo(trait.descriptionInUI);
        }
    }
    public void OnHoverOutTrait() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Logs
    private void ClearLogs() {
        for (int i = 0; i < logHistoryItems.Count; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            currItem.gameObject.SetActive(false);
        }
    }
    private void ResetAllScrollPositions() {
        historyScrollView.verticalNormalizedPosition = 1;
    }
    #endregion
}
