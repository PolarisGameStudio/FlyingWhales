﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public interface IPointOfInterest : ITraitable {
    new string name { get; }
    int id { get; } //Be careful with how you handle this since this can duplicate depending on its poiType
    POINT_OF_INTEREST_TYPE poiType { get; }
    POI_STATE state { get; }
    Area specificLocation { get; }
    new LocationGridTile gridTileLocation { get; }
    List<INTERACTION_TYPE> advertisedActions { get; }
    List<JobQueueItem> allJobsTargettingThis { get; }
    Faction factionOwner { get; }
    bool isDisabledByPlayer { get; }
    Vector3 worldPosition { get; }
    bool isDead { get; }
    void SetGridTileLocation(LocationGridTile tile);
    void AddAdvertisedAction(INTERACTION_TYPE actionType);
    void RemoveAdvertisedAction(INTERACTION_TYPE actionType);
    void AddJobTargettingThis(JobQueueItem job);
    void SetPOIState(POI_STATE state);
    void SetIsDisabledByPlayer(bool state);
    bool HasJobTargettingThis(params JOB_TYPE[] jobType);
    bool IsAvailable();
    bool RemoveJobTargettingThis(JobQueueItem job);
    LocationGridTile GetNearestUnoccupiedTileFromThis();
    GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost);
    bool CanAdvertiseActionToActor(Character actor, GoapAction action, Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost);
    bool IsValidCombatTarget();
    bool IsStillConsideredPartOfAwarenessByCharacter(Character character);
    void OnPlacePOI();
    void OnDestroyPOI();
}

/// <summary>
/// Helper struct to contain data of generic POI's for saving and loading.
/// Usage Example:
///  - When a class has a list of poi's that it needs to save/load use this. (List<IPointOfInterest>)
/// </summary>
[System.Serializable]
public class POIData {
    public int poiID;
    public int areaID; //area location
    public POINT_OF_INTEREST_TYPE poiType;
    public TILE_OBJECT_TYPE tileObjectType; //The type of tile object that this is, should only be used if poi type is TILE_OBJECT
    public SPECIAL_TOKEN specialTokenType; //The type of item that this is, should only be used if poi type is ITEM

    public Vector3 genericTileObjectPlace; //used for generic tile objects, use this instead of id. NOTE: Generic Tile objects must ALWAYS have an areaID

    public POIData() { }

    public POIData(IPointOfInterest poi) {
        poiID = poi.id;
        if (poi.gridTileLocation == null) {
            areaID = -1;
        } else {
            areaID = poi.gridTileLocation.parentAreaMap.area.id;
        }
        poiType = poi.poiType;
        tileObjectType = TILE_OBJECT_TYPE.NONE;
        specialTokenType = default(SPECIAL_TOKEN);
        genericTileObjectPlace = Vector3.zero;

        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            tileObjectType = (poi as TileObject).tileObjectType;
            if (tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                genericTileObjectPlace = poi.gridTileLocation.localPlace;
            }
        } else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            specialTokenType = (poi as SpecialToken).specialTokenType;
        }
    }

    public override string ToString() {
        string name = poiType.ToString() + " " + poiID + ".";
        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            name += " Tile Object Type: " + tileObjectType.ToString();
        } else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
            name += " Item Type: " + specialTokenType.ToString();
        }
        return name;
    }
}