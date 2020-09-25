﻿using System;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Logs;
using UnityEngine.Assertions;

public class PlayerNotificationItem : PooledObject {
    
    public int tickShown { get; private set; }
    public string fromActionID { get; private set; }
    public string logPersistentID { get; private set; }
    
    [SerializeField] private TextMeshProUGUI logLbl;
    [SerializeField] private LogItem logItem;
    [SerializeField] private RectTransform _container;
    [SerializeField] private LayoutElement _layoutElement;
    private UIHoverPosition _hoverPosition;

    private Action<PlayerNotificationItem> onDestroyAction;
    private bool _adjustHeightOnEnable;
    private void OnEnable() {
        if (_adjustHeightOnEnable) {
            StartCoroutine(InstantHeight());
            _adjustHeightOnEnable = false;
        }
    }
    public void Initialize(in Log log, Action<PlayerNotificationItem> onDestroyAction = null) {
        logPersistentID = log.persistentID;
        tickShown = GameManager.Instance.Today().tick;
        logLbl.text = $"[{GameManager.ConvertTickToTime(tickShown)}] {log.logText}";
        fromActionID = log.actionID;
        this.onDestroyAction = onDestroyAction;
        Messenger.AddListener<Log>(Signals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
    }
    public void Initialize(in Log log, int tick, Action<PlayerNotificationItem> onDestroyAction = null) {
        logPersistentID = log.persistentID;
        tickShown = tick;
        logLbl.text = $"[{GameManager.ConvertTickToTime(tickShown)}] {log.logText}";
        fromActionID = log.actionID;
        this.onDestroyAction = onDestroyAction;
        
        Messenger.AddListener<Log>(Signals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
    }
    public void SetHoverPosition(UIHoverPosition hoverPosition) {
        _hoverPosition = hoverPosition;
    }
    public void DoTweenHeight() {
        StartCoroutine(TweenHeight());
    }
    public void QueueAdjustHeightOnEnable() {
        _adjustHeightOnEnable = true;
       
        
        // RectTransform thisRect = transform as RectTransform;
        // thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, sizeDelta.y);
    }
    private IEnumerator TweenHeight() {
        yield return null;
        _layoutElement.DOPreferredSize(new Vector2(0f, (logLbl.transform as RectTransform).sizeDelta.y), 0.5f);
        //_layoutElement.preferredHeight = (logLbl.transform as RectTransform).sizeDelta.y;
    }
    private IEnumerator InstantHeight() {
        yield return null;
        RectTransform logRect = logLbl.transform as RectTransform;
        var sizeDelta = logRect.sizeDelta;
        _layoutElement.preferredHeight = sizeDelta.y;
        // _layoutElement.DOPreferredSize(new Vector2(0f, (logLbl.transform as RectTransform).sizeDelta.y), 0.5f);
        //_layoutElement.preferredHeight = (logLbl.transform as RectTransform).sizeDelta.y;
    }
    public void DeleteNotification() {
        onDestroyAction?.Invoke(this);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    public virtual void DeleteOldestNotification() {
        DeleteNotification();
    }
    public void TweenIn() {
        _container.anchoredPosition = new Vector2(450f, 0f);
        _container.DOAnchorPosX(0f, 0.5f);
    }

    public void OnHoverOverLog(object obj) {
        if (obj is Character character && _hoverPosition != null) {
            UIManager.Instance.ShowCharacterNameplateTooltip(character, _hoverPosition);
        }
    }
    public void OnHoverOutLog() {
        UIManager.Instance.HideCharacterNameplateTooltip();
    }

    #region Listeners
    private void OnLogRemovedFromDatabase(Log log) {
        if (log.persistentID == logPersistentID) {
            Assert.IsFalse(this is IntelNotificationItem, $"Intel log was removed from database! This should never happen! {logPersistentID}");
            //if log in this notification is removed from database, then destroy it.
            DeleteNotification();
        }
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        _adjustHeightOnEnable = false;
        _container.anchoredPosition = Vector2.zero;
        transform.localScale = Vector3.one;
        fromActionID = string.Empty;
        logPersistentID = string.Empty;
        Messenger.RemoveListener<Log>(Signals.LOG_REMOVED_FROM_DATABASE, OnLogRemovedFromDatabase);
    }
    #endregion
}
