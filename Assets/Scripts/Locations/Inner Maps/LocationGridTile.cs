﻿using System.Collections.Generic;
using System.Linq;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using PathFind;
using Traits;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class LocationGridTile : IHasNeighbours<LocationGridTile> {

        public enum Tile_Type { Empty, Wall, Structure_Entrance }
        public enum Tile_State { Empty, Occupied }
        public enum Ground_Type { Soil, Grass, Stone, Snow, Tundra, Cobble, Wood, Snow_Dirt, Water }
        public bool hasDetail { get; set; }
        public InnerTileMap parentMap { get; private set; }
        public Tilemap parentTileMap { get; private set; }
        public Vector3Int localPlace { get; private set; }
        public Vector3 worldLocation { get; private set; }
        public Vector3 centeredWorldLocation { get; private set; }
        public Vector3 localLocation { get; private set; }
        public Vector3 centeredLocalLocation { get; private set; }
        public bool isInside { get; private set; }
        public Tile_Type tileType { get; private set; }
        public Tile_State tileState { get; private set; }
        public Ground_Type groundType { get; private set; }
        public LocationStructure structure { get; private set; }
        public Dictionary<GridNeighbourDirection, LocationGridTile> neighbours { get; private set; }
        public List<LocationGridTile> neighbourList { get; private set; }
        public IPointOfInterest objHere { get; private set; }
        public List<Character> charactersHere { get; private set; }
        public bool isOccupied { get { return tileState == Tile_State.Occupied; } }
        public bool isLocked { get; private set; } //if a tile is locked, any asset on it should not be replaced.
        public TILE_OBJECT_TYPE reservedObjectType { get; private set; } //the only type of tile object that can be placed here
        public FurnitureSpot furnitureSpot { get; private set; }
        public bool hasFurnitureSpot { get; private set; }
        public List<Trait> normalTraits {
            get { return genericTileObject.traitContainer.allTraits; }
        }
        public bool hasBlueprint { get; private set; }

        private Color defaultTileColor;

        public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty).ToList(); } }
        public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbours.Values.Where(o => !o.isOccupied && o.structure == this.structure).ToList(); } }

        public GenericTileObject genericTileObject { get; private set; }
        public List<WallObject> walls { get; private set; }
        public bool isPartOfRegion => buildSpotOwner.hexTileOwner != null;
        
        public LocationGridTile(int x, int y, Tilemap tilemap, InnerTileMap parentMap) {
            this.parentMap = parentMap;
            parentTileMap = tilemap;
            localPlace = new Vector3Int(x, y, 0);
            worldLocation = tilemap.CellToWorld(localPlace);
            localLocation = tilemap.CellToLocal(localPlace);
            centeredLocalLocation = new Vector3(localLocation.x + 0.5f, localLocation.y + 0.5f, localLocation.z);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
            tileType = Tile_Type.Empty;
            tileState = Tile_State.Empty;
            charactersHere = new List<Character>();
            walls = new List<WallObject>();
            SetLockedState(false);
            SetReservedType(TILE_OBJECT_TYPE.NONE);
            defaultTileColor = Color.white;
        }
        public LocationGridTile(SaveDataLocationGridTile data, Tilemap tilemap, InnerTileMap parentMap) {
            this.parentMap = parentMap;
            parentTileMap = tilemap;
            localPlace = new Vector3Int((int)data.localPlace.x, (int)data.localPlace.y, 0);
            worldLocation = data.worldLocation;
            localLocation = data.localLocation;
            centeredLocalLocation = data.centeredLocalLocation;
            centeredWorldLocation = data.centeredWorldLocation;
            tileType = data.tileType;
            tileState = data.tileState;
            SetLockedState(data.isLocked);
            SetReservedType(data.reservedObjectType);
            charactersHere = new List<Character>();
            walls = new List<WallObject>();
            defaultTileColor = Color.white;
        }

        public void CreateGenericTileObject() {
            genericTileObject = new GenericTileObject();
        }
        public void UpdateWorldLocation() {
            worldLocation = parentTileMap.CellToWorld(localPlace);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        }
        public List<LocationGridTile> FourNeighbours() {
            List<LocationGridTile> fn = new List<LocationGridTile>();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Key.IsCardinalDirection()) {
                    fn.Add(keyValuePair.Value);
                }
            }
            return fn;
        }
        public Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary() {
            Dictionary<GridNeighbourDirection, LocationGridTile> fn = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Key.IsCardinalDirection()) {
                    fn.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return fn;
        }
        public void FindNeighbours(LocationGridTile[,] map) {
            neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            int mapUpperBoundX = map.GetUpperBound(0);
            int mapUpperBoundY = map.GetUpperBound(1);
            Point thisPoint = new Point(localPlace.x, localPlace.y);
            foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
                GridNeighbourDirection currDir = kvp.Key;
                Point exit = kvp.Value;
                Point result = exit.Sum(thisPoint);
                if (Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                    Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                    neighbours.Add(currDir, map[result.X, result.Y]);
                    neighbourList.Add(map[result.X, result.Y]);
                }

            }
        }
        public Dictionary<GridNeighbourDirection, Point> possibleExits {
            get {
                return new Dictionary<GridNeighbourDirection, Point>() {
                    {GridNeighbourDirection.North, new Point(0,1) },
                    {GridNeighbourDirection.South, new Point(0,-1) },
                    {GridNeighbourDirection.West, new Point(-1,0) },
                    {GridNeighbourDirection.East, new Point(1,0) },
                    {GridNeighbourDirection.North_West, new Point(-1,1) },
                    {GridNeighbourDirection.North_East, new Point(1,1) },
                    {GridNeighbourDirection.South_West, new Point(-1,-1) },
                    {GridNeighbourDirection.South_East, new Point(1,-1) },
                };
            }
        }
        public void SetIsInside(bool isInside) {
            this.isInside = isInside;
        }
        public void SetTileType(Tile_Type tileType) {
            this.tileType = tileType;
        }
        private void SetGroundType(Ground_Type groundType) {
            this.groundType = groundType;
            if (genericTileObject != null) {
                switch (groundType) {
                    case Ground_Type.Grass:
                    case Ground_Type.Wood:
                        genericTileObject.traitContainer.AddTrait(genericTileObject, "Flammable");
                        break;
                    default:
                        genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                        break;
                }
            }
        }
        private void UpdateGroundTypeBasedOnAsset() {
            Sprite groundAsset = parentMap.groundTilemap.GetSprite(this.localPlace);
            if (groundAsset != null) {
                string assetName = groundAsset.name.ToLower();
                if (assetName.Contains("structure floor") || assetName.Contains("wood")) {
                    SetGroundType(Ground_Type.Wood);
                } else if (assetName.Contains("cobble")) {
                    SetGroundType(Ground_Type.Cobble);
                } else if (assetName.Contains("water") || assetName.Contains("pond")) {
                    SetGroundType(Ground_Type.Water);
                } else if ((assetName.Contains("Dirt") || assetName.Contains("dirt") || assetName.Contains("snowoutside")) && (parentMap.location.coreTile.biomeType == BIOMES.SNOW || parentMap.location.coreTile.biomeType == BIOMES.TUNDRA)) {
                    SetGroundType(Ground_Type.Tundra);
                    //override tile to use tundra soil
                    parentMap.groundTilemap.SetTile(this.localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                } else if (assetName.Contains("snow")) {
                    if (assetName.Contains("dirt")) {
                        SetGroundType(Ground_Type.Snow_Dirt);
                    } else {
                        SetGroundType(Ground_Type.Snow);
                    }
                } else if (assetName.Contains("stone") || assetName.Contains("road")) {
                    SetGroundType(Ground_Type.Stone);
                } else if (assetName.Contains("grass")) {
                    SetGroundType(Ground_Type.Grass);
                } else if (assetName.Contains("soil") || assetName.Contains("outside") || assetName.Contains("dirt")) {
                    SetGroundType(Ground_Type.Soil);
                } else if (assetName.Contains("tundra")) {
                    SetGroundType(Ground_Type.Tundra);
                    //override tile to use tundra soil
                    parentMap.groundTilemap.SetTile(this.localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                } 
            }
        }
        public bool TryGetNeighbourDirection(LocationGridTile tile, out GridNeighbourDirection dir) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Value == tile) {
                    dir = keyValuePair.Key;
                    return true;
                }
            }
            dir = GridNeighbourDirection.East;
            return false;
        }

        #region Visuals
        public TileBase previousGroundVisual { get; private set; }
        public void SetGroundTilemapVisual(TileBase tileBase) {
            SetPreviousGroundVisual(parentMap.groundTilemap.GetTile(this.localPlace));
            parentMap.groundTilemap.SetTile(this.localPlace, tileBase);
            UpdateGroundTypeBasedOnAsset();
        }
        public void SetPreviousGroundVisual(TileBase tileBase) {
            previousGroundVisual = tileBase;
        }
        public void RevertToPreviousGroundVisual() {
            if (previousGroundVisual != null) {
                SetGroundTilemapVisual(previousGroundVisual);
            }
        }
        public void CreateSeamlessEdgesForSelfAndNeighbours() {
            CreateSeamlessEdgesForTile(parentMap);
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbour = neighbourList[i];
                neighbour.CreateSeamlessEdgesForTile(parentMap);
            }
        }
        public void CreateSeamlessEdgesForTile(InnerTileMap map) {
            string summary = $"Creating seamless edges for tile {this.ToString()}";
            Dictionary<GridNeighbourDirection, LocationGridTile> neighbours;
            if (HasCardinalNeighbourOfDifferentGroundType(out neighbours)) {
                summary += $"\nHas Neighbour of different ground type. Checking neighbours {neighbours.Count.ToString()}";
                //grass should be higher than dirt
                //dirt should be higher than cobble
                //cobble should be higher than grass
                foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                    LocationGridTile currNeighbour = keyValuePair.Value;
                    //if (currNeighbour.structure != null && !currNeighbour.structure.structureType.IsOpenSpace()) { continue; } //skip non open space structure tiles.
                    bool createEdge = false;
                    //if (tile.groundType == currNeighbour.groundType) {
                    //    createEdge = true;
                    //} else 
                    summary += $"\n\tChecking {currNeighbour.ToString()}. Ground type is {groundType.ToString()}. Neighbour Ground Type is {currNeighbour.groundType.ToString()}";
                    if (currNeighbour.tileType == Tile_Type.Wall || currNeighbour.tileType == Tile_Type.Structure_Entrance) {
                        createEdge = false;
                    } else if (groundType != LocationGridTile.Ground_Type.Water && currNeighbour.groundType == LocationGridTile.Ground_Type.Water) {
                        createEdge = true;
                    } else if (groundType == LocationGridTile.Ground_Type.Snow) {
                        createEdge = true;
                    } else if (groundType == LocationGridTile.Ground_Type.Cobble && currNeighbour.groundType != LocationGridTile.Ground_Type.Snow) {
                        createEdge = true;
                    } else if ((groundType == LocationGridTile.Ground_Type.Tundra || groundType == LocationGridTile.Ground_Type.Snow_Dirt) &&
                               (currNeighbour.groundType == LocationGridTile.Ground_Type.Stone || currNeighbour.groundType == LocationGridTile.Ground_Type.Soil)) {
                        createEdge = true;
                    } else if (groundType == LocationGridTile.Ground_Type.Grass && currNeighbour.groundType == LocationGridTile.Ground_Type.Soil) {
                        createEdge = true;
                    } else if (groundType == LocationGridTile.Ground_Type.Soil && currNeighbour.groundType == LocationGridTile.Ground_Type.Stone) {
                        createEdge = true;
                    } else if (groundType == LocationGridTile.Ground_Type.Stone && currNeighbour.groundType == LocationGridTile.Ground_Type.Grass) {
                        createEdge = true;
                    }
                    summary += $"\n\tWill create edge? {createEdge.ToString()}. At {keyValuePair.Key.ToString()}";
                    if (createEdge) {
                        Tilemap mapToUse;
                        switch (keyValuePair.Key) {
                            case GridNeighbourDirection.North:
                                mapToUse = map.northEdgeTilemap;
                                break;
                            case GridNeighbourDirection.South:
                                mapToUse = map.southEdgeTilemap;
                                break;
                            case GridNeighbourDirection.West:
                                mapToUse = map.westEdgeTilemap;
                                break;
                            case GridNeighbourDirection.East:
                                mapToUse = map.eastEdgeTilemap;
                                break;
                            default:
                                mapToUse = null;
                                break;
                        }
                        System.Diagnostics.Debug.Assert(mapToUse != null, nameof(mapToUse) + " != null");
                        mapToUse.SetTile(localPlace, InnerMapManager.Instance.assetManager.edgeAssets[groundType][(int)keyValuePair.Key]);
                    }
                }
            }
