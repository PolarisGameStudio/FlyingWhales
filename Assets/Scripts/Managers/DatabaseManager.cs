﻿using System;
using System.Collections;
using System.Collections.Generic;
using Databases;
using Databases.SQLDatabase;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DatabaseManager : MonoBehaviour {
    public static DatabaseManager Instance;

    public HexTileDatabase hexTileDatabase { get; private set; }
    public RegionDatabase regionDatabase { get; private set; }
    public CharacterDatabase characterDatabase { get; private set; }
    public FactionDatabase factionDatabase { get; private set; }
    public TileObjectDatabase tileObjectDatabase { get; private set; }
    public LocationGridTileDatabase locationGridTileDatabase { get; private set; }
    public SettlementDatabase settlementDatabase { get; private set; }
    public LocationStructureDatabase structureDatabase { get; private set; }
    public TraitDatabase traitDatabase { get; private set; }
    public BurningSourceDatabase burningSourceDatabase { get; private set; }
    public JobDatabase jobDatabase { get; private set; }
    public FamilyTreeDatabase familyTreeDatabase { get; private set; }

    //These databases are only used when loading from a saved game, and therefore must be cleared out when loading is complete to save memory
    public ActionDatabase actionDatabase { get; private set; }
    public InterruptDatabase interruptDatabase { get; private set; }
    public LogDatabase logDatabase { get; private set; }
    public PartyDatabase partyDatabase { get; private set; }
    public CrimeDatabase crimeDatabase { get; private set; }
    
    //SQL Databases
    public RuinarchSQLDatabase mainSQLDatabase { get; private set; } 
    
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        } else {
            Destroy(gameObject);
        }
    }

    //Use this for initialization
    public void Initialize() {
        //Called in InitializeDataBeforeWorldCreation
        hexTileDatabase = new HexTileDatabase();
        regionDatabase = new RegionDatabase();
        characterDatabase = new CharacterDatabase();
        factionDatabase = new FactionDatabase();
        tileObjectDatabase = new TileObjectDatabase();
        locationGridTileDatabase = new LocationGridTileDatabase();
        settlementDatabase = new SettlementDatabase();
        structureDatabase = new LocationStructureDatabase();
        traitDatabase = new TraitDatabase();
        burningSourceDatabase = new BurningSourceDatabase();
        jobDatabase = new JobDatabase();
        familyTreeDatabase = new FamilyTreeDatabase();
        actionDatabase = new ActionDatabase();
        interruptDatabase = new InterruptDatabase();
        logDatabase = new LogDatabase();
        partyDatabase = new PartyDatabase();
        crimeDatabase = new CrimeDatabase();
        mainSQLDatabase = new RuinarchSQLDatabase();
    }

    #region Query
    public object GetObjectFromDatabase(System.Type type, string persistentID) {
        if (type == typeof(Character) || type.IsSubclassOf(typeof(Character))) {
            return characterDatabase.GetCharacterByPersistentID(persistentID);
        } else if (type == typeof(TileObject) || type.IsSubclassOf(typeof(TileObject))) {
            return tileObjectDatabase.GetTileObjectByPersistentID(persistentID);
        } else if (type == typeof(LocationStructure) || type.IsSubclassOf(typeof(LocationStructure))) {
            return structureDatabase.GetStructureByPersistentID(persistentID);
        } else if (type == typeof(Region)) {
            return regionDatabase.GetRegionByPersistentID(persistentID);
        } else if (type == typeof(BaseSettlement) || type.IsSubclassOf(typeof(BaseSettlement))) {
            return settlementDatabase.GetSettlementByPersistentID(persistentID);
        } else if (type == typeof(Faction)) {
            return factionDatabase.GetFactionBasedOnPersistentID(persistentID);
        }
        return null;
    }
    #endregion

    public void ClearVolatileDatabases() {
        actionDatabase.allActions.Clear();
        interruptDatabase.allInterrupts.Clear();
        logDatabase.allLogs.Clear();
        partyDatabase.allParties.Clear();
        crimeDatabase.allCrimes.Clear();
        locationGridTileDatabase.tileByGUID.Clear();
        locationGridTileDatabase.LocationGridTiles.Clear();
        System.GC.Collect();
    }

    private void OnSceneUnloaded(Scene unloaded) {
        Debug.Log($"Scene {unloaded.name} was unloaded.");
        if (unloaded.name == "Game") {
            //TODO: Dispose of old databases.
            DisposeDatabases();
        }
    }

    private void DisposeDatabases() {
        mainSQLDatabase?.Dispose();
    }
    private void OnDestroy() {
        DisposeDatabases();
    }
}
