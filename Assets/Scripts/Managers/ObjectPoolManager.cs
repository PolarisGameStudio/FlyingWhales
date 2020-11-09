﻿using UnityEngine;
using System.Collections;
using EZObjectPools;
using System.Collections.Generic;
using System;
using System.Linq;
using Interrupts;

public class ObjectPoolManager : BaseMonoBehaviour {

    public static ObjectPoolManager Instance = null;

    private Dictionary<string, EZObjectPool> allObjectPools;

    [SerializeField] private GameObject[] UIPrefabs;
    [SerializeField] internal GameObject[] otherPrefabs;
    [SerializeField] private GameObject UIObjectPoolParent;

    public List<GoapNode> goapNodesPool { get; private set; }
    public List<OpinionData> opinionDataPool { get; private set; }
    public List<TraitRemoveSchedule> traitRemoveSchedulePool { get; private set; }
    public List<CombatData> combatDataPool { get; private set; }
    public List<InterruptHolder> _interruptPool { get; private set; }
    public List<Party> _partyPool { get; private set; }
    public List<GoapThread> _goapThreadPool { get; private set; }
    private List<LogDatabaseThread> _logDatabaseThreadPool;

    private void Awake() {
        Instance = this;
        allObjectPools = new Dictionary<string, EZObjectPool>();
    }
    public void InitializeObjectPools() {
        for (int i = 0; i < UIPrefabs.Length; i++) {
            GameObject currPrefab = UIPrefabs[i];
            int size = 0;
            if (currPrefab.name == "LogHistoryItem") {
                size = 2000; //automatically create 200 log history items for performance in game
            }
            EZObjectPool newUIPool = CreateNewPool(currPrefab, currPrefab.name, size, true, true, false); //100
            newUIPool.transform.SetParent(UIObjectPoolParent.transform, false);
        }

        for (int i = 0; i < otherPrefabs.Length; i++) {
            GameObject currPrefab = otherPrefabs[i];
            CreateNewPool(currPrefab, currPrefab.name, 0, true, true, false); //50    
        }

        ConstructGoapNodes();
        ConstructOpinionDataPool();
        ConstructTraitRemoveSchedulePool();
        ConstructCombatDataPool();
        ConstructInterruptPool();
        ConstructPartyPool();
        ConstructGoapThreadPool();
        ConstructLogDatabaseThreadPool();
    }