//            Debug.Log(summary);
        }
        #endregion

        #region Structures
        public void SetStructure(LocationStructure structure) {
            if (this.structure != null) {
                this.structure.RemoveTile(this);
            }
            this.structure = structure;
            this.structure.AddTile(this);
            if (!genericTileObject.hasBeenInitialized) { //TODO: Make this better
                genericTileObject.ManualInitialize(structure, this);
            }
        }
        public void SetTileState(Tile_State state) {
            if (structure != null) {
                if (this.tileState == Tile_State.Empty && state == Tile_State.Occupied) {
                    structure.RemoveUnoccupiedTile(this);
                } else if (this.tileState == Tile_State.Occupied && state == Tile_State.Empty && reservedObjectType == TILE_OBJECT_TYPE.NONE) {
                    structure.AddUnoccupiedTile(this);
                }
            }
            this.tileState = state;
            //if (state == Tile_State.Occupied) {
            //    Messenger.Broadcast(Signals.TILE_OCCUPIED, this, objHere);
            //}
        }
        #endregion

        #region Characters
        public void AddCharacterHere(Character character) {
            if (!charactersHere.Contains(character)) {
                charactersHere.Add(character);
            }
        }
        public void RemoveCharacterHere(Character character) {
            charactersHere.Remove(character);
        }
        #endregion

        #region Points of Interest
        public void SetObjectHere(IPointOfInterest poi) {
            objHere = poi;
            poi.SetGridTileLocation(this);
            poi.OnPlacePOI();
            SetTileState(Tile_State.Occupied);
            Messenger.Broadcast(Signals.OBJECT_PLACED_ON_TILE, this, poi);
        }
        public IPointOfInterest RemoveObjectHere(Character removedBy) {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                if (objHere is TileObject && removedBy != null) {
                    //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                    (objHere as TileObject).RemoveTileObject(removedBy);
                } else {
                    objHere.SetGridTileLocation(null);
                    objHere.OnDestroyPOI();
                }
                objHere = null;
                SetTileState(Tile_State.Empty);
                return removedObj;
            }
            return null;
        }
        public IPointOfInterest RemoveObjectHereWithoutDestroying() {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                objHere.SetGridTileLocation(null);
                objHere = null;
                SetTileState(Tile_State.Empty);
                return removedObj;
            }
            return null;
        }
        #endregion

        #region Utilities
        public bool IsAtEdgeOfMap() {
            GridNeighbourDirection[] dirs = Utilities.GetEnumValues<GridNeighbourDirection>();
            for (int i = 0; i < dirs.Length; i++) {
                if (!neighbours.ContainsKey(dirs[i])) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighborAtEdgeOfMap() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
                if (kvp.Value.IsAtEdgeOfMap()) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Does this tile have a neighbour that is part of a different structure, or is part of the outside map?
        /// </summary>
        public bool HasDifferentDwellingOrOutsideNeighbour() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
                if (!kvp.Value.isInside || (kvp.Value.structure != this.structure)) {
                    return true;
                }
            }
            return false;
        }
        public bool IsAdjacentToWall() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> kvp in neighbours) {
                if (kvp.Value.tileType == Tile_Type.Wall || (kvp.Value.structure != null && kvp.Value.structure.structureType != STRUCTURE_TYPE.WORK_AREA)) {
                    return true;
                }
            }
            return false;
        }
        public override string ToString() {
            return localPlace.ToString();
        }
        public float GetDistanceTo(LocationGridTile tile) {
            return Vector2.Distance(this.localLocation, tile.localLocation);
        }
        public bool HasOccupiedNeighbour() {
            for (int i = 0; i < neighbours.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).isOccupied) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourOfType(Tile_Type type, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).tileType == type) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNeighbour(LocationGridTile tile) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Value == tile) {
                    return true;
                }
            }
            return false;
        }
        public bool IsAdjacentTo(System.Type type) {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if ((keyValuePair.Value.objHere != null && keyValuePair.Value.objHere.GetType() == type)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbouringWalledStructure() {
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in neighbours) {
                if (keyValuePair.Value.structure != null && keyValuePair.Value.structure.structureType.IsOpenSpace() == false) {
                    return true;
                }
            }
            return false;
        }
        public LocationGridTile GetNearestUnoccupiedTileFromThis() {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                if (this.structure != null) {
                    LocationGridTile nearestTile = null;
                    float nearestDist = 99999f;
                    for (int i = 0; i < this.structure.unoccupiedTiles.Count; i++) {
                        LocationGridTile currTile = this.structure.unoccupiedTiles[i];
                        if (currTile != this && currTile.groundType != Ground_Type.Water) {
                            float dist = Vector2.Distance(currTile.localLocation, this.localLocation);
                            if (dist < nearestDist) {
                                nearestTile = currTile;
                                nearestDist = dist;
                            }
                        }
                    }
                    return nearestTile;
                }
            } else {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
            return null;
        }
        public LocationGridTile GetRandomUnoccupiedNeighbor() {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count > 0) {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
            return null;
        }
        public void SetLockedState(bool state) {
            isLocked = state;
        }
        public bool IsAtEdgeOfWalkableMap() {
            if ((localPlace.y == AreaInnerTileMap.SouthEdge && localPlace.x >= AreaInnerTileMap.WestEdge && localPlace.x <= parentMap.width - AreaInnerTileMap.EastEdge - 1)
                || (localPlace.y == parentMap.height - AreaInnerTileMap.NorthEdge - 1 && localPlace.x >= AreaInnerTileMap.WestEdge && localPlace.x <= parentMap.width - AreaInnerTileMap.EastEdge - 1)
                || (localPlace.x == AreaInnerTileMap.WestEdge && localPlace.y >= AreaInnerTileMap.SouthEdge && localPlace.y <= parentMap.height - AreaInnerTileMap.NorthEdge - 1) 
                || (localPlace.x == parentMap.width - AreaInnerTileMap.EastEdge - 1 && localPlace.y >= AreaInnerTileMap.SouthEdge && localPlace.y <= parentMap.height - AreaInnerTileMap.NorthEdge - 1)) {
                return true;
            }
            return false;
        }
        public void HighlightTile() {
            parentMap.groundTilemap.SetColor(localPlace, Color.blue);
        }
        public void HighlightTile(Color color) {
            parentMap.groundTilemap.SetColor(localPlace, color);
        }
        public void UnhighlightTile() {
            parentMap.groundTilemap.SetColor(localPlace, defaultTileColor);
        }
        public bool HasCardinalNeighbourOfDifferentGroundType(out Dictionary<GridNeighbourDirection, LocationGridTile> differentTiles) {
            bool hasDiff = false;
            differentTiles = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            Dictionary<GridNeighbourDirection, LocationGridTile> cardinalNeighbours = FourNeighboursDictionary();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in cardinalNeighbours) {
                if (keyValuePair.Value.groundType != this.groundType) {
                    hasDiff = true;
                    differentTiles.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return hasDiff;
        }
        public void SetDefaultTileColor(Color color) {
            defaultTileColor = color;
        }
        public List<ITraitable> GetTraitablesOnTileWithTrait(string requiredTrait) {
            List<ITraitable> traitables = new List<ITraitable>();
            if (genericTileObject.traitContainer.GetNormalTrait<Trait>(requiredTrait) != null) {
                traitables.Add(genericTileObject);
            }
            for (int i = 0; i < walls.Count; i++) {
                WallObject wallObject = walls[i];
                if (wallObject.traitContainer.GetNormalTrait<Trait>(requiredTrait) != null) {
                    traitables.Add(wallObject);
                }
            }
            if (objHere != null && objHere.traitContainer.GetNormalTrait<Trait>(requiredTrait) != null) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)
                    || (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                    traitables.Add(objHere);
                }
            }
        
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character.traitContainer.GetNormalTrait<Trait>(requiredTrait) != null) {
                    traitables.Add(character);
                }
            }
            return traitables;
        }
        #endregion

        #region Mouse Actions
