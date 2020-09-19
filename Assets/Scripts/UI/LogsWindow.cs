﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LogsWindow : MonoBehaviour {
    [Space(10)] [Header("Logs")]
    [SerializeField] private GameObject logParentGO;
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private UIHoverPosition logHoverPosition;
    [SerializeField] private GameObject daySeparatorPrefab;
    [SerializeField] private TMP_InputField searchField;
    private List<LogHistoryItem> logHistoryItems;
    private List<DaySeparator> daySeparators;

    [Space(10)] [Header("Filters")] 
    [SerializeField] private GameObject filterGO;
    [SerializeField] private LogFilterItem[] allFilters;
    [SerializeField] private Toggle showAllToggle;

    private string _objPersistentID;
    private List<LOG_TAG> enabledFilters;
    private void OnDisable() {
        filterGO.gameObject.SetActive(false);
    }
    public void Initialize() {
        logHistoryItems = new List<LogHistoryItem>();
        daySeparators = new List<DaySeparator>();
        searchField.onValueChanged.AddListener(OnEndSearchEdit);
        
        //default logs filters to all be on.
        enabledFilters = UtilityScripts.CollectionUtilities.GetEnumValues<LOG_TAG>().ToList();
        showAllToggle.SetIsOnWithoutNotify(true);
        for (int i = 0; i < allFilters.Length; i++) {
            allFilters[i].SetIsOnWithoutNotify(true);
        }
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && EventSystem.current.currentSelectedGameObject == searchField.gameObject) {
            DoSearch();
        }
    }
    public void SetObjectPersistentID(string id) {
        _objPersistentID = id;
    }
    
    private void CreateNewLogHistoryItem() {
        GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
        newLogItem.transform.localScale = Vector3.one;
        newLogItem.SetActive(true);
        LogHistoryItem logHistoryItem = newLogItem.GetComponent<LogHistoryItem>();
        logHistoryItems.Add(logHistoryItem);
    }
    public void UpdateAllHistoryInfo() {
        List<Log> logs = DatabaseManager.Instance.mainSQLDatabase.GetLogsThatMatchCriteria(_objPersistentID, searchField.text, enabledFilters);
        int historyCount = logs?.Count ?? 0;
        int historyLastIndex = historyCount - 1;
        int missingItems = historyCount - logHistoryItems.Count;
        for (int i = 0; i < missingItems; i++) {
            CreateNewLogHistoryItem();
        }
        for (int i = 0; i < daySeparators.Count; i++) {
            ObjectPoolManager.Instance.DestroyObject(daySeparators[i]);
        }
        daySeparators.Clear();
        
        int currentDay = 0;
        for (int i = 0; i < logHistoryItems.Count; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            currItem.ManualReset();
            if(logs != null && i < historyCount) {
                Log currLog = logs[historyLastIndex - i];
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                currItem.SetHoverPosition(logHoverPosition);
                if (currLog.gameDate.day != currentDay) {
                    int siblingIndex = currItem.transform.GetSiblingIndex();
                    if (siblingIndex < 0) {
                        siblingIndex = 0;
                    }
                    CreateDaySeparator(currLog.gameDate.day, siblingIndex);
                    currentDay = currLog.gameDate.day;
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
    }
    private void CreateDaySeparator(int day, int indexInHierarchy) {
        //create day separator prefab
        GameObject dayGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(daySeparatorPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
        DaySeparator daySeparator = dayGO.GetComponent<DaySeparator>();
        daySeparator.SetDay(day);
        dayGO.transform.SetSiblingIndex(indexInHierarchy); //swap log with day label since log is the first log in the day
        daySeparators.Add(daySeparator);
    }
    public void ResetScrollPosition() {
        historyScrollView.verticalNormalizedPosition = 1;
    }

    #region Search
    public void DoSearch() {
        UpdateAllHistoryInfo();
    }
    private void OnEndSearchEdit(string text) {
        // if (string.IsNullOrEmpty(text)) {
        //     //only update automatically if text changed to empty
        //     UpdateAllHistoryInfo();
        // }
        UpdateAllHistoryInfo();
    }
    #endregion

    #region Filters
    public void ToggleFilters() {
        filterGO.gameObject.SetActive(!filterGO.activeInHierarchy);
    }
    public void OnToggleFilter(bool isOn, LOG_TAG tag) {
        if (isOn) {
            enabledFilters.Add(tag);
        } else {
            enabledFilters.Remove(tag);
        }
        showAllToggle.SetIsOnWithoutNotify(AreAllFiltersOn());
        UpdateAllHistoryInfo();
    }
    private bool AreAllFiltersOn() {
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            if (!filterItem.isOn) {
                return false;
            }
        }
        return true;
    }
    public void OnToggleAllFilters(bool state) {
        enabledFilters.Clear();
        for (int i = 0; i < allFilters.Length; i++) {
            LogFilterItem filterItem = allFilters[i];
            filterItem.SetIsOnWithoutNotify(state);
            if (state) {
                //if search all is enabled then add filter. If it is not do not do anything to the list since list was cleared beforehand.
                enabledFilters.Add(filterItem.filterType);    
            }
        }
        UpdateAllHistoryInfo();
    }
    #endregion
}
