﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using Logs;

public class ActionIntel : IIntel, IDisposable {
    public ActualGoapNode node { get; private set; }

    #region getters
    public IReactable reactable => node;
    public Log log => node.descriptionLog;
    public Character actor => node.actor;
    public IPointOfInterest target => node.target;
    #endregion

    public ActionIntel(ActualGoapNode node) {
        this.node = node;
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, true);
    }
    public ActionIntel(SaveDataActionIntel data) {
        node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.node);
    }
    public void OnIntelRemoved() {
        //set is intel in database to false, so that it can be overwritten.
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, false);
    }

    #region Clean Up
    ~ActionIntel() {
        ReleaseUnmanagedResources();
    }
    private void ReleaseUnmanagedResources() {
        //release unmanaged resources here
        node = null;
    }
    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    #endregion
}
public class InterruptIntel : IIntel, IDisposable {
    //public Interrupt interrupt { get; private set; }
    //public Character actor { get; private set; }
    //public IPointOfInterest target { get; private set; }
    //public Log effectLog { get; private set; }
    public InterruptHolder interruptHolder { get; private set; }

    #region getters
    public IReactable reactable => interruptHolder;
    public Log log => interruptHolder.effectLog;
    public Character actor => interruptHolder.actor;
    public IPointOfInterest target => interruptHolder.target;
    #endregion

    public InterruptIntel(InterruptHolder interrupt) {
        interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
        interruptHolder.Initialize(interrupt.interrupt, interrupt.actor, interrupt.target, interrupt.identifier, interrupt.reason);
        interruptHolder.SetEffectLog(interrupt.effectLog);

        //This is set because the interrupt intel must copy the data of the interrupt
        interruptHolder.SetDisguisedActor(interrupt.disguisedActor);
        interruptHolder.SetDisguisedTarget(interrupt.disguisedTarget);
        
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, true);
    }
    public InterruptIntel(SaveDataInterruptIntel data) {
        interruptHolder = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.interruptHolder);
    }
    public void OnIntelRemoved() {
        //set is intel in database to false, so that it can be overwritten.
        DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, false);
    }
    
    #region Clean Up
    ~InterruptIntel() {
        ReleaseUnmanagedResources();
    }
    private void ReleaseUnmanagedResources() {
        //release unmanaged resources here
        interruptHolder = null;
    }
    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    #endregion
}

public interface IIntel {
    IReactable reactable { get; }
    Log log { get; }
    Character actor { get; }
    IPointOfInterest target { get; }

    /// <summary>
    /// Called whenever this intel is used up by the player or the notification it belongs to expires.
    /// </summary>
    void OnIntelRemoved();
}

[System.Serializable]
public class SaveDataActionIntel : SaveData<ActionIntel> {
    public string node;

    #region Overrides
    public override void Save(ActionIntel data) {
        node = data.node.persistentID;
        SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.node);
    }

    public override ActionIntel Load() {
        ActionIntel interrupt = new ActionIntel(this);
        return interrupt;
    }
    #endregion
}

[System.Serializable]
public class SaveDataInterruptIntel : SaveData<InterruptIntel> {
    public string interruptHolder;

    #region Overrides
    public override void Save(InterruptIntel data) {
        interruptHolder = data.interruptHolder.persistentID;
        SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.interruptHolder);
    }

    public override InterruptIntel Load() {
        InterruptIntel interrupt = new InterruptIntel(this);
        return interrupt;
    }
    #endregion
}
