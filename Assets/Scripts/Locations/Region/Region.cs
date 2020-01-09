﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using PathFind;
using UnityEngine;
using Random = UnityEngine.Random;

public class Region : ILocation {

    private const float HoveredBorderAlpha = 255f / 255f;
    private const float UnhoveredBorderAlpha = 128f / 255f;

    public int id { get; private set; }
    public string name { get; private set; }
    public string description => GetDescription();
    public List<HexTile> tiles { get; private set; }
    public HexTile coreTile { get; private set; }
    public LOCATION_TYPE locationType => LOCATION_TYPE.EMPTY;
    public Color regionColor { get; private set; }
    public Minion assignedMinion { get; private set; }
    public List<Faction> factionsHere { get; private set; }
    public List<Character> residents { get; private set; }
    public DemonicLandmarkBuildingData demonicBuildingData { get; private set; }
    public DemonicLandmarkInvasionData demonicInvasionData { get; private set; }
    public GameObject eventIconGo { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public InnerTileMap innerMap => _regionInnerTileMap;
    public RegionTileObject regionTileObject { get; private set; }

    private RegionInnerTileMap _regionInnerTileMap; //inner map of the region, this should only be used if this region does not have an settlement. 
    private string _activeEventAfterEffectScheduleId;
    private List<SpriteRenderer> _borderSprites;
    private Dictionary<STRUCTURE_TYPE, List<LocationStructure>> _structures;
    public HexTile[,] hexTileMap { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; private set; }

    #region getter/setter
    public BaseLandmark mainLandmark => coreTile.landmarkOnTile;
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures => _structures;
    #endregion

    private Region() {
        charactersAtLocation = new List<Character>();
        new List<System.Action>();
        factionsHere = new List<Faction>();
        residents = new List<Character>();
        awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>();
    }
    public Region(HexTile coreTile) : this() {
        id = Utilities.SetID(this);
        name = RandomNameGenerator.Instance.GetRegionName();
        this.coreTile = coreTile;
        tiles = new List<HexTile>();
        AddTile(coreTile);
        if (id == 1) {
            regionColor = Color.blue;
        } else if (id == 2) {
            regionColor = Color.yellow;
        } else if (id == 3) {
            regionColor = Color.green;
        } else if (id == 4) {
            regionColor = Color.red;
        } else if (id == 5) {
            regionColor = Color.yellow;
        }
        // regionColor = Random.ColorHSV();
    }
    public Region(SaveDataRegion data) : this() {
        id = Utilities.SetID(this, data.id);
        name = data.name;
        coreTile = GridMap.Instance.normalHexTiles[data.coreTileID];
        tiles = new List<HexTile>();
        regionColor = data.regionColor;
    }

    public void SetName(string name) {
        this.name = name;
    }
    public void AddTile(HexTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            tile.SetRegion(this);
            // tile.spriteRenderer.color = regionColor;
        }
    }
    private void RemoveTile(HexTile tile) {
        if (tiles.Remove(tile)) {
            tile.SetRegion(null);
            // tile.spriteRenderer.color = Color.white;
        }
    }
    public void OnMainLandmarkChanged() {
        regionTileObject?.UpdateAdvertisements(this);
    }

