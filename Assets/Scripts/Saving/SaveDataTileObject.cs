﻿using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Traits;
using UnityEngine;

[System.Serializable]
public class SaveDataTileObject : SaveData<TileObject>, ISavableCounterpart {
    public string persistentID { get; set; }
    public int id;
    public string name;
    public TILE_OBJECT_TYPE tileObjectType;
    public string characterOwnerID;
    public int regionLocationID;
    public TileLocationSave tileLocationID;
    public bool isPreplaced;
    public int[] resourceValues; //food, wood, stone, metal
    // public Dictionary<RESOURCE, int> storedResources;
    public string isBeingCarriedByID;
    public POI_STATE poiState;
    public List<INTERACTION_TYPE> advertisedActions;
    public List<string> jobsTargetingThis;
    public List<string> existingJobsTargetingThis;
    public MAP_OBJECT_STATE mapObjectState;
    
    //hp
    public int currentHP;
    
    //visuals
    public string spriteName;
    public QuaternionSave rotation;
    
    //Traits
    public SaveDataTraitContainer saveDataTraitContainer;

    public SaveDataLogComponent logComponent;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Tile_Object;
    #endregion
    
    public override void Save(TileObject data) {
        persistentID = data.persistentID;
        id = data.id;
        name = data.name;
        tileObjectType = data.tileObjectType;
        characterOwnerID = data.characterOwner?.persistentID ?? string.Empty;
        if (data.gridTileLocation != null) {
            regionLocationID = data.gridTileLocation.parentMap.region.id;
            tileLocationID = new TileLocationSave(data.gridTileLocation);    
        } else {
            regionLocationID = -1;
            tileLocationID = new TileLocationSave();
        }
        isPreplaced = data.isPreplaced;
        poiState = data.state;
        
        advertisedActions = data.advertisedActions != null ? new List<INTERACTION_TYPE>(data.advertisedActions) : new List<INTERACTION_TYPE>();

        jobsTargetingThis = new List<string>();
        for (int i = 0; i < data.allJobsTargetingThis.Count; i++) {
            JobQueueItem jobQueueItem = data.allJobsTargetingThis[i];
            jobsTargetingThis.Add(jobQueueItem.persistentID);
        }
        
        existingJobsTargetingThis = new List<string>();
        for (int i = 0; i < data.allExistingJobsTargetingThis.Count; i++) {
            JobQueueItem jobQueueItem = data.allExistingJobsTargetingThis[i];
            existingJobsTargetingThis.Add(jobQueueItem.persistentID);
        }
        mapObjectState = data.mapObjectState;
        
        currentHP = data.currentHP;

        if (data.mapObjectVisual == null || data.mapObjectVisual.usedSprite == null) {
            spriteName = string.Empty;
            rotation = Quaternion.identity;
        } else {
            spriteName = data.mapObjectVisual.usedSprite.name;
            rotation = data.mapObjectVisual.rotation;
        }
        
        resourceValues = new int[data.storedResources.Count];
        int index = 0;
        foreach (var storedResource in data.storedResources) {
            resourceValues[index] = storedResource.Value;
            index++;
        }

        isBeingCarriedByID = data.isBeingCarriedBy != null ? data.isBeingCarriedBy.persistentID : string.Empty;
        
        saveDataTraitContainer = new SaveDataTraitContainer();
        saveDataTraitContainer.Save(data.traitContainer);

        logComponent = new SaveDataLogComponent();
        logComponent.Save(data.logComponent);
    }
    public override TileObject Load() {
        TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
        tileObject.Initialize(this);
        return tileObject;
    }
}