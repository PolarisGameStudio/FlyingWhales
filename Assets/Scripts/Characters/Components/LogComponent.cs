﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogComponent  {
    public Character owner { get; private set; }
    public List<Log> history { get; private set; }

    private string _planCostLog;

    public LogComponent(Character owner) {
        this.owner = owner;
        history = new List<Log>();
    }

    #region History
    public bool AddHistory(Log log) {
        if (!history.Contains(log)) {
            history.Add(log);
            if (history.Count > 300) {
                history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, owner as object);
            if (owner.isLycanthrope) {
                owner.lycanData.limboForm.logComponent.AddHistory(log);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// What should happen if another character sees this character?
    /// </summary>
    /// <param name="character">The character that saw this character.</param>
    public List<Log> GetMemories(int dayFrom, int dayTo, bool eventMemoriesOnly = false) {
        List<Log> memories = new List<Log>();
        if (eventMemoriesOnly) {
            for (int i = 0; i < history.Count; i++) {
                if (history[i].node != null) {
                    if (history[i].day >= dayFrom && history[i].day <= dayTo) {
                        memories.Add(history[i]);
                    }
                }
            }
        } else {
            for (int i = 0; i < history.Count; i++) {
                if (history[i].day >= dayFrom && history[i].day <= dayTo) {
                    memories.Add(history[i]);
                }
            }
        }
        return memories;
    }
    #endregion

    #region Notifications
    public void RegisterLogAndShowNotifToThisCharacterOnly(Log addLog, GoapAction goapAction = null, bool onlyClickedCharacter = true) {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        addLog.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFrom(addLog, owner, onlyClickedCharacter);
    }
    #endregion

    #region Debug
    public void PrintLogIfActive(string log) {
        //if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
        Debug.Log(GameManager.Instance.TodayLogString() + log);
        //}
    }
    public void PrintLogErrorIfActive(string log) {
        //if (InteriorMapManager.Instance.currentlyShowingArea == specificLocation) {//UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == this
        Debug.LogError(GameManager.Instance.TodayLogString() + log);
        //}
    }
    #endregion
    
    #region Goap Planning Cost Log
    public void ClearCostLog() {
        _planCostLog = string.Empty;
    }
    public void AppendCostLog(string text) {
        _planCostLog += text;
    }
    public void PrintCostLog(){
        PrintLogIfActive(_planCostLog);   
    }
    #endregion
}