    #region Utilities
    private string GetDescription() {
        if (coreTile.isCorrupted) {
            if (mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
                return "This region is empty. You may assign a minion to build a demonic landmark here.";
            }
        }
        return LandmarkManager.Instance.GetLandmarkData(mainLandmark.specificLandmarkType).description;
    }
    public void FinalizeData() {
        //outerTiles = GetOuterTiles();
        _borderSprites = GetOuterBorders();
        DetermineHexTileMap();
    }
    public void RedetermineCore() {
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int x = (minX + maxX) / 2;
        int y = (minY + maxY) / 2;

        coreTile = GridMap.Instance.map[x, y];

        // while (tiles.Contains(coreTile) == false) {
        //     x++;
        //     y++;
        //     coreTile = GridMap.Instance.map[x, y];
        // }
        
        //clear all tiles again after redetermining core
        List<HexTile> allTiles = new List<HexTile>(tiles);
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currTile = allTiles[i];
            if (currTile != coreTile) {
                RemoveTile(currTile);
            }
        }
        
    }
    /// <summary>
    /// Get the outer tiles of this region. NOTE: Made this into a getter instead of saving it in a variable, to save memory.
    /// </summary>
    /// <returns>List of outer tiles.</returns>
    private List<HexTile> GetOuterTiles() {
        List<HexTile> outerTiles = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            if (currTile.AllNeighbours.Count != 6 || currTile.HasNeighbourFromOtherRegion()) {
                outerTiles.Add(currTile);
            }
        }
        return outerTiles;
    }
    private List<SpriteRenderer> GetOuterBorders() {
        List<HexTile> outerTiles = GetOuterTiles();
        List<SpriteRenderer> borders = new List<SpriteRenderer>();
        HEXTILE_DIRECTION[] dirs = Utilities.GetEnumValues<HEXTILE_DIRECTION>();
        for (int i = 0; i < outerTiles.Count; i++) {
            HexTile currTile = outerTiles[i];
            for (int j = 0; j < dirs.Length; j++) {
                HEXTILE_DIRECTION dir = dirs[j];
                if (dir == HEXTILE_DIRECTION.NONE) { continue; }
                HexTile neighbour = currTile.GetNeighbour(dir);
                if (neighbour == null || neighbour.region != currTile.region) {
                    SpriteRenderer border = currTile.GetBorder(dir);
                    //currTile.SetBorderColor(regionColor);
                    borders.Add(border);
                }
            }
        }
        return borders;
    }
    public List<Region> AdjacentRegions() {
        List<Region> adjacent = new List<Region>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            List<Region> regions;
            if (currTile.TryGetDifferentRegionNeighbours(out regions)) {
                for (int j = 0; j < regions.Count; j++) {
                    Region currRegion = regions[j];
                    if (!adjacent.Contains(currRegion)) {
                        adjacent.Add(currRegion);
                    }
                }
            }
        }
        return adjacent;
    }
    public void OnHoverOverAction() {
        ShowSolidBorder();
    }
    public void OnHoverOutAction() {
        if (UIManager.Instance.regionInfoUI.isShowing) {
            if (UIManager.Instance.regionInfoUI.activeRegion != this) {
                ShowTransparentBorder();
            }
        } else {
            ShowTransparentBorder();
        }

    }
    public void ShowSolidBorder() {
        for (int i = 0; i < _borderSprites.Count; i++) {
            SpriteRenderer s = _borderSprites[i];
            Color color = s.color;
            color.a = HoveredBorderAlpha;
            s.color = color;
            s.gameObject.SetActive(true);
        }
    }
    public void ShowTransparentBorder() {
        for (int i = 0; i < _borderSprites.Count; i++) {
            SpriteRenderer s = _borderSprites[i];
            Color color = s.color;
            color.a = UnhoveredBorderAlpha;
            s.color = color;
            s.gameObject.SetActive(true);
        }
    }
    public void CenterCameraOnRegion() {
        coreTile.CenterCameraHere();
    }
    public bool HasTileWithFeature(string featureName) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.featureComponent.HasFeature(featureName)) {
                return true;
            }
        }
        return false;
    }
    public List<HexTile> GetTilesWithFeature(string featureName) {
        List<HexTile> tilesWithFeature = new List<HexTile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            if (tile.featureComponent.HasFeature(featureName)) {
                tilesWithFeature.Add(tile);
            }
        }
        return tilesWithFeature;
    }
    #endregion

    #region Invasion
    public bool CanBeInvaded() {
        // if (settlement != null) {
        //     return settlement.CanInvadeSettlement();
        // }
        //return HasCorruptedConnection() &&!coreTile.isCorrupted && !demonicInvasionData.beingInvaded;
        return  !coreTile.isCorrupted; //HasCorruptedConnection() && TODO:
    }
    public void StartInvasion(Minion assignedMinion) {
        //PlayerManager.Instance.player.SetInvadingRegion(this);
        assignedMinion.SetAssignedRegion(this);
        SetAssignedMinion(assignedMinion);

        demonicInvasionData = new DemonicLandmarkInvasionData() {
            beingInvaded = true,
            currentDuration = 0,
        };

        //ticksInInvasion = 0;
        Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
        // TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.settlementOfTile != null ? mainLandmark.tileLocation.settlementOfTile.name : name), mainLandmark.invasionTicks, () => UIManager.Instance.ShowRegionInfo(this));
    }
    public void LoadInvasion(SaveDataRegion data) {
        //PlayerManager.Instance.player.SetInvadingRegion(this);
        //assignedMinion.SetAssignedRegion(this);
        //SetAssignedMinion(assignedMinion);

        demonicInvasionData = data.demonicInvasionData;
        if (demonicInvasionData.beingInvaded) {
            Messenger.AddListener(Signals.TICK_STARTED, PerInvasionTick);
            // TimerHubUI.Instance.AddItem("Invasion of " + (mainLandmark.tileLocation.settlementOfTile != null ? mainLandmark.tileLocation.settlementOfTile.name : name), mainLandmark.invasionTicks - demonicInvasionData.currentDuration, () => UIManager.Instance.ShowRegionInfo(this));
        }
    }
    private void PerInvasionTick() {
        DemonicLandmarkInvasionData tempData = demonicInvasionData;
        tempData.currentDuration++;
        demonicInvasionData = tempData;
        if (demonicInvasionData.currentDuration > mainLandmark.invasionTicks) {
            //invaded.
            Invade();
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "You have successfully invaded " + this.name, () => UIManager.Instance.ShowRegionInfo(this));
            Messenger.RemoveListener(Signals.TICK_STARTED, PerInvasionTick);
        }
    }
    private void Invade() {
        //corrupt region
        InvadeActions();
        //TODO:
        // LandmarkManager.Instance.OwnRegion(PlayerManager.Instance.player.playerFaction, this);
        //PlayerManager.Instance.AddTileToPlayerArea(coreTile);
        //PlayerManager.Instance.player.SetInvadingRegion(null);
        demonicInvasionData = new DemonicLandmarkInvasionData();
        assignedMinion.SetAssignedRegion(null);
        SetAssignedMinion(null);

        //This is done so that when a region is invaded by the player, the showing Info UI will update appropriately
        if (UIManager.Instance.regionInfoUI.isShowing && UIManager.Instance.regionInfoUI.activeRegion == this) {
            UIManager.Instance.ShowRegionInfo(this);
        }
    }
    public void SetAssignedMinion(Minion minion) {
        Minion previouslyAssignedMinion = assignedMinion;
        assignedMinion = minion;
        if (assignedMinion != null) {
            AddCharacterToLocation(assignedMinion.character);
            mainLandmark.OnMinionAssigned(assignedMinion); //a new minion was assigned 
        } else if (previouslyAssignedMinion != null) {
            RemoveCharacterFromLocation(previouslyAssignedMinion.character);
            mainLandmark.OnMinionUnassigned(previouslyAssignedMinion); //a minion was unassigned
        }
    }
    #endregion

    #region Player Build Structure
    public void StartBuildingStructure(LANDMARK_TYPE landmarkType, Minion minion) {
        SetAssignedMinion(minion);
        minion.SetAssignedRegion(this);
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        demonicBuildingData = new DemonicLandmarkBuildingData() {
            landmarkType = landmarkType,
            landmarkName = landmarkData.landmarkTypeString,
            buildDuration = landmarkData.buildDuration + Mathf.RoundToInt(landmarkData.buildDuration * PlayerManager.Instance.player.constructionRatePercentageModifier),
            currentDuration = 0,
        };
        coreTile.UpdateBuildSprites();
        TimerHubUI.Instance.AddItem("Building " + demonicBuildingData.landmarkName + " at " + name, demonicBuildingData.buildDuration, () => UIManager.Instance.ShowRegionInfo(this));
        Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
    }
    public void LoadBuildingStructure(SaveDataRegion data) {
        demonicBuildingData = data.demonicBuildingData;
        if (demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            TimerHubUI.Instance.AddItem("Building " + demonicBuildingData.landmarkName + " at " + name, demonicBuildingData.buildDuration - demonicBuildingData.currentDuration, () => UIManager.Instance.ShowRegionInfo(this));
            Messenger.AddListener(Signals.TICK_STARTED, PerTickBuilding);
        }
    }
    private void PerTickBuilding() {
        DemonicLandmarkBuildingData tempData = demonicBuildingData;
        tempData.currentDuration++;
        demonicBuildingData = tempData;
        if (demonicBuildingData.currentDuration >= demonicBuildingData.buildDuration) {
            FinishBuildingStructure();
        }
    }
    private void FinishBuildingStructure() {
        //NOTE: We do not call SetAssignedMinion to null and SetAssignedRegion to null here because it is already called in CreateNewLandmarkOnTile inside DestroyLandmarkOnTile
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBuilding);
        //mainLandmark.ChangeLandmarkType(demonicBuildingData.landmarkType);
        //int previousID = mainLandmark.id;
        BaseLandmark newLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(coreTile, demonicBuildingData.landmarkType, false);
        //newLandmark.OverrideID(previousID);

        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Finished building " + Utilities.NormalizeStringUpperCaseFirstLetters(newLandmark.specificLandmarkType.ToString()) + " at " + this.name, () => UIManager.Instance.ShowRegionInfo(this));
        demonicBuildingData = new DemonicLandmarkBuildingData();
        //assignedMinion.SetAssignedRegion(null);
        //SetAssignedMinion(null);

        newLandmark.OnFinishedBuilding();
        coreTile.UpdateBuildSprites();
        Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, this);
    }
    private void StopBuildingStructure() {
        if (demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBuilding);
            TimerHubUI.Instance.RemoveItem("Building " + demonicBuildingData.landmarkName + " at " + name);
            Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, this);
            demonicBuildingData = new DemonicLandmarkBuildingData();
            coreTile.UpdateBuildSprites();
        }
    }
    #endregion

    #region Corruption/Invasion
    public void InvadeActions() {
        mainLandmark?.ChangeLandmarkType(LANDMARK_TYPE.NONE);
        // ActivateRegionFeatures();
        // RemoveFeaturesAfterInvade();
        // ExecuteEventAfterInvasion();
        // ExecuteOtherAfterInvasionActions();
    }
    #endregion

    #region Events
    public void SetEventIcon(GameObject go) {
        eventIconGo = go;
    }
    public void OnCleansedRegion() {
        StopBuildingStructure();
    }
    #endregion

    #region Characters
    public void LoadCharacterHere(Character character) {
        charactersAtLocation.Add(character);
        character.SetRegionLocation(this);
        Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            character.SetRegionLocation(this);
            Messenger.Broadcast(Signals.CHARACTER_ENTERED_REGION, character, this);
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            character.currentStructure?.RemoveCharacterAtLocation(character);
            // for (int i = 0; i < features.Count; i++) {
            //     features[i].OnRemoveCharacterFromRegion(this, character);
            // }
            character.SetRegionLocation(null);
            Messenger.Broadcast(Signals.CHARACTER_EXITED_REGION, character, this);
        }
    }
    public void RemoveCharacterFromLocation(Party party) {
        RemoveCharacterFromLocation(party.owner);
    }
    public bool IsResident(Character character) {
        return residents.Contains(character);
    }
    public bool AddResident(Character character) {
        if (!residents.Contains(character)) {
            residents.Add(character);
            character.SetHomeRegion(this);
        }
        return false;
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            character.SetHomeRegion(null);
        }
    }
    #endregion

    #region Faction
    public void AddFactionHere(Faction faction) {
        if (!IsFactionHere(faction)) {
            factionsHere.Add(faction);
            //Once a faction is added and there is no ruling faction yet, automatically let the added faction own the region
            //TODO:
            // if(owner == null) {
            //     LandmarkManager.Instance.OwnRegion(faction, this);
            // }
        }
    }
    public void RemoveFactionHere(Faction faction) {
        if (factionsHere.Remove(faction)) {
            //If a faction is removed and it is the ruling faction, transfer ruling faction to the next faction on the list if there's any, if not make the region part of neutral faction
            //TODO:
            // if(owner == faction) {
            //     LandmarkManager.Instance.UnownSettlement(this);
            //     if(factionsHere.Count > 0) {
            //         LandmarkManager.Instance.OwnRegion(factionsHere[0], this);
            //     } else {
            //         FactionManager.Instance.neutralFaction.AddToOwnedSettlements(this);
            //     }
            // }
        }
    }
    public bool IsFactionHere(Faction faction) {
        return factionsHere.Contains(faction);
    }
    #endregion

    #region Awareness
    public bool AddAwareness(IPointOfInterest pointOfInterest) {
        if (!HasAwareness(pointOfInterest)) {
            if (!awareness.ContainsKey(pointOfInterest.poiType)) {
                awareness.Add(pointOfInterest.poiType, new List<IPointOfInterest>());
            }
            awareness[pointOfInterest.poiType].Add(pointOfInterest);
            //if (pointOfInterest is TreeObject) {
            //    List<IPointOfInterest> treeAwareness = GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE.TREE_OBJECT);
            //    if (treeAwareness.Count >= Character.TREE_AWARENESS_LIMIT) {
            //        RemoveAwareness(treeAwareness[0]);
            //    }
            //}
            return true;
        }
        return false;
    }
    public void RemoveAwareness(IPointOfInterest pointOfInterest) {
        if (awareness.ContainsKey(pointOfInterest.poiType)) {
            List<IPointOfInterest> awarenesses = awareness[pointOfInterest.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IPointOfInterest iawareness = awarenesses[i];
                if (iawareness == pointOfInterest) {
                    awarenesses.RemoveAt(i);
                    break;
                }
            }
        }
    }
    public void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType) {
        if (awareness.ContainsKey(poiType)) {
            awareness.Remove(poiType);
        }
    }
    public bool HasAwareness(IPointOfInterest poi) {
        if (awareness.ContainsKey(poi.poiType)) {
            List<IPointOfInterest> awarenesses = awareness[poi.poiType];
            for (int i = 0; i < awarenesses.Count; i++) {
                IPointOfInterest currPOI = awarenesses[i];
                if (currPOI == poi) {
                    return true;
                }
            }
            return false;
        }
        return false;
    }
    #endregion
    
    #region Structures
    public void GenerateStructures() {
        _structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS);
    }
    public void AddStructure(LocationStructure structure) {
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
        }
    }
    public void RemoveStructure(LocationStructure structure) {
        if (structures.ContainsKey(structure.structureType)) {
            if (structures[structure.structureType].Remove(structure)) {

                if (structures[structure.structureType].Count == 0) { //this is only for optimization
                    structures.Remove(structure.structureType);
                }
            }
        }
    }
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            return structures[type][Utilities.rng.Next(0, structures[type].Count)];
        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> _structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>(this.structures);
        _structures.Remove(STRUCTURE_TYPE.EXIT);
        int dictIndex = UnityEngine.Random.Range(0, _structures.Count);
        int count = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in _structures) {
            if (count == dictIndex) {
                return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
            count++;
        }
        return null;
    }
    public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> locStructures = structures[type];
            for (int i = 0; i < locStructures.Count; i++) {
                if(locStructures[i].id == id) {
                    return locStructures[i];
                }
            }
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation() {
        List<LocationStructure> _structures = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.structureType != STRUCTURE_TYPE.EXIT) {
                    _structures.Add(currStructure);
                }
            }
        }
        return _structures;
    }
    public List<T> GetStructuresAtLocation<T>(STRUCTURE_TYPE type) where T : LocationStructure{
        List<T> _structures = new List<T>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.structureType == type) {
                    _structures.Add(currStructure as T);
                }
            }
        }
        return _structures;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    public void OnLocationStructureObjectPlaced(LocationStructure structure) {
        if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //if a warehouse was placed, and this settlement does not yet have a main storage structure, or is using the city center as their main storage structure, then use the new warehouse instead.
            if (mainStorage == null || mainStorage.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                SetMainStorage(structure);
            }
        } else if (structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (mainStorage == null) {
                SetMainStorage(structure);
            }
        }
    }
    private void SetMainStorage(LocationStructure structure) {
        bool shouldCheckResourcePiles = mainStorage != null && structure != null && mainStorage != structure;
        mainStorage = structure;
        if (shouldCheckResourcePiles) {
            Messenger.Broadcast(Signals.REGION_CHANGE_STORAGE, this);
        }
    }
    #endregion

    #region Inner Map
    public void SetRegionInnerMap(RegionInnerTileMap regionInnerTileMap) {
        _regionInnerTileMap = regionInnerTileMap;
    }
    //public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
    //    token.SetOwner(this.owner);
    //    if (innerMap != null) { //if the settlement map of this settlement has already been created.
    //        if (structure != null) {
    //            structure.AddItem(token, gridLocation);
    //        } else {
    //            //get structure for token
    //            LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //            chosen.AddItem(token);
    //        }
    //    }
    //    return true;
    //}
    //public void RemoveSpecialTokenFromLocation(SpecialToken token) {
    //    LocationStructure takenFrom = token.structureLocation;
    //    if (takenFrom != null) {
    //        takenFrom.RemoveItem(token);
    //    }
    //}
    public bool IsRequiredByLocation(SpecialToken token) {
        return false;
    }
    public bool IsSameCoreLocationAs(ILocation location) {
        return location.coreTile == this.coreTile;
    }
    public void SetRegionTileObject(RegionTileObject _regionTileObject) {
        regionTileObject = _regionTileObject;
    }
    public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsOfType(type));
            }
        }
        return objs;
    }
    #endregion

    #region Hex Tile Map
    private void DetermineHexTileMap() {
        int maxX = tiles.Max(t => t.data.xCoordinate);
        int minX = tiles.Min(t => t.data.xCoordinate);
        int maxY = tiles.Max(t => t.data.yCoordinate);
        int minY = tiles.Min(t => t.data.yCoordinate);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        
        hexTileMap = new HexTile[width, height];
        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                int mapXIndex = x - minX;
                int mapYIndex = y - minY;

                HexTile tile = GridMap.Instance.map[x, y];
                if (tiles.Contains(tile)) {
                    hexTileMap[mapXIndex, mapYIndex] = tile;
                } else {
                    hexTileMap[mapXIndex, mapYIndex] = null;
                }
            }
        }
    }
    public HexTile GetLeftMostTile() {
        int leftMostXCoordinate = GetLeftMostCoordinate();
        //loop through even rows first, if there are left most tiles that are
        //on an even row, then consider them as the left most tile.
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile != null
                    && Utilities.IsEven(tile.yCoordinate)
                    && tile.xCoordinate == leftMostXCoordinate) {
                    return tile;
                }
            }    
        }
        //if no left most tile is in an even row, then just return the first tile that is on
        //the left most column
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile != null && tile.xCoordinate == leftMostXCoordinate) {
                    return tile;
                }
            }    
        }

        return null; //NOTE: this should never happen
    }
    public HexTile GetRightMostTile() {
        int rightMostXCoordinate = GetRightMostCoordinate();
        //loop through odd rows first, if there are right most tiles that are
        //on an odd row, then consider them as the right most tile.
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile != null 
                    && Utilities.IsEven(tile.yCoordinate) == false 
                    && tile.xCoordinate == rightMostXCoordinate) {
                    return tile;
                }
            }
        }
        //if no right most tile is in an odd row, then just return the first tile that is on
        //the right most column
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile != null && tile.xCoordinate == rightMostXCoordinate) {
                    return tile;
                }
            }    
        }

        return null; //NOTE: this should never happen
    }
    private int GetLeftMostCoordinate() {
        return tiles.Min(t => t.data.xCoordinate);
    }
    private int GetRightMostCoordinate() {
        return tiles.Max(t => t.data.xCoordinate);
    }
    public List<int> GetLeftMostRows() {
        List<int> rows = new List<int>();
        HexTile leftMostTile = GetLeftMostTile();
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile != null 
                    && tile.xCoordinate == leftMostTile.xCoordinate
                    && Utilities.IsEven(leftMostTile.yCoordinate) == Utilities.IsEven(tile.yCoordinate) //only include tiles that are on the same row type as the left most tile (odd/even)
                    && rows.Contains(y) == false) {
                    rows.Add(y);
                }
            }
        }
        return rows;
    }
    public List<int> GetRightMostRows() {
        List<int> rows = new List<int>();
        HexTile rightMostTile = GetRightMostTile();
        for (int x = 0; x <= hexTileMap.GetUpperBound(0); x++) {
            for (int y = 0; y <= hexTileMap.GetUpperBound(1); y++) {
                HexTile tile = hexTileMap[x, y];
                if (tile != null 
                    && tile.xCoordinate == rightMostTile.xCoordinate
                    && Utilities.IsEven(rightMostTile.yCoordinate) == Utilities.IsEven(tile.yCoordinate) //only include tiles that are on the same row type as the right most tile (odd/even)
                    && rows.Contains(y) == false) {
                    rows.Add(y);
                }
            }
        }
        return rows;
    }
    public bool AreLeftAndRightMostTilesInSameRowType() {
        List<int> leftMostRows = GetLeftMostRows();
        List<int> rightMostRows = GetRightMostRows();
        for (int i = 0; i < leftMostRows.Count; i++) {
            int currLeftRow = leftMostRows[i];
            if (rightMostRows.Contains(currLeftRow)) {
                //left most rows and right most rows have at least 1 row in common
                return true;
            } else {
                bool isLeftRowEven = Utilities.IsEven(currLeftRow);
                for (int j = 0; j < rightMostRows.Count; j++) {
                    int currRightRow = rightMostRows[j];
                    bool isRightRowEven = Utilities.IsEven(currRightRow);
                    if (isLeftRowEven == isRightRowEven) {
                        return true;
                    }
                }  
            }
        }
        return false;
    }
    #endregion
}
