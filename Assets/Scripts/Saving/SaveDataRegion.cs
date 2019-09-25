﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class SaveDataRegion {
    public int id;
    public string name;
    public List<int> tileIDs;
    public int coreTileID;
    public ColorSave regionColor;
    public List<int> connectionsTileIDs;
    public int previousOwnerID;
    public List<int> factionsHereIDs;

    public List<int> charactersAtLocationIDs;
    public SaveDataWorldObject worldObj;
    public bool hasWorldObject;
    public WORLD_EVENT activeEvent;
    public int eventSpawnedByCharacterID;
    public bool hasEventIconGO;
    public SaveDataWorldEventData eventData;

    //public int invadingMinionID;
    public DemonicLandmarkBuildingData demonicBuildingData;
    public DemonicLandmarkInvasionData demonicInvasionData;


    //public Minion assignedMinion - NOTE: Minion assigned for the region is already saved and loaded in SaveDataMinion. It is not saved and loaded here because there will be redundancy
    //SEE SaveDataMinion

    public void Save(Region region) {
        id = region.id;
        name = region.name;

        tileIDs = new List<int>();
        for (int i = 0; i < region.tiles.Count; i++) {
            tileIDs.Add(region.tiles[i].id);
        }

        coreTileID = region.coreTile.id;
        //ticksInInvasion = region.ticksInInvasion;
        regionColor = region.regionColor;

        demonicBuildingData = region.demonicBuildingData;
        demonicInvasionData = region.demonicInvasionData;

        connectionsTileIDs = new List<int>();
        for (int i = 0; i < region.connections.Count; i++) {
            connectionsTileIDs.Add(region.connections[i].coreTile.id);
        }

        charactersAtLocationIDs = new List<int>();
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            charactersAtLocationIDs.Add(region.charactersAtLocation[i].id);
        }

        if(region.previousOwner != null) {
            previousOwnerID = region.previousOwner.id;
        } else {
            previousOwnerID = -1;
        }

        factionsHereIDs = new List<int>();
        for (int i = 0; i < region.factionsHere.Count; i++) {
            factionsHereIDs.Add(region.factionsHere[i].id);
        }

        if (region.activeEvent != null) {
            activeEvent = region.activeEvent.eventType;
            eventSpawnedByCharacterID = region.eventSpawnedBy.id;

            var typeName = "SaveData" + region.eventData.GetType().ToString();
            eventData = System.Activator.CreateInstance(System.Type.GetType(typeName)) as SaveDataWorldEventData;
            eventData.Save(region.eventData);
        } else {
            activeEvent = WORLD_EVENT.NONE;
        }
        hasEventIconGO = region.eventIconGO != null;

        if (region.worldObj != null) {
            hasWorldObject = true;

            worldObj = new SaveDataWorldObject();
            worldObj.Save(region.worldObj);
            //if (region.worldObj is Artifact) {
            //    worldObj = new SaveDataArtifact();
            //} else if (region.worldObj is Summon) {
            //    worldObj = new SaveDataSummon();
            //} else {
            //    var typeName = "SaveData" + region.worldObj.GetType().ToString();
            //    worldObj = System.Activator.CreateInstance(System.Type.GetType(typeName)) as SaveDataWorldObject;
            //}
            //worldObj.Save(region.worldObj);
        } else {
            hasWorldObject = false;
        }

        //if (region.assignedMinion != null) {
        //    invadingMinionID = region.assignedMinion.character.id;
        //} else {
        //    invadingMinionID = -1;
        //}
    }

    public Region Load() {
        Region region = new Region(this);
        for (int i = 0; i < tileIDs.Count; i++) {
            region.AddTile(GridMap.Instance.hexTiles[tileIDs[i]]);
        }
        return region;
    }

    public void LoadRegionAdditionalData(Region region) {
        //Region region = GridMap.Instance.GetRegionByID(id);
        if(previousOwnerID != -1) {
            region.SetPreviousOwner(FactionManager.Instance.GetFactionBasedOnID(previousOwnerID));
        }

        for (int i = 0; i < factionsHereIDs.Count; i++) {
            region.AddFactionHere(FactionManager.Instance.GetFactionBasedOnID(factionsHereIDs[i]));
        }

        region.LoadBuildingStructure(this);
        region.LoadInvasion(this);
    }
    public void LoadRegionConnections(Region region) {
        for (int i = 0; i < connectionsTileIDs.Count; i++) {
            Region regionToConnect = GridMap.Instance.hexTiles[connectionsTileIDs[i]].region;
            if (!region.connections.Contains(regionToConnect)) {
                LandmarkManager.Instance.ConnectRegions(region, regionToConnect);
            }
        }
    }
    public void LoadRegionCharacters(Region region) {
        for (int i = 0; i < charactersAtLocationIDs.Count; i++) {
            region.LoadCharacterHere(CharacterManager.Instance.GetCharacterByID(charactersAtLocationIDs[i]));
        }
    }
    public void LoadActiveEventAndWorldObject(Region region) {
        region.LoadEventAndWorldObject(this);
    }
    //public Minion LoadInvadingMinion() {
    //    if(invadingMinionID != -1) {
    //        Minion minion = CharacterManager.Instance.GetCharacterByID(invadingMinionID).minion;
    //        return minion;
    //    }
    //    return null;
    //}
}