//         public void OnClickTileActions(PointerEventData.InputButton inputButton) {
//             if (InnerMapManager.Instance.IsMouseOnMarker()) {
//                 return;
//             }
//             if (objHere == null) {
// #if UNITY_EDITOR
//                 if (inputButton == PointerEventData.InputButton.Right) {
//                     UIManager.Instance.poiTestingUI.ShowUI(this);
//                 } else {
//                     Messenger.Broadcast(Signals.HIDE_MENUS);
//                 }
// #else
//             Messenger.Broadcast(Signals.HIDE_MENUS);
// #endif
//             } else if (objHere is TileObject || objHere is SpecialToken) {
// #if UNITY_EDITOR
//                 if (inputButton == PointerEventData.InputButton.Right) {
//                     if (objHere is TileObject) {
//                         UIManager.Instance.poiTestingUI.ShowUI(objHere);
//                     }
//                 } 
//                 //else {
//                 //    if (objHere is TileObject) {
//                 //        UIManager.Instance.ShowTileObjectInfo(objHere as TileObject);
//                 //    }
//                 //}
// #else
//              //if (inputButton == PointerEventData.InputButton.Left) {
//              //   if (objHere is TileObject) {
//              //       UIManager.Instance.ShowTileObjectInfo(objHere as TileObject);
//              //   }
//              //}
// #endif
//             }
//         }
        #endregion

        #region Tile Objects
        public void SetReservedType(TILE_OBJECT_TYPE reservedType) {
            if (structure != null) {
                if (this.reservedObjectType != TILE_OBJECT_TYPE.NONE && reservedType == TILE_OBJECT_TYPE.NONE && tileState == Tile_State.Empty) {
                    structure.AddUnoccupiedTile(this);
                } else if (this.reservedObjectType == TILE_OBJECT_TYPE.NONE && reservedType != TILE_OBJECT_TYPE.NONE) {
                    structure.RemoveUnoccupiedTile(this);
                }
            }
            reservedObjectType = reservedType;
        }
        #endregion

        #region Furniture Spots
        public void SetFurnitureSpot(FurnitureSpot spot) {
            furnitureSpot = spot;
            hasFurnitureSpot = true;
        }
        public FURNITURE_TYPE GetFurnitureThatCanProvide(FACILITY_TYPE facility) {
            List<FURNITURE_TYPE> choices = new List<FURNITURE_TYPE>();
            if (furnitureSpot.allowedFurnitureTypes != null) {
                for (int i = 0; i < furnitureSpot.allowedFurnitureTypes.Length; i++) {
                    FURNITURE_TYPE currType = furnitureSpot.allowedFurnitureTypes[i];
                    if (currType.ConvertFurnitureToTileObject().CanProvideFacility(facility)) {
                        choices.Add(currType);
                    }
                }
                if (choices.Count > 0) {
                    return choices[UnityEngine.Random.Range(0, choices.Count)];
                }
            }
            throw new System.Exception("Furniture spot at " + this.ToString() + " cannot provide facility " + facility.ToString() + "! Should not reach this point if that is the case!");
        }
        #endregion

        #region Building
        public BuildingSpot buildSpotOwner { get; private set; }
        public void SetHasBlueprint(bool hasBlueprint) {
            this.hasBlueprint = hasBlueprint;
        }
        public void SetBuildSpotOwner(BuildingSpot buildSpot) {
            buildSpotOwner = buildSpot;
        }
        #endregion

        #region Walls
        public void AddWallObject(WallObject wallObject) {
            walls.Add(wallObject);
        }
        public void RemoveWallObject(WallObject wallObject) {
            walls.Remove(wallObject);
        }
        public void ClearWallObjects() {
            walls.Clear();
        }
        #endregion
    }

    [System.Serializable]
    public struct TwoTileDirections {
        public GridNeighbourDirection from;
        public GridNeighbourDirection to;

        public TwoTileDirections(GridNeighbourDirection from, GridNeighbourDirection to) {
            this.from = from;
            this.to = to;
        }
    }


    [System.Serializable]
    public class SaveDataLocationGridTile {
        public Vector3Save localPlace; //this is the id
        public Vector3Save worldLocation;
        public Vector3Save centeredWorldLocation;
        public Vector3Save localLocation;
        public Vector3Save centeredLocalLocation;
        public LocationGridTile.Tile_Type tileType;
        public LocationGridTile.Tile_State tileState;
        public LocationGridTile.Ground_Type groundType;
        //public LocationStructure structure { get; private set; }
        //public Dictionary<TileNeighbourDirection, LocationGridTile> neighbours { get; private set; }
        //public List<Vector3Save> neighbours;
        //public List<TileNeighbourDirection> neighbourDirections;
        public List<SaveDataTrait> traits;
        //public List<int> charactersHere;
        public int objHereID;
        public POINT_OF_INTEREST_TYPE objHereType;
        public TILE_OBJECT_TYPE objHereTileObjectType;


        public TILE_OBJECT_TYPE reservedObjectType;
        public FurnitureSpot furnitureSpot;
        public bool hasFurnitureSpot;
        public bool hasDetail;
        public bool isInside;
        public bool isLocked;

        public int structureID;
        public STRUCTURE_TYPE structureType;

        private LocationGridTile loadedGridTile;

        //tilemap assets
        public string groundTileMapAssetName;
        public string roadTileMapAssetName;
        public string wallTileMapAssetName;
        public string detailTileMapAssetName;
        public string structureTileMapAssetName;
        public string objectTileMapAssetName;

        public Matrix4x4 groundTileMapMatrix;
        public Matrix4x4 roadTileMapMatrix;
        public Matrix4x4 wallTileMapMatrix;
        public Matrix4x4 detailTileMapMatrix;
        public Matrix4x4 structureTileMapMatrix;
        public Matrix4x4 objectTileMapMatrix;

        public void Save(LocationGridTile gridTile) {
            localPlace = new Vector3Save(gridTile.localPlace);
            worldLocation = gridTile.worldLocation;
            centeredWorldLocation = gridTile.centeredWorldLocation;
            localLocation = gridTile.localLocation;
            centeredLocalLocation = gridTile.centeredLocalLocation;
            tileType = gridTile.tileType;
            tileState = gridTile.tileState;
            groundType = gridTile.groundType;
            reservedObjectType = gridTile.reservedObjectType;
            furnitureSpot = gridTile.furnitureSpot;
            hasFurnitureSpot = gridTile.hasFurnitureSpot;
            hasDetail = gridTile.hasDetail;
            isInside = gridTile.isInside;
            isLocked = gridTile.isLocked;

            if(gridTile.structure != null) {
                structureID = gridTile.structure.id;
                structureType = gridTile.structure.structureType;
            } else {
                structureID = -1;
            }

            //neighbourDirections = new List<TileNeighbourDirection>();
            //neighbours = new List<Vector3Save>();
            //foreach (KeyValuePair<TileNeighbourDirection, LocationGridTile> kvp in gridTile.neighbours) {
            //    neighbourDirections.Add(kvp.Key);
            //    neighbours.Add(new Vector3Save(kvp.Value.localPlace));
            //}

            traits = new List<SaveDataTrait>();
            for (int i = 0; i < gridTile.normalTraits.Count; i++) {
                SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(gridTile.normalTraits[i]);
                if (saveDataTrait != null) {
                    saveDataTrait.Save(gridTile.normalTraits[i]);
                    traits.Add(saveDataTrait);
                }
            }

            if(gridTile.objHere != null) {
                objHereID = gridTile.objHere.id;
                objHereType = gridTile.objHere.poiType;
                if(gridTile.objHere is TileObject) {
                    objHereTileObjectType = (gridTile.objHere as TileObject).tileObjectType;
                }
            } else {
                objHereID = -1;
            }

            //tilemap assets
            groundTileMapAssetName = gridTile.parentMap.groundTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
            detailTileMapAssetName = gridTile.parentMap.detailsTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;
            structureTileMapAssetName = gridTile.parentMap.structureTilemap.GetTile(gridTile.localPlace)?.name ?? string.Empty;

            groundTileMapMatrix = gridTile.parentMap.groundTilemap.GetTransformMatrix(gridTile.localPlace);
            detailTileMapMatrix = gridTile.parentMap.detailsTilemap.GetTransformMatrix(gridTile.localPlace);
            structureTileMapMatrix = gridTile.parentMap.structureTilemap.GetTransformMatrix(gridTile.localPlace);
        }

        public LocationGridTile Load(Tilemap tilemap, InnerTileMap parentAreaMap, Dictionary<string, TileBase> tileAssetDB) {
            LocationGridTile tile = new LocationGridTile(this, tilemap, parentAreaMap);

            if(structureID != -1) {
                LocationStructure structure = (parentAreaMap.location as Settlement).GetStructureByID(structureType, structureID);
                tile.SetStructure(structure);
            }

            //tile.SetGroundType(groundType);
            if (hasFurnitureSpot) {
                tile.SetFurnitureSpot(furnitureSpot);
            }
            loadedGridTile = tile;

            //load tile assets
            tile.SetGroundTilemapVisual(InnerMapManager.Instance.TryGetTileAsset(groundTileMapAssetName, tileAssetDB));
            tile.parentMap.detailsTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.TryGetTileAsset(detailTileMapAssetName, tileAssetDB));
            tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.TryGetTileAsset(structureTileMapAssetName, tileAssetDB));

            tile.parentMap.groundTilemap.SetTransformMatrix(tile.localPlace, groundTileMapMatrix);
            tile.parentMap.detailsTilemap.SetTransformMatrix(tile.localPlace, detailTileMapMatrix);
            tile.parentMap.structureTilemap.SetTransformMatrix(tile.localPlace, structureTileMapMatrix);

            return tile;
        }

        public void LoadTraits() {
            for (int i = 0; i < traits.Count; i++) {
                Character responsibleCharacter = null;
                Trait trait = traits[i].Load(ref responsibleCharacter);
                loadedGridTile.genericTileObject.traitContainer.AddTrait(loadedGridTile.genericTileObject, trait, responsibleCharacter);
            }
        }

        //This is loaded last so release loadedGridTile here
        public void LoadObjectHere() {
            if(objHereID != -1) {
                if(objHereType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    loadedGridTile.structure.AddPOI(CharacterManager.Instance.GetCharacterByID(objHereID), loadedGridTile);
                }

                //NOTE: Do not load item in grid tile because it is already loaded in LoadAreaItems
                //else if (objHereType == POINT_OF_INTEREST_TYPE.ITEM) {
                //    loadedGridTile.structure.AddPOI(TokenManager.Instance.GetSpecialTokenByID(objHereID), loadedGridTile);
                //}
                else if (objHereType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject obj = InnerMapManager.Instance.GetTileObject(objHereTileObjectType, objHereID);
                    if (obj == null) {
                        throw new System.Exception("Could not find object of type " + objHereTileObjectType.ToString() + " with id " + objHereID.ToString() + " at " + loadedGridTile.structure.ToString());
                    }
                    loadedGridTile.structure.AddPOI(obj, loadedGridTile);
                }
            }
            //loadedGridTile = null;
        }
    }
}