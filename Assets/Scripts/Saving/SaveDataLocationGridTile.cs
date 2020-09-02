﻿using System;
using System.Collections.Generic;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
public class SaveDataLocationGridTile : SaveData<LocationGridTile> {
    public string persistentID;
    public Vector3Save localPlace; //this is the id
    public Vector3Save worldLocation;
    public Vector3Save centeredWorldLocation;
    public Vector3Save localLocation;
    public Vector3Save centeredLocalLocation;
    public LocationGridTile.Tile_Type tileType;
    public LocationGridTile.Tile_State tileState;
    public string genericTileObjectID;
    public bool hasLandmine;
    public bool hasFreezingTrap;
    public bool hasSnareTrap;
    public List<RACE> freezingTrapExclusions;

    //tilemap assets
    public string groundTileMapAssetName;
    public string wallTileMapAssetName;
    public float floorSample;

    public override void Save(LocationGridTile gridTile) {
        persistentID = gridTile.persistentID;
        localPlace = new Vector3Save(gridTile.localPlace);
        worldLocation = gridTile.worldLocation;
        centeredWorldLocation = gridTile.centeredWorldLocation;
        localLocation = gridTile.localLocation;
        centeredLocalLocation = gridTile.centeredLocalLocation;
        tileType = gridTile.tileType;
        tileState = gridTile.tileState;
        hasLandmine = gridTile.hasLandmine;
        hasFreezingTrap = gridTile.hasFreezingTrap;
        hasSnareTrap = gridTile.hasSnareTrap;
        freezingTrapExclusions = gridTile.freezingTrapExclusions;
        
        genericTileObjectID = gridTile.genericTileObject.persistentID;

        //tilemap assets
        groundTileMapAssetName = gridTile.parentMap.groundTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
        wallTileMapAssetName = gridTile.parentMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;

        floorSample = gridTile.floorSample;
    }

    public LocationGridTile InitialLoad(Tilemap tilemap, InnerTileMap parentAreaMap, SaveDataCurrentProgress saveData) {
        LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap);
        tile.SetFloorSample(floorSample);
        SaveDataTileObject saveDataTileObject = saveData.GetFromSaveHub<SaveDataTileObject>(OBJECT_TYPE.Tile_Object, genericTileObjectID);
        TileObject loadedObject = saveDataTileObject.Load();
        GenericTileObject genericTileObject = loadedObject as GenericTileObject;
        Assert.IsNotNull(genericTileObject);
        genericTileObject.SetTileOwner(tile);
        genericTileObject.ManualInitializeLoad(tile, saveDataTileObject);
        tile.LoadGenericTileObject(genericTileObject);

        if (hasLandmine) {
            tile.SetHasLandmine(true);
        }
        if (hasFreezingTrap) {
            tile.SetHasFreezingTrap(true, freezingTrapExclusions?.ToArray());
        }
        if (hasSnareTrap) {
            tile.SetHasSnareTrap(true);
        }
        return tile;
    }

    // public void LoadTraits() {
        // for (int i = 0; i < traits.Count; i++) {
        //     Character responsibleCharacter = null;
        //     Trait trait = traits[i].Load(ref responsibleCharacter);
        //     loadedGridTile.genericTileObject.traitContainer.AddTrait(loadedGridTile.genericTileObject, trait, responsibleCharacter);
        // }
    // }
}