    public GameObject InstantiateObjectFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null, bool isWorldPosition = false) {
        poolName = poolName.ToUpper();
        if (!allObjectPools.ContainsKey(poolName)) {
            throw new Exception($"Object Pool does not have key {poolName}");
        }
        GameObject instantiatedObj = null;
        EZObjectPool objectPoolToUse = allObjectPools[poolName];

        if(ReferenceEquals(objectPoolToUse, null)) {
            throw new Exception($"Cannot find an object pool with name {poolName}");
        } else {
            if(objectPoolToUse.TryGetNextObject(Vector3.zero, rotation, out instantiatedObj)) {
                if(ReferenceEquals(parent, null) == false) {
                    instantiatedObj.transform.SetParent(parent, false);
                }
                instantiatedObj.transform.localScale = objectPoolToUse.Template.transform.localScale;
                if (isWorldPosition) {
                    instantiatedObj.transform.position = position;
                } else {
                    instantiatedObj.transform.localPosition = position;    
                }
                
            }
        }
        instantiatedObj.SetActive(true);
        return instantiatedObj;
    }
    public GameObject GetOriginalObjectFromPool(string poolName) {
        poolName = poolName.ToUpper();
        if (!allObjectPools.ContainsKey(poolName)) {
            throw new Exception($"Object Pool does not have key {poolName}");
        }
        EZObjectPool objectPoolToUse = allObjectPools[poolName];
        return objectPoolToUse.Template;
    }
    
    public void DestroyObject(PooledObject pooledObject) {
        PooledObject[] pooledObjects = pooledObject.GetComponents<PooledObject>();
        Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, pooledObject.gameObject);
        pooledObject.BeforeDestroyActions();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].BeforeDestroyActions();
        }
        pooledObject.SendObjectBackToPool();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].Reset();
        }
        pooledObject.transform.SetParent(pooledObject.ParentPool.transform);
    }
    public void DestroyObject(GameObject gameObject) {
        PooledObject[] pooledObjects = gameObject.GetComponents<PooledObject>();
        Messenger.Broadcast(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, gameObject);
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].BeforeDestroyActions();
        }
        pooledObjects[0].SendObjectBackToPool();
        for (int i = 0; i < pooledObjects.Length; i++) {
            pooledObjects[i].Reset();
        }
        pooledObjects[0].transform.SetParent(pooledObjects[0].ParentPool.transform);
    }

    public EZObjectPool CreateNewPool(GameObject template, string poolName, int size, bool autoResize, bool instantiateImmediate, bool shared) {
        poolName = poolName.ToUpper();
        EZObjectPool newPool = EZObjectPool.CreateObjectPool(template, poolName, size, autoResize, instantiateImmediate, shared);
        //try {
            allObjectPools.Add(poolName, newPool);
        //}catch(Exception e) {
        //    throw new Exception(e.Message + " Pool name " + poolName);
        //}
        
        return newPool;
    }

    public bool HasPool(string key) {
        if (allObjectPools.ContainsKey(key)) {
            return true;
        }
        return false;
    }

    #region Goap Node
    private void ConstructGoapNodes() {
        goapNodesPool = new List<GoapNode>();
    }
    public GoapNode CreateNewGoapPlanJob(int cost, int level, GoapAction action, IPointOfInterest target) {
        GoapNode node = GetGoapNodeFromPool();
        node.Initialize(cost, level, action, target);
        return node;
    }
    public void ReturnGoapNodeToPool(GoapNode node) {
        node.Reset();
        goapNodesPool.Add(node);
    }
    private GoapNode GetGoapNodeFromPool() {
        if(goapNodesPool.Count > 0) {
            GoapNode node = goapNodesPool[0];
            goapNodesPool.RemoveAt(0);
            return node;
        }
        return new GoapNode();
    }
    #endregion

    #region Opinion Data
    private void ConstructOpinionDataPool() {
        opinionDataPool = new List<OpinionData>();
    }
    public OpinionData CreateNewOpinionData() {
        OpinionData data = GetOpinionDataFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnOpinionDataToPool(OpinionData data) {
        data.Reset();
        opinionDataPool.Add(data);
    }
    private OpinionData GetOpinionDataFromPool() {
        if (opinionDataPool.Count > 0) {
            OpinionData data = opinionDataPool[0];
            opinionDataPool.RemoveAt(0);
            return data;
        }
        return new OpinionData();
    }
    #endregion

    #region Trait Remove Schedule
    private void ConstructTraitRemoveSchedulePool() {
        traitRemoveSchedulePool = new List<TraitRemoveSchedule>();
    }
    public TraitRemoveSchedule CreateNewTraitRemoveSchedule() {
        TraitRemoveSchedule data = GetTraitRemoveScheduleFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnTraitRemoveScheduleToPool(TraitRemoveSchedule data) {
        data.Reset();
        traitRemoveSchedulePool.Add(data);
    }
    private TraitRemoveSchedule GetTraitRemoveScheduleFromPool() {
        if (traitRemoveSchedulePool.Count > 0) {
            TraitRemoveSchedule data = traitRemoveSchedulePool[0];
            traitRemoveSchedulePool.RemoveAt(0);
            return data;
        }
        return new TraitRemoveSchedule();
    }
    #endregion

    #region Combat Data
    private void ConstructCombatDataPool() {
        combatDataPool = new List<CombatData>();
    }
    public CombatData CreateNewCombatData() {
        CombatData data = GetCombatDataFromPool();
        data.Initialize();
        return data;
    }
    public void ReturnCombatDataToPool(CombatData data) {
        data.Reset();
        combatDataPool.Add(data);
    }
    private CombatData GetCombatDataFromPool() {
        if (combatDataPool.Count > 0) {
            CombatData data = combatDataPool[0];
            combatDataPool.RemoveAt(0);
            return data;
        }
        return new CombatData();
    }
    #endregion

    #region Interrupts
    private void ConstructInterruptPool() {
        _interruptPool = new List<InterruptHolder>();
    }
    public InterruptHolder CreateNewInterrupt() {
        InterruptHolder data = GetInterruptFromPool();
        return data;
    }
    public void ReturnInterruptToPool(InterruptHolder data) {
        if (data.shouldNotBeObjectPooled) {
            return;
        }
        data.Reset();
        _interruptPool.Add(data);
    }
    private InterruptHolder GetInterruptFromPool() {
        if (_interruptPool.Count > 0) {
            InterruptHolder data = _interruptPool[0];
            _interruptPool.RemoveAt(0);
            return data;
        }
        return new InterruptHolder();
    }
    #endregion

    #region Party
    private void ConstructPartyPool() {
        _partyPool = new List<Party>();
    }
    public Party CreateNewParty() {
        Party data = GetPartyFromPool();
        return data;
    }
    public void ReturnPartyToPool(Party data) {
        data.Reset();
        _partyPool.Add(data);
    }
    private Party GetPartyFromPool() {
        if (_partyPool.Count > 0) {
            Party data = _partyPool[0];
            _partyPool.RemoveAt(0);
            return data;
        }
        return new Party();
    }
    #endregion

    #region Goap Thread
    private void ConstructGoapThreadPool() {
        _goapThreadPool = new List<GoapThread>();
    }
    public GoapThread CreateNewGoapThread() {
        GoapThread data = GetGoapThreadFromPool();
        return data;
    }
    public void ReturnGoapThreadToPool(GoapThread data) {
        data.Reset();
        _goapThreadPool.Add(data);
    }
    private GoapThread GetGoapThreadFromPool() {
        if (_goapThreadPool.Count > 0) {
            GoapThread data = _goapThreadPool[0];
            _goapThreadPool.RemoveAt(0);
            return data;
        }
        return new GoapThread();
    }
    #endregion
    
    #region Database Thread
    private void ConstructLogDatabaseThreadPool() {
        _logDatabaseThreadPool = new List<LogDatabaseThread>();
    }
    public LogDatabaseThread CreateNewLogDatabaseThread() {
        LogDatabaseThread data = GetLogDatabaseThreadFromPool();
        return data;
    }
    public void ReturnLogDatabaseThreadToPool(LogDatabaseThread data) {
        data.Reset();
        _logDatabaseThreadPool.Add(data);
    }
    private LogDatabaseThread GetLogDatabaseThreadFromPool() {
        if (_logDatabaseThreadPool.Count > 0) {
            LogDatabaseThread data = _logDatabaseThreadPool[0];
            _logDatabaseThreadPool.RemoveAt(0);
            return data;
        }
        return new LogDatabaseThread();
    }
    #endregion

    protected override void OnDestroy() {
        if (allObjectPools != null) {
            foreach (KeyValuePair<string,EZObjectPool> pool in allObjectPools) {
                pool.Value.ClearPool();
            }
            allObjectPools.Clear();
            allObjectPools = null;
        }
        goapNodesPool?.Clear();
        goapNodesPool = null;
        opinionDataPool?.Clear();
        opinionDataPool = null;
        traitRemoveSchedulePool?.Clear();
        traitRemoveSchedulePool = null;
        combatDataPool?.Clear();
        combatDataPool = null;
        _interruptPool?.Clear();
        _interruptPool = null;
        _goapThreadPool?.Clear();
        _goapThreadPool = null;
        _partyPool?.Clear();
        _partyPool = null;
        _logDatabaseThreadPool?.Clear();
        _logDatabaseThreadPool = null;
        base.OnDestroy();
        Instance = null;
    }
}
