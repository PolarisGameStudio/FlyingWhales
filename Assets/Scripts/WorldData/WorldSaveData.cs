﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldSaveData {
    public int width;
    public int height;
    public List<HexTileData> tilesData;
    public List<RegionSaveData> regionsData;
    public List<FactionSaveData> factionsData;
    public List<LandmarkSaveData> landmarksData;

    private Dictionary<int, HexTileData> tileDictionary;

    public WorldSaveData(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void OccupyTileData(List<HexTile> tiles) {
        tilesData = new List<HexTileData>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            tilesData.Add(currTile.data);
        }
    }
    public void OccupyRegionData(List<Region> regions) {
        regionsData = new List<RegionSaveData>();
        for (int i = 0; i < regions.Count; i++) {
            Region currRegion = regions[i];
            RegionSaveData regionData = new RegionSaveData(currRegion);
            regionsData.Add(regionData);
        }
    }
    public void OccupyFactionData(List<Faction> factions) {
        factionsData = new List<FactionSaveData>();
        for (int i = 0; i < factions.Count; i++) {
            Faction currFaction = factions[i];
            FactionSaveData factionData = new FactionSaveData(currFaction);
            factionsData.Add(factionData);
        }
    }
    public void OccupyLandmarksData(List<BaseLandmark> landmarks) {
        landmarksData = new List<LandmarkSaveData>();
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            LandmarkSaveData landmarkData = new LandmarkSaveData(currLandmark);
            landmarksData.Add(landmarkData);
        }
    }

    public void ConstructTileDictionary() {
        tileDictionary = new Dictionary<int, HexTileData>();
        for (int i = 0; i < tilesData.Count; i++) {
            tileDictionary.Add(tilesData[i].id, tilesData[i]);
        }
    }

    public HexTileData GetTileData(int tileID) {
        if (tileDictionary == null) {
            for (int i = 0; i < tilesData.Count; i++) {
                HexTileData currData = tilesData[i];
                if (currData.id == tileID) {
                    return currData;
                }
            }
        } else {
            return tileDictionary[tileID];
        }
        return null;
    }

    public RegionSaveData GetRegionData(int regionID) {
        for (int i = 0; i < regionsData.Count; i++) {
            RegionSaveData data = regionsData[i];
            if (data.regionID == regionID) {
                return data;
            }
        }
        return null;
    }
}