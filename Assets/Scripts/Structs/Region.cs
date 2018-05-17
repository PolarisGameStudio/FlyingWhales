﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathFind;
using System;

public class Region : IHasNeighbours<Region> {
    private int _id;
    private string _name;
    private HexTile _centerOfMass;
    private List<HexTile> _tilesInRegion; //This also includes the center of mass
    private List<HexTile> _outerGridTilesInRegion;
    //private Color regionColor;
    private List<Region> _adjacentRegions;
    private List<Region> _adjacentRegionsViaRoad;
    private List<HexTile> _tilesWithMaterials; //The tiles inside the region that have materials

    //private Color defaultBorderColor = new Color(94f / 255f, 94f / 255f, 94f / 255f, 255f / 255f);

    //Landmarks
    private List<BaseLandmark> _landmarks; //This contains all the landmarks in the region, except for it's city
                                           //private List<BaseLandmark> _allLandmarks; //This contains all the landmarks in the region

    private List<HexTile> _outerTiles;
    private List<SpriteRenderer> regionBorderLines;

    //Roads
    private List<HexTile> _roadTilesInRegion;

    //Ownership
    private Faction _owner;

    //Islands
    private Dictionary<HexTile, RegionIsland> _islands;
    private RegionIsland _mainIsland;

    #region getters/sertters
    internal int id {
        get { return this._id; }
    }
    internal string name {
        get { return _name; }
    }
    internal HexTile centerOfMass {
        get { return _centerOfMass; }
    }
    internal List<HexTile> tilesInRegion {
        get { return _tilesInRegion; }
    }
    internal List<HexTile> outerGridTilesInRegion {
        get { return _outerGridTilesInRegion; }
    }
    internal List<Region> adjacentRegions {
        get { return _adjacentRegions; }
    }
    internal List<Region> adjacentRegionsViaRoad {
        get { return _adjacentRegionsViaRoad; }
    }
    internal List<HexTile> outerTiles {
        get { return this._outerTiles; }
    }
    internal List<BaseLandmark> landmarks {
        get { return _landmarks; }
    }
    //internal List<BaseLandmark> allLandmarks {
    //	get { return _landmarks; }
    //}
    internal List<HexTile> roadTilesInRegion {
        get { return _roadTilesInRegion; }
    }
    internal Faction owner {
        get { return _owner; } //The faction that owns this region
    }
    internal bool isOwned {
        get { return owner != null; }
    }
    internal BaseLandmark mainLandmark {
        get { return _centerOfMass.landmarkOnTile; }
    }
    internal List<HexTile> tilesWithMaterials {
        get { return _tilesWithMaterials; }
    }
    internal List<ECS.Character> charactersInRegion {
        get { return GetCharactersInRegion(); }
    }
    internal int numOfCharactersInLandmarks {
        get { return _landmarks.Sum(x => x.charactersAtLocation.Sum(y => y.numOfCharacters)); }
    }
    #endregion

    public List<Region> ValidTiles {
        get {
            return new List<Region>(adjacentRegionsViaRoad);
        }
    }

    public Region(HexTile centerOfMass) {
        _id = Utilities.SetID(this);
        _name = RandomNameGenerator.Instance.GetRegionName();
        SetCenterOfMass(centerOfMass);
        _tilesInRegion = new List<HexTile>();
        _outerGridTilesInRegion = new List<HexTile>();
        _adjacentRegionsViaRoad = new List<Region>();
        _roadTilesInRegion = new List<HexTile>();
        _landmarks = new List<BaseLandmark>();
        _tilesWithMaterials = new List<HexTile>();
        AddTile(_centerOfMass);
        //regionColor = UnityEngine.Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
    }

    #region Center Of Mass Functions
    internal void ReComputeCenterOfMass() {
        int maxXCoordinate = _tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = _tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = _tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = _tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        if (GridMap.Instance.width - 2 >= midPointX) {
            midPointX -= 2;
        }
        if (GridMap.Instance.height - 2 >= midPointY) {
            midPointY -= 2;
        }
        if (midPointX >= 2) {
            midPointX += 2;
        }
        if (midPointY >= 2) {
            midPointY += 2;
        }
        try {
            HexTile newCenterOfMass = GridMap.Instance.map[midPointX, midPointY];
            SetCenterOfMass(newCenterOfMass);
        } catch {
            throw new Exception("Cannot Recompute center of mass for " + this.name + ". Current center is " + centerOfMass.name + ". Computed new center is " + midPointX.ToString() + ", " + midPointY.ToString());
        }
        
    }
    internal void RevalidateCenterOfMass() {
        if (_centerOfMass.elevationType != ELEVATION.PLAIN) {
            SetCenterOfMass(_tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN)
                .OrderBy(x => x.GetDistanceTo(_centerOfMass)).FirstOrDefault());
            if (_centerOfMass == null) {
                throw new System.Exception("center of mass is null!");
            }
        }
    }
    internal void SetCenterOfMass(HexTile newCenter) {
        if (_centerOfMass != null) {
            //_centerOfMass.RemoveLandmarkOnTile();
            _centerOfMass.isHabitable = false;
            //_centerOfMass.emptyCityGO.SetActive(false);
        }
        _centerOfMass = newCenter;
        _centerOfMass.isHabitable = true;
        //_centerOfMass.emptyCityGO.SetActive (true);
        //_centerOfMass.CreateLandmarkOfType(BASE_LANDMARK_TYPE.SETTLEMENT, LANDMARK_TYPE.TOWN);
    }
    #endregion

    #region Ownership
    public void SetOwner(Faction owner) {
        _owner = owner;
    }
    #endregion

    #region Adjacency Functions
    /*
     * <summary>
     * Check For Adjacent regions, this will populate the
     * _outerTiles and _adjacentRegions Lists. This is only called at the
     * start of the game, after all the regions have been determined. This will
     * also populate regionBorderLines.
     * </summary>
     * */
    internal void CheckForAdjacency() {
        _outerTiles = new List<HexTile>();
        _adjacentRegions = new List<Region>();
        regionBorderLines = new List<SpriteRenderer>();
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            HexTile currTile = _tilesInRegion[i];
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile currNeighbour = currTile.AllNeighbours[j];
                if (currNeighbour.region != currTile.region) {
                    //Load Border For currTile
                    HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour);
                    SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate);
                    AddRegionBorderLineSprite(border);

                    if (!_outerTiles.Contains(currTile)) {
                        //currTile has a neighbour that is part of a different region, this means it is an outer tile.
                        _outerTiles.Add(currTile);
                    }
                    //if (currNeighbour.region != null) {
                    if (!_adjacentRegions.Contains(currNeighbour.region)) {
                        if (currNeighbour.region == null) {
                            throw new System.Exception("REGION IS NULL! " + currNeighbour.name);
                        } else {
                            _adjacentRegions.Add(currNeighbour.region);
                        }
                    }
                    //}
                }
            }
        }
    }
    public void CheckForRoadAdjacency() {
        for (int i = 0; i < adjacentRegions.Count; i++) {
            Region otherRegion = adjacentRegions[i];
            if (HasConnectionToRegion(otherRegion, true)) {
                _adjacentRegionsViaRoad.Add(otherRegion);
            }
        }
    }
    #endregion

    #region Tile Functions
    internal List<HexTile> GetTilesAdjacentOnlyTo(Region otherRegion) {
        List<HexTile> adjacentTiles = new List<HexTile>();
        for (int i = 0; i < _outerTiles.Count; i++) {
            HexTile currTile = _outerTiles[i];
            if(currTile.roadType != ROAD_TYPE.MAJOR && currTile.MajorRoadTiles.Count <= 0 && currTile.AllNeighbours.Where(x => x.region.id == otherRegion.id).Any()) {
                bool isOnlyAdjacentToOtherRegion = true;
                for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                    HexTile currNeighbour = currTile.AllNeighbours[j];
                    if (currNeighbour.region.id != otherRegion.id && currNeighbour.region.id != this.id) {
                        isOnlyAdjacentToOtherRegion = false;
                        break;
                    }
                }
                if (isOnlyAdjacentToOtherRegion) {
                    adjacentTiles.Add(currTile);
                }
            }
        }
        return adjacentTiles;
    }
    internal void AddTile(HexTile tile) {
        if (!_tilesInRegion.Contains(tile)) {
            _tilesInRegion.Add(tile);
            tile.SetRegion(this);
        }
    }
    internal void AddOuterGridTile(HexTile tile) {
        if (!_outerGridTilesInRegion.Contains(tile)) {
            _outerGridTilesInRegion.Add(tile);
            tile.SetRegion(this);
        }
    }
    internal void ResetTilesInRegion() {
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            HexTile tile = _tilesInRegion[i];
            if (tile.region == this) {
                tile.SetRegion(null);
            }
        }
        _tilesInRegion.Clear();
    }
    /*
     Highlight all tiles in the region.
         */
    internal void HighlightRegionTiles(Color highlightColor, float highlightAlpha) {
        Color color = highlightColor;
        color.a = highlightAlpha;
        Color fullColor = highlightColor;
        fullColor.a = 255f/255f;
        for (int i = 0; i < this.tilesInRegion.Count; i++) {
            HexTile currentTile = this.tilesInRegion[i];
            //currentTile.kingdomColorSprite.color = color;
            //currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(fullColor);
        }
        for (int i = 0; i < this.outerGridTilesInRegion.Count; i++) {
            HexTile currentTile = this.outerGridTilesInRegion[i];
            //currentTile.kingdomColorSprite.color = color;
            //currentTile.kingdomColorSprite.gameObject.SetActive(true);
            currentTile.SetMinimapTileColor(fullColor);
        }
    }
    internal void ReColorBorderTiles(Color color) {
        Color fullColor = color;
        fullColor.a = 255f / 255f;
        for (int i = 0; i < regionBorderLines.Count; i++) {
            regionBorderLines[i].color = fullColor;
        }
    }
    internal void AddRegionBorderLineSprite(SpriteRenderer sprite) {
        if (!regionBorderLines.Contains(sprite)) {
            regionBorderLines.Add(sprite);
        }
    }
    #endregion

    #region Materials
    public void AddTileWithMaterial(HexTile tile) {
        if (!_tilesWithMaterials.Contains(tile)) {
            _tilesWithMaterials.Add(tile);
        }
    }
    public void RemoveTileWithMaterial(HexTile tile) {
        _tilesWithMaterials.Remove(tile);
    }
    internal int GetActivelyHarvestedMaterialsOfType(MATERIAL material) {
        int count = 0;
        for (int i = 0; i < _landmarks.Count; i++) {
            BaseLandmark currLandmark = _landmarks[i];
            if (currLandmark is ResourceLandmark) {
                ResourceLandmark resourceLandmark = currLandmark as ResourceLandmark;
                //check if the landmark has the material specified, and already has a structure built on it.
                if (resourceLandmark.materialOnLandmark == material && resourceLandmark.tileLocation.HasStructure()) {
                    count++;
                }
            }
        }
        return count;
    }
    #endregion

    #region Landmark Functions
    internal void AddLandmarkToRegion(BaseLandmark landmark) {
        if (!_landmarks.Contains(landmark)) {
            _landmarks.Add(landmark);
        }
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return true;
            }
        }
        return false;
    }
    public List<BaseLandmark> GetLandmarksOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> landmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                landmarksOfType.Add(currLandmark);
            }
        }
        return landmarksOfType;
    }
    public List<BaseLandmark> GetLandmarksOfType(BASE_LANDMARK_TYPE baseLandmarkType) {
        List<BaseLandmark> landmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (LandmarkManager.Instance.GetLandmarkData(currLandmark.specificLandmarkType).baseLandmarkType == baseLandmarkType) {
                landmarksOfType.Add(currLandmark);
            }
        }
        return landmarksOfType;
    }
    #endregion

    #region Road Functions
    /*
     Check if there are any landmarks in this region, 
     that are connected to any landmarks in another region.
     Also check landmarks in this region that has connections, and check
     if any of them are already connected to the other region
         */
    internal bool HasConnectionToRegion(Region otherRegion, bool directOnly = false) {
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            if (directOnly) {
                if (currLandmark.IsConnectedTo(otherRegion)) {
                    return true;
                }
            } else {
                if (currLandmark.IsConnectedTo(otherRegion) || currLandmark.IsIndirectlyConnectedTo(otherRegion)) {
                    return true;
                }
            }
        }
        return false;
    }
    internal BaseLandmark GetLandmarkNearestTo(Region otherRegion) {
        int nearestDistance = 9999;
        BaseLandmark nearestLandmark = null;
        for (int i = 0; i < landmarks.Count; i++) {
            BaseLandmark currLandmark = landmarks[i];
            for (int j = 0; j < otherRegion.landmarks.Count; j++) {
                BaseLandmark otherLandmark = otherRegion.landmarks[j];
                List<HexTile> path = PathGenerator.Instance.GetPath(currLandmark.tileLocation, otherLandmark.tileLocation, PATHFINDING_MODE.LANDMARK_CONNECTION);
                if (path != null) { //check if there is a path between the 2 landmarks
                    if (path.Count < nearestDistance) {
                        nearestDistance = path.Count;
                        nearestLandmark = currLandmark;
                    }
                }
            }
        }
        return nearestLandmark;
    }
    internal void AddTileAsRoad(HexTile tile) {
        if (!_roadTilesInRegion.Contains(tile)) {
            _roadTilesInRegion.Add(tile);
        }
    }
    internal void RemoveTileAsRoad(HexTile tile) {
        _roadTilesInRegion.Remove(tile);
    }
    #endregion

    #region Utilities
    private List<ECS.Character> GetCharactersInRegion() {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < tilesInRegion.Count; i++) {
            HexTile currTile = tilesInRegion[i];
            characters.AddRange(currTile.charactersAtLocation.Select(x => x.mainCharacter));
            if (currTile.landmarkOnTile != null) {
                characters.AddRange(currTile.landmarkOnTile.charactersAtLocation.Select(x => x.mainCharacter));
            }
        }
        return characters;
    }
    internal void LogPassableTiles() {
        Dictionary<PASSABLE_TYPE, int> passableTiles = new Dictionary<PASSABLE_TYPE, int>();
        PASSABLE_TYPE[] types = Utilities.GetEnumValues<PASSABLE_TYPE>();
        for (int i = 0; i < types.Length; i++) {
            passableTiles.Add(types[i], 0);
        }

        for (int i = 0; i < tilesInRegion.Count; i++) {
            HexTile currTile = tilesInRegion[i];
            passableTiles[currTile.passableType]++;
        }
        string text = this._name + " tiles summary (" + tilesInRegion.Count.ToString() + "): ";
        foreach (KeyValuePair<PASSABLE_TYPE, int> kvp in passableTiles) {
            text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
        }
        Debug.Log(text, this.centerOfMass);
    }
    #endregion

    #region Islands
    public void DetermineRegionIslands() {
        List<HexTile> passableTilesInRegion = tilesInRegion.Where(x => x.isPassable).ToList();
        _islands = new Dictionary<HexTile, RegionIsland>();
        for (int i = 0; i < passableTilesInRegion.Count; i++) {
            HexTile currTile = passableTilesInRegion[i];
            RegionIsland island = new RegionIsland(currTile);
            _islands.Add(currTile, island);
        }

        Queue<HexTile> tileQueue = new Queue<HexTile>();
        while (passableTilesInRegion.Count != 0) {
            HexTile currTile;
            if (tileQueue.Count <= 0) {
                currTile = passableTilesInRegion[UnityEngine.Random.Range(0, passableTilesInRegion.Count)];
            } else {
                currTile = tileQueue.Dequeue();
            }
            RegionIsland islandOfCurrTile = _islands[currTile];
            List<HexTile> neighbours = currTile.AllNeighbours;
            for (int i = 0; i < neighbours.Count; i++) {
                HexTile currNeighbour = neighbours[i];
                if (currNeighbour.isPassable && passableTilesInRegion.Contains(currNeighbour)) {
                    RegionIsland islandOfNeighbour = _islands[currNeighbour];
                    MergeIslands(islandOfCurrTile, islandOfNeighbour, _islands);
                    tileQueue.Enqueue(currNeighbour);
                }
            }
            passableTilesInRegion.Remove(currTile);
        }

        List<RegionIsland> allIslands = new List<RegionIsland>();
        foreach (KeyValuePair<HexTile, RegionIsland> kvp in _islands) {
            if (!allIslands.Contains(kvp.Value)) {
                allIslands.Add(kvp.Value);
            }
        }
        ConnectIslands(allIslands, _islands);
        //allIslands = allIslands.OrderByDescending(x => x.tilesInIsland.Count).ToList();
        //_mainIsland = allIslands[0];
    }
    private RegionIsland MergeIslands(RegionIsland island1, RegionIsland island2, Dictionary<HexTile, RegionIsland> islands) {
        if (island1 == island2) {
            return island1;
        }
        island1.AddTileToIsland(island2.tilesInIsland);
        for (int i = 0; i < island2.tilesInIsland.Count; i++) {
            HexTile currTile = island2.tilesInIsland[i];
            islands[currTile] = island1;
        }
        island2.ClearIsland();
        return island1;
    }
    //public bool IsPartOfMainIsland(HexTile tile) {
    //    if (_islands[tile] == _mainIsland) {
    //        return true;
    //    }
    //    return false;
    //}
    private void ConnectIslands(List<RegionIsland> islands, Dictionary<HexTile, RegionIsland> islandsDict) {
        for (int i = 0; i < islands.Count; i++) {
            RegionIsland currIsland = islands[i];
            if (currIsland.tilesInIsland.Count > 0) {
                ConnectToNearestIsland(currIsland, islandsDict, islands);
            }
        }
    }
    private void ConnectToNearestIsland(RegionIsland originIsland, Dictionary<HexTile, RegionIsland> islandsDict, List<RegionIsland> islands) {
        int nearestDistance = 9999;
        RegionIsland nearestIsland = null;
        List<HexTile> nearestPath = null;

        for (int i = 0; i < islands.Count; i++) {
            RegionIsland otherIsland = islands[i];
            if (otherIsland != originIsland && otherIsland.tilesInIsland.Count > 0) {
                if (!AreIslandsConnected(originIsland, otherIsland)) {
                    List<HexTile> path = PathGenerator.Instance.GetPath(originIsland.mainTile, otherIsland.mainTile, PATHFINDING_MODE.REGION_ISLAND_CONNECTION, this);
                    if (path != null && path.Count < nearestDistance) {
                        nearestDistance = path.Count;
                        nearestPath = path;
                        nearestIsland = otherIsland;
                    }
                }
            }
        }

        if (nearestPath != null) {
            MergeIslands(originIsland, nearestIsland, islandsDict);
            List<HexTile> tilesToFlatten = new List<HexTile>();
            for (int i = 0; i < nearestPath.Count; i++) {
                HexTile currTile = nearestPath[i];
                if (!originIsland.tilesInIsland.Contains(currTile)) {
                    //only flattern tiles that is not part of the island, meaning the unpassable tiles in between the regions islands
                    tilesToFlatten.Add(currTile);
                }
            }
            //islands.Remove(nearestIsland);
            FlattenTiles(tilesToFlatten);
        }
    }
    private bool AreIslandsConnected(RegionIsland island1, RegionIsland island2) {
        HexTile randomTile1 = island1.tilesInIsland[UnityEngine.Random.Range(0, island1.tilesInIsland.Count)];
        HexTile randomTile2 = island2.tilesInIsland[UnityEngine.Random.Range(0, island2.tilesInIsland.Count)];

        return PathGenerator.Instance.GetPath(randomTile1, randomTile2, PATHFINDING_MODE.PASSABLE_REGION_ONLY, this) != null;
    }
    private void FlattenTiles(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.isPassable) {
                continue;
            }
            currTile.SetElevation(ELEVATION.PLAIN);
            currTile.SetPassableState(true);
            currTile.DeterminePassableType();
            currTile.PassableNeighbours.ForEach(x => x.DeterminePassableType());
        }
    }
    #endregion

    #region Corruption
    //public void LandmarkStartedCorruption(BaseLandmark corruptedLandmark) {
    //    for (int i = 0; i < landmarks.Count; i++) {
    //        BaseLandmark landmark = landmarks[i];
    //        if(corruptedLandmark.id != landmark.id && !landmark.tileLocation.isCorrupted) {
    //            landmark.ALandmarkHasStartedCorruption(corruptedLandmark);
    //        }
    //    }
    //}
    //public void LandmarkStoppedCorruption(BaseLandmark corruptedLandmark) {
    //    for (int i = 0; i < landmarks.Count; i++) {
    //        BaseLandmark landmark = landmarks[i];
    //        if (corruptedLandmark.id != landmark.id && !landmark.tileLocation.isCorrupted) {
    //            landmark.ALandmarkHasStoppedCorruption(corruptedLandmark);
    //        }
    //    }
    //}
    #endregion
}

public class RegionIsland {
    private HexTile _mainTile;
    private List<HexTile> _tilesInIsland;
    //private List<HexTile> _outerTiles;

    public HexTile mainTile {
        get { return _mainTile; }
    }
    public List<HexTile> tilesInIsland {
        get { return _tilesInIsland; }
    }
    //public List<HexTile> outerTiles {
    //    get { return _outerTiles; }
    //}

    public RegionIsland(HexTile tile) {
        _mainTile = tile;
        _tilesInIsland = new List<HexTile>();
        AddTileToIsland(tile);
    }

    public void AddTileToIsland(HexTile tile) {
        if (!_tilesInIsland.Contains(tile)) {
            _tilesInIsland.Add(tile);
        }
    }
    public void AddTileToIsland(List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            AddTileToIsland(tiles[i]);
        }
    }
    public void RemoveTileFromIsland(HexTile tile) {
        _tilesInIsland.Remove(tile);
    }
    public void ClearIsland() {
        _tilesInIsland.Clear();
    }
}
