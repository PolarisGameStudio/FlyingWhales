﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using PathFind;
using Pathfinding;
using Scriptable_Object_Scripts;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Inner_Maps {
    public class LocationGridTile : IHasNeighbours<LocationGridTile> {

        public enum Tile_Type { Empty, Wall }
        public enum Tile_State { Empty, Occupied }
        public enum Ground_Type { Soil, Grass, Stone, Snow, Tundra, Cobble, Wood, Snow_Dirt, Water, Cave, Corrupted, 
            Desert_Grass, Sand, Desert_Stone, Bone, Demon_Stone, Flesh, Structure_Stone,
            Ruined_Stone
        }
        public InnerTileMap parentMap { get; }
        public Tilemap parentTileMap { get; }
        public Vector3Int localPlace { get; }
        public Vector3 worldLocation { get; private set; }
        public Vector3 centeredWorldLocation { get; private set; }
        public Vector3 localLocation { get; }
        public Vector3 centeredLocalLocation { get; }
        public Tile_Type tileType { get; private set; }
        public Tile_State tileState { get; private set; }
        public Ground_Type groundType { get; private set; }
        public LocationStructure structure { get; private set; }
        private Dictionary<GridNeighbourDirection, LocationGridTile> neighbours { get; set; }
        private Dictionary<GridNeighbourDirection, LocationGridTile> fourNeighbours { get; set; }
        public List<LocationGridTile> neighbourList { get; private set; }
        public IPointOfInterest objHere { get; private set; }
        public List<Character> charactersHere { get; }
        public bool hasBlueprint { get; private set; }
        public GenericTileObject genericTileObject { get; private set; }
        public List<StructureWallObject> walls { get; }
        public LocationGridTileCollection collectionOwner { get; private set; }
        public bool isCorrupted => groundType == Ground_Type.Corrupted;
        public bool hasLandmine { get; private set; }
        public bool hasFreezingTrap { get; private set; }
        public bool hasSnareTrap { get; private set; }
        /// <summary>
        /// The generated perlin noise sample of this tile.
        /// </summary>
        public float floorSample { get; private set; }

        private GameObject _landmineEffect;
        private GameObject _freezingTrapEffect;
        private GameObject _snareTrapEffect;

        private TrapChecker _freezingTrapChecker;

        #region getters
        public bool isOccupied => tileState == Tile_State.Occupied;
        public List<Trait> normalTraits => genericTileObject.traitContainer.allTraitsAndStatuses;
        #endregion
        
        #region Pathfinding
        public List<LocationGridTile> ValidTiles { get { return FourNeighbours().Where(o => o.tileType == Tile_Type.Empty).ToList(); } }
        public List<LocationGridTile> CaveInterconnectionTiles { get { return FourNeighbours().Where(o => o.structure == structure && !o.HasDifferentStructureNeighbour()).ToList() ; } } //
        public List<LocationGridTile> UnoccupiedNeighbours { get { return neighbours.Values.Where(o => !o.isOccupied && o.structure == structure).ToList(); } }
        public List<LocationGridTile> UnoccupiedNeighboursWithinHex {
            get {
                return neighbours.Values.Where(o =>
                        !o.isOccupied && o.charactersHere.Count <= 0 && o.structure == structure &&
                        o.collectionOwner.isPartOfParentRegionMap &&
                        o.collectionOwner.partOfHextile.hexTileOwner == collectionOwner.partOfHextile.hexTileOwner)
                    .ToList();
            }
        }
        #endregion
        
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
            walls = new List<StructureWallObject>();
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
            charactersHere = new List<Character>();
            walls = new List<StructureWallObject>();
        }

        #region Other Data
        private Dictionary<GridNeighbourDirection, Point> possibleExits =>
            new Dictionary<GridNeighbourDirection, Point>() {
                {GridNeighbourDirection.North, new Point(0,1) },
                {GridNeighbourDirection.South, new Point(0,-1) },
                {GridNeighbourDirection.West, new Point(-1,0) },
                {GridNeighbourDirection.East, new Point(1,0) },
                {GridNeighbourDirection.North_West, new Point(-1,1) },
                {GridNeighbourDirection.North_East, new Point(1,1) },
                {GridNeighbourDirection.South_West, new Point(-1,-1) },
                {GridNeighbourDirection.South_East, new Point(1,-1) },
            };
        public void SetTileType(Tile_Type tileType) {
            this.tileType = tileType;
        }
        public void CreateGenericTileObject() {
            genericTileObject = new GenericTileObject(this);
        }
        public void SetCollectionOwner(LocationGridTileCollection _collectionOwner) {
            collectionOwner = _collectionOwner;
        }
        private void SetGroundType(Ground_Type newGroundType) {
            Ground_Type previousType = this.groundType;
            this.groundType = newGroundType;
            if (genericTileObject != null) {
                switch (newGroundType) {
                    case Ground_Type.Grass:
                    case Ground_Type.Wood:
                    case Ground_Type.Sand:
                    case Ground_Type.Desert_Grass:
                    case Ground_Type.Structure_Stone:
                        genericTileObject.traitContainer.AddTrait(genericTileObject, "Flammable");
                        break;
                    case Ground_Type.Snow:
                        genericTileObject.traitContainer.AddTrait(genericTileObject, "Frozen", bypassElementalChance: true, overrideDuration: 0);
                        genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                        break;
                    default:
                        genericTileObject.traitContainer.RemoveTrait(genericTileObject, "Flammable");
                        break;
                }
            }
            //if snow ground was set to tundra, schedule snow to grow back after a few hours
            if (GameManager.Instance.gameHasStarted && previousType == Ground_Type.Snow && newGroundType == Ground_Type.Tundra) {
                GameDate dueDate = GameManager.Instance.Today();
                dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4)));
                SchedulingManager.Instance.AddEntry(dueDate,
                    () => SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.snowTile, true), this);
            }
        }
        public void UpdateWorldLocation() {
            worldLocation = parentTileMap.CellToWorld(localPlace);
            centeredWorldLocation = new Vector3(worldLocation.x + 0.5f, worldLocation.y + 0.5f, worldLocation.z);
        }
        public List<LocationGridTile> FourNeighbours() {
            List<LocationGridTile> fn = new List<LocationGridTile>();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in fourNeighbours) {
                fn.Add(keyValuePair.Value);
            }
            return fn;
        }
        private Dictionary<GridNeighbourDirection, LocationGridTile> FourNeighboursDictionary() { return fourNeighbours; }
        public void FindNeighbours(LocationGridTile[,] map) {
            fourNeighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbours = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            neighbourList = new List<LocationGridTile>();
            int mapUpperBoundX = map.GetUpperBound(0);
            int mapUpperBoundY = map.GetUpperBound(1);
            Point thisPoint = new Point(localPlace.x, localPlace.y);
            foreach (KeyValuePair<GridNeighbourDirection, Point> kvp in possibleExits) {
                GridNeighbourDirection currDir = kvp.Key;
                Point exit = kvp.Value;
                Point result = exit.Sum(thisPoint);
                if (UtilityScripts.Utilities.IsInRange(result.X, 0, mapUpperBoundX + 1) &&
                    UtilityScripts.Utilities.IsInRange(result.Y, 0, mapUpperBoundY + 1)) {
                    LocationGridTile tile = map[result.X, result.Y];
                    neighbours.Add(currDir, tile);
                    neighbourList.Add(tile);
                    if (currDir.IsCardinalDirection()) {
                        fourNeighbours.Add(currDir, tile);
                    }
                }

            }
        }
        #endregion
        
        #region Visuals
        public void SetFloorSample(float floorSample) {
            this.floorSample = floorSample;
        }
        public void UpdateGroundTypeBasedOnAsset() {
            Sprite groundAsset = parentMap.groundTilemap.GetSprite(localPlace);
            Sprite structureAsset = parentMap.structureTilemap.GetSprite(localPlace);
            if (ReferenceEquals(structureAsset, null) == false) {
                string assetName = structureAsset.name.ToLower();
                if (assetName.Contains("dungeon") || assetName.Contains("cave") || assetName.Contains("laid")) {
                    SetGroundType(Ground_Type.Cave);
                } else if (assetName.Contains("water") || assetName.Contains("pond") || assetName.Contains("shore")) {
                    SetGroundType(Ground_Type.Water);
                } 
            } else if (ReferenceEquals(groundAsset, null) == false) {
                string assetName = groundAsset.name.ToLower();
                if (assetName.Contains("desert")) {
                    if (assetName.Contains("grass")) {
                        SetGroundType(Ground_Type.Desert_Grass);
                    } else if (assetName.Contains("sand")) {
                        SetGroundType(Ground_Type.Sand);
                    } else if (assetName.Contains("rocks")) {
                        SetGroundType(Ground_Type.Desert_Stone);
                    }
                } else if (assetName.Contains("corruption") || assetName.Contains("corrupted")) {
                    SetGroundType(Ground_Type.Corrupted);
                } else if (assetName.Contains("bone")) {
                    SetGroundType(Ground_Type.Bone);
                } else if (assetName.Contains("structure floor") || assetName.Contains("wood")) {
                    SetGroundType(Ground_Type.Wood);
                } else if (assetName.Contains("cobble")) {
                    SetGroundType(Ground_Type.Cobble);
                } else if (assetName.Contains("water") || assetName.Contains("pond")) {
                    SetGroundType(Ground_Type.Water);
                } else if (assetName.Contains("dirt") || assetName.Contains("soil") || assetName.Contains("outside") || assetName.Contains("snow")) {
                    BIOMES biomeType = parentMap.region.coreTile.biomeType;
                    if (collectionOwner.isPartOfParentRegionMap) {
                        biomeType = collectionOwner.partOfHextile.hexTileOwner.biomeType;
                    }
                    if (biomeType == BIOMES.SNOW || biomeType == BIOMES.TUNDRA) {
                        if (assetName.Contains("dirtsnow")) {
                            SetGroundType(Ground_Type.Snow_Dirt);
                        } else if (assetName.Contains("snow")) {
                            SetGroundType(Ground_Type.Snow);
                        } else {
                            SetGroundType(Ground_Type.Tundra);
                            //override tile to use tundra soil
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);    
                        }
                    } else if (biomeType == BIOMES.DESERT) {
                        if (structure != null && (structure.structureType == STRUCTURE_TYPE.CAVE || structure.structureType == STRUCTURE_TYPE.MONSTER_LAIR)) {
                            SetGroundType(Ground_Type.Stone);
                            //override tile to use stone
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.stoneTile);    
                        } else {
                            SetGroundType(Ground_Type.Sand);
                            //override tile to use sand
                            parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.desertSandTile);
                        }
                        
                    } else {
                        SetGroundType(Ground_Type.Soil);
                    }
                } else if (assetName.Contains("stone") || assetName.Contains("road")) {
                    if (assetName.Contains("demon")) {
                        SetGroundType(Ground_Type.Demon_Stone);   
                    } else if (assetName.Contains("floor")) {
                        SetGroundType(Ground_Type.Structure_Stone);
                    } else {
                        SetGroundType(Ground_Type.Stone);    
                    }
                } else if (assetName.Contains("ruins")) {
                    SetGroundType(Ground_Type.Ruined_Stone);
                } else if (assetName.Contains("grass")) {
                    SetGroundType(Ground_Type.Grass);
                } else if (assetName.Contains("tundra")) {
                    SetGroundType(Ground_Type.Tundra);
                    //override tile to use tundra soil
                    parentMap.groundTilemap.SetTile(localPlace, InnerMapManager.Instance.assetManager.tundraTile);
                } else if (assetName.Contains("flesh")) {
                    SetGroundType(Ground_Type.Flesh);
                }
            }
        }
        public TileBase previousGroundVisual { get; private set; }
        public void SetGroundTilemapVisual(TileBase tileBase, bool updateEdges = false) {
            SetPreviousGroundVisual(parentMap.groundTilemap.GetTile(localPlace));
            parentMap.groundTilemap.SetTile(localPlace, tileBase);
            if (genericTileObject.mapObjectVisual != null && genericTileObject.mapObjectVisual.usedSprite != null) {
                //if this tile's map object is shown and is showing a visual, update it's sprite to use the updated sprite.
                genericTileObject.mapObjectVisual.SetVisual(parentMap.groundTilemap.GetSprite(localPlace));
            }
            UpdateGroundTypeBasedOnAsset();
            if (updateEdges) {
                CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
        public void SetStructureTilemapVisual(TileBase tileBase) {
            parentMap.structureTilemap.SetTile(localPlace, tileBase);
            UpdateGroundTypeBasedOnAsset();
        }
        public void SetPreviousGroundVisual(TileBase tileBase) {
            previousGroundVisual = tileBase;
        }
        public void RevertToPreviousGroundVisual() {
            if (previousGroundVisual != null) {
                SetGroundTilemapVisual(previousGroundVisual);
            }
            CreateSeamlessEdgesForSelfAndNeighbours();
        }
        public void CreateSeamlessEdgesForSelfAndNeighbours() {
            CreateSeamlessEdgesForTile(parentMap);
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile neighbour = neighbourList[i];
                neighbour.CreateSeamlessEdgesForTile(parentMap);
            }
        }
        public void CreateSeamlessEdgesForTile(InnerTileMap map) {
            // string summary = $"Creating seamless edges for tile {ToString()}";
            Dictionary<GridNeighbourDirection, LocationGridTile> neighbours;
            if (HasCardinalNeighbourOfDifferentGroundType(out neighbours)) {
                // summary += $"\nHas Neighbour of different ground type. Checking neighbours {neighbours.Count.ToString()}";
                BIOMES thisBiome = GetBiomeOfGroundType(groundType);
                Dictionary<GridNeighbourDirection, LocationGridTile> fourNeighbours = FourNeighboursDictionary();
                foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in fourNeighbours) {
                    LocationGridTile currNeighbour = keyValuePair.Value;
                    bool createEdge = false;
                    BIOMES otherBiome = GetBiomeOfGroundType(currNeighbour.groundType);
                    if (thisBiome != otherBiome && thisBiome != BIOMES.NONE && otherBiome != BIOMES.NONE) {
                        if (thisBiome == BIOMES.SNOW) {
                            createEdge = true;
                        } else if (thisBiome == BIOMES.GRASSLAND && otherBiome == BIOMES.DESERT) {
                            createEdge = true;
                        }
                    } else {
                        // summary += $"\n\tChecking {currNeighbour.ToString()}. Ground type is {groundType.ToString()}. Neighbour Ground Type is {currNeighbour.groundType.ToString()}";
                        if (this.groundType != Ground_Type.Cave && groundType != Ground_Type.Structure_Stone && currNeighbour.groundType == Ground_Type.Cave) {
                            createEdge = true;
                        } else if (currNeighbour.tileType == Tile_Type.Wall) {
                            createEdge = false;
                        } else if (groundType != Ground_Type.Water && currNeighbour.groundType == Ground_Type.Water) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Corrupted && currNeighbour.groundType != Ground_Type.Bone && currNeighbour.groundType != Ground_Type.Corrupted) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Demon_Stone && currNeighbour.groundType != Ground_Type.Corrupted && currNeighbour.groundType != Ground_Type.Demon_Stone && currNeighbour.groundType != Ground_Type.Bone) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Bone && (currNeighbour.groundType == Ground_Type.Corrupted || currNeighbour.groundType == Ground_Type.Demon_Stone)) {
                            createEdge = true;
                        } else if (groundType != Ground_Type.Corrupted && currNeighbour.groundType == Ground_Type.Corrupted) {
                            createEdge = false;
                        } else if (groundType == Ground_Type.Snow && currNeighbour.groundType != Ground_Type.Snow) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Cobble && currNeighbour.groundType != Ground_Type.Snow) {
                            createEdge = true;
                        } else if ((groundType == Ground_Type.Tundra || groundType == Ground_Type.Snow_Dirt) &&
                                   (currNeighbour.groundType == Ground_Type.Stone || currNeighbour.groundType == Ground_Type.Soil)) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Grass && currNeighbour.groundType == Ground_Type.Soil) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Soil && currNeighbour.groundType == Ground_Type.Stone) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Stone && currNeighbour.groundType == Ground_Type.Grass) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Desert_Grass &&
                                   (currNeighbour.groundType == Ground_Type.Desert_Stone || currNeighbour.groundType == Ground_Type.Sand)) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Sand && currNeighbour.groundType == Ground_Type.Desert_Stone) {
                            createEdge = true;
                        } else if (groundType == Ground_Type.Sand && currNeighbour.groundType == Ground_Type.Stone) {
                            createEdge = true;
                        } else if ((groundType != Ground_Type.Ruined_Stone && groundType != Ground_Type.Structure_Stone) 
                                   && currNeighbour.groundType == Ground_Type.Ruined_Stone) {
                            createEdge = true;
                        }
                    }
                    
                    // summary += $"\n\tWill create edge? {createEdge.ToString()}. At {keyValuePair.Key.ToString()}";
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
                    Assert.IsNotNull(mapToUse, $"{nameof(mapToUse)} != null");
                    if (createEdge) {
                        if (InnerMapManager.Instance.assetManager.edgeAssets.ContainsKey(groundType) && 
                            InnerMapManager.Instance.assetManager.edgeAssets[groundType].Count > (int)keyValuePair.Key) {
                            mapToUse.SetTile(localPlace, InnerMapManager.Instance.assetManager.edgeAssets[groundType][(int)keyValuePair.Key]);    
                        }
                    } else {
                        mapToUse.SetTile(localPlace, null);
                    }
                }
                // Debug.Log(summary);    
            }
            else {
                map.northEdgeTilemap.SetTile(localPlace, null);
                map.southEdgeTilemap.SetTile(localPlace, null);
                map.westEdgeTilemap.SetTile(localPlace, null);
                map.eastEdgeTilemap.SetTile(localPlace, null);
            }
        }
        private BIOMES GetBiomeOfGroundType(Ground_Type groundType) {
            switch (groundType) {
                case Ground_Type.Sand:
                case Ground_Type.Desert_Grass:
                case Ground_Type.Desert_Stone:
                    return BIOMES.DESERT;
                case Ground_Type.Snow:
                case Ground_Type.Snow_Dirt:
                case Ground_Type.Tundra:
                    return BIOMES.SNOW;
                case Ground_Type.Soil:
                case Ground_Type.Grass:
                    return BIOMES.GRASSLAND;
                default:
                    return BIOMES.NONE;
            }
        }
        /// <summary>
        /// Set this tile to the ground that it originally was, aka before anything was put on it.
        /// </summary>
        public void RevertTileToOriginalPerlin() {
             TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(floorSample, parentMap.region.coreTile.biomeType);
             SetGroundTilemapVisual(groundTile);
             SetPreviousGroundVisual(null);
        }
        public void DetermineNextGroundTypeAfterDestruction() {
            TileBase nextGroundAsset;
            switch (groundType) {
                case Ground_Type.Grass:
                case Ground_Type.Stone:
                case Ground_Type.Water:
                    //if grass or stone or water revert to soil 
                    nextGroundAsset = InnerMapManager.Instance.assetManager.soilTile;
                    break;
                case Ground_Type.Snow:
                case Ground_Type.Snow_Dirt:
                    //if snow revert to tundra
                    nextGroundAsset = InnerMapManager.Instance.assetManager.tundraTile;
                    break;
                case Ground_Type.Cobble:
                case Ground_Type.Wood:
                case Ground_Type.Structure_Stone:
                case Ground_Type.Ruined_Stone:
                case Ground_Type.Demon_Stone:
                case Ground_Type.Flesh:
                case Ground_Type.Cave:
                case Ground_Type.Corrupted:
                case Ground_Type.Bone:
                    //if from structure, revert to original ground asset
                    nextGroundAsset = InnerTileMap.GetGroundAssetPerlin(floorSample, parentMap.region.coreTile.biomeType);
                    break;
                case Ground_Type.Desert_Grass:
                case Ground_Type.Sand:
                    //if desert grass or sand, revert to desert stone
                    nextGroundAsset = InnerMapManager.Instance.assetManager.desertStoneGroundTile;
                    break;
                default:
                    nextGroundAsset = null;
                    break;
            }
            if (nextGroundAsset != null) {
                SetGroundTilemapVisual(nextGroundAsset);
                CreateSeamlessEdgesForSelfAndNeighbours();
            }
        }
        #endregion

        #region Structures
        public void SetStructure(LocationStructure structure) {
            this.structure?.RemoveTile(this);
            this.structure = structure;
            this.structure.AddTile(this);
            genericTileObject.ManualInitialize(this);
        }
        public void SetTileState(Tile_State state) {
            if (structure != null) {
                if (tileState == Tile_State.Empty && state == Tile_State.Occupied) {
                    structure.RemoveUnoccupiedTile(this);
                } else if (tileState == Tile_State.Occupied && state == Tile_State.Empty) { //&& reservedObjectType == TILE_OBJECT_TYPE.NONE
                    structure.AddUnoccupiedTile(this);
                }
            }
            tileState = state;
        }
        #endregion

        #region Characters
        public void AddCharacterHere(Character character) {
            // if (!charactersHere.Contains(character)) {
                charactersHere.Add(character);
            // }
            if(genericTileObject != null) {
                List<Trait> traitOverrideFunctions = genericTileObject.traitContainer.GetTraitOverrideFunctions(TraitManager.Enter_Grid_Tile_Trait);
                if (traitOverrideFunctions != null) {
                    for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                        Trait trait = traitOverrideFunctions[i];
                        trait.OnEnterGridTile(character, genericTileObject);
                    }
                }
            }
            if (hasLandmine) {
                GameManager.Instance.StartCoroutine(TriggerLandmine(character));
            }
            if (hasFreezingTrap && (_freezingTrapChecker == null || _freezingTrapChecker.CanTrapAffectCharacter(character))) {
                TriggerFreezingTrap(character);
            }
            if (hasSnareTrap) {
                TriggerSnareTrap(character);
            }
            if (isCorrupted) {
                //Reporting does not trigger until Tutorial is over
                //https://trello.com/c/OmmyR6go/1239-reporting-does-not-trigger-until-tutorial-is-over

                LocationStructure mostImportantStructureOnTile =
                    collectionOwner.partOfHextile.hexTileOwner.GetMostImportantStructureOnTile();
                if(mostImportantStructureOnTile is DemonicStructure demonicStructure) {
                    if (!character.behaviourComponent.isAttackingDemonicStructure
                       && character.homeSettlement != null
                       && character.necromancerTrait == null
                       && (character.race == RACE.HUMANS || character.race == RACE.ELVES)
                       && character.marker != null && character.carryComponent.IsNotBeingCarried()
                       && character.isAlliedWithPlayer == false
                       //&& !InnerMapManager.Instance.HasWorldKnownDemonicStructure(mostImportantStructureOnTile)
                       && (Tutorial.TutorialManager.Instance.hasCompletedImportantTutorials || WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial)) {
                        if (character.faction != null && character.faction.isMajorNonPlayer && !character.faction.HasActiveParty(PARTY_TYPE.Counterattack) && !character.faction.HasActiveReportDemonicStructureJob(mostImportantStructureOnTile)) {
                            character.jobComponent.CreateReportDemonicStructure(mostImportantStructureOnTile);
                            return;
                        }
                    }
                    //If cannot report flee instead
                    //do not make characters that are allied with the player or attacking a demonic structure flee from corruption.
                    if (!character.behaviourComponent.isAttackingDemonicStructure && (!character.partyComponent.hasParty ||
                        character.partyComponent.currentParty.partyType != PARTY_TYPE.Counterattack) && character.isAlliedWithPlayer == false &&
                        character.necromancerTrait == null) {
                        if (!character.movementComponent.hasMovedOnCorruption) {
                            character.movementComponent.SetHasMovedOnCorruption(true);
                            if (character.isNormalCharacter) {
                                genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                                //if (character.characterClass.IsCombatant()) {
                                //    character.behaviourComponent.SetIsAttackingDemonicStructure(true, demonicStructure);
                                //} else {
                                //    genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
                                //}
                            }
                        }
                    }
                }
            } else {
                character.movementComponent.SetHasMovedOnCorruption(false);
            }
        }
        public void RemoveCharacterHere(Character character) {
            charactersHere.Remove(character);
        }
        #endregion

        #region Points of Interest
        public void SetObjectHere(IPointOfInterest poi) {
            bool isPassablePreviously = IsPassable();
            if (poi is TileObject tileObject) {
                if (tileObject.OccupiesTile()) {
                    objHere = poi;
                }
            } else {
                objHere = poi;    
            }
            
            poi.SetGridTileLocation(this);
            poi.OnPlacePOI();
            SetTileState(Tile_State.Occupied);
            if (!IsPassable()) {
                structure.RemovePassableTile(this);
            } else if (IsPassable() && !isPassablePreviously) {
                structure.AddPassableTile(this);
            }
            Messenger.Broadcast(Signals.OBJECT_PLACED_ON_TILE, this, poi);
        }
        public IPointOfInterest RemoveObjectHere(Character removedBy) {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                objHere = null;
                if (removedObj is TileObject tileObject) {
                    //if the object in this tile is a tile object and it was removed by a character, use tile object specific function
                    tileObject.RemoveTileObject(removedBy);
                } else {
                    removedObj.SetGridTileLocation(null);
                    removedObj.OnDestroyPOI();
                }
                SetTileState(Tile_State.Empty);
                Messenger.Broadcast(Signals.STOP_CURRENT_ACTION_TARGETING_POI, removedObj);
                return removedObj;
            }
            return null;
        }
        public IPointOfInterest RemoveObjectHereWithoutDestroying() {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                objHere = null;
                SetTileState(Tile_State.Empty);
                if (removedObj.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    (removedObj as TileObject).OnRemoveTileObject(null, gridTile, false, false);
                }
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                return removedObj;
            }
            return null;
        }
        public IPointOfInterest RemoveObjectHereDestroyVisualOnly(Character remover = null) {
            if (objHere != null) {
                IPointOfInterest removedObj = objHere;
                LocationGridTile gridTile = objHere.gridTileLocation;
                objHere.SetGridTileLocation(null);
                objHere = null;
                SetTileState(Tile_State.Empty);
                if (removedObj.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject removedTileObj = removedObj as TileObject;
                    removedTileObj.OnRemoveTileObject(null, gridTile, false, false);
                    removedTileObj.DestroyMapVisualGameObject();
                }
                removedObj.SetPOIState(POI_STATE.INACTIVE);
                Messenger.Broadcast(Signals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, removedObj, remover);
                return removedObj;
            }
            return null;
        }
        #endregion

        #region Utilities
        public LocationGridTile GetNeighbourAtDirection(GridNeighbourDirection dir) {
            if (neighbours.ContainsKey(dir)) {
                return neighbours[dir];
            }
            return null;
        }
        public List<LocationGridTile> GetCrossNeighbours() {
            List<LocationGridTile> crossNeighbours = new List<LocationGridTile>();
            if (neighbours.ContainsKey(GridNeighbourDirection.North)) {
                crossNeighbours.Add(neighbours[GridNeighbourDirection.North]);
            }
            if (neighbours.ContainsKey(GridNeighbourDirection.South)) {
                crossNeighbours.Add(neighbours[GridNeighbourDirection.South]);
            }
            if (neighbours.ContainsKey(GridNeighbourDirection.East)) {
                crossNeighbours.Add(neighbours[GridNeighbourDirection.East]);
            }
            if (neighbours.ContainsKey(GridNeighbourDirection.West)) {
                crossNeighbours.Add(neighbours[GridNeighbourDirection.West]);
            }
            return crossNeighbours;
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
        public bool IsAtEdgeOfMap() {
            GridNeighbourDirection[] dirs = CollectionUtilities.GetEnumValues<GridNeighbourDirection>();
            for (int i = 0; i < dirs.Length; i++) {
                if (!neighbours.ContainsKey(dirs[i])) {
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
                if (kvp.Value.structure != structure) {
                    return true;
                }
            }
            return false;
        }
        public override string ToString() {
            return localPlace.ToString();
        }
        public float GetDistanceTo(LocationGridTile tile) {
            return Vector2.Distance(localLocation, tile.localLocation);
        }
        public bool HasOccupiedNeighbour() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.isOccupied) {
                    return true;
                }
            }
            return false;
        }
        public bool HasUnoccupiedNeighbour(out List<LocationGridTile> unoccupiedTiles, bool sameStructure = false) {
            bool hasUnoccupied = false;
            unoccupiedTiles = new List<LocationGridTile>();
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.isOccupied == false) {
                    if (sameStructure) {
                        //if same structure switch is on, check if the neighbour is at the same structure
                        //as this tile before adding to list
                        if (tile.structure != structure) {
                            continue; //skip neighbour
                        }
                    }
                    unoccupiedTiles.Add(tile);
                    hasUnoccupied = true;
                }
            }
            return hasUnoccupied;
        }
        public bool HasNeighbourOfElevation(ELEVATION elevation, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).collectionOwner.partOfHextile.hexTileOwner &&
                    neighbours.Values.ElementAt(i).collectionOwner.partOfHextile.hexTileOwner.elevationType == elevation) {
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
        public int GetCountNeighboursOfType(Tile_Type type, bool useFourNeighbours = false) {
            int count = 0;
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).tileType == type) {
                    count++;
                }
            }
            return count;
        }
        public bool HasNeighbourOfType(Ground_Type type, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).groundType == type) {
                    return true;
                }
            }
            return false;
        }
        public bool HasNeighbourNotInList(List<LocationGridTile> list, bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (list.Contains(neighbours.Values.ElementAt(i)) == false) {
                    return true;
                }
            }
            return false;
        }
        public bool HasDifferentStructureNeighbour(bool useFourNeighbours = false) {
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                LocationGridTile tile = n.Values.ElementAt(i);
                if (tile.structure != structure) {
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
        public bool IsAdjacentTo(Type type) {
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
                if (structure != null) {
                    LocationGridTile nearestTile = null;
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.unoccupiedTiles.Count; i++) {
                        LocationGridTile currTile = structure.unoccupiedTiles.ElementAt(i);
                        if (currTile != this && currTile.groundType != Ground_Type.Water) {
                            float dist = Vector2.Distance(currTile.localLocation, localLocation);
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
        public LocationStructure GetNearestInteriorStructureFromThis() {
            LocationStructure nearestStructure = null;
            if (structure != null) {
                if (structure.location.allStructures.Count > 0) {
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.location.allStructures.Count; i++) {
                        LocationStructure currStructure = structure.location.allStructures[i];
                        if (currStructure != structure && currStructure.isInterior) {
                            LocationGridTile randomPassableTile = currStructure.GetRandomPassableTile();
                            if (randomPassableTile != null && PathfindingManager.Instance.HasPath(this, randomPassableTile)) {
                                float dist = Vector2.Distance(randomPassableTile.localLocation, localLocation);
                                if (nearestStructure == null || dist < nearestDist) {
                                    nearestStructure = currStructure;
                                    nearestDist = dist;
                                }
                            }
                        }
                    }
                }
            }
            return nearestStructure;
        }
        public LocationStructure GetNearestInteriorStructureFromThisExcept(List<LocationStructure> exclusions) {
            LocationStructure nearestStructure = null;
            if (structure != null) {
                if (structure.location.allStructures.Count > 0) {
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.location.allStructures.Count; i++) {
                        LocationStructure currStructure = structure.location.allStructures[i];
                        if (currStructure != structure && currStructure.isInterior) {
                            if (exclusions != null && exclusions.Contains(currStructure)) {
                                continue;
                            }
                            LocationGridTile randomPassableTile = currStructure.GetRandomPassableTile();
                            if (randomPassableTile != null && PathfindingManager.Instance.HasPath(this, randomPassableTile)) {
                                float dist = Vector2.Distance(randomPassableTile.localLocation, localLocation);
                                if (nearestStructure == null || dist < nearestDist) {
                                    nearestStructure = currStructure;
                                    nearestDist = dist;
                                }
                            }
                        }
                    }
                }
            }
            return nearestStructure;
        }
        public LocationStructure GetNearestVillageStructureFromThisWithResidents(Character relativeTo = null) {
            LocationStructure nearestStructure = null;
            if (structure != null) {
                if (structure.location.allStructures.Count > 0) {
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.location.allStructures.Count; i++) {
                        LocationStructure currStructure = structure.location.allStructures[i];
                        if (currStructure != structure && currStructure.settlementLocation != null && !(currStructure.settlementLocation is PlayerSettlement)
                            && currStructure.settlementLocation.owner != null && currStructure.settlementLocation.residents.Count > 0) {
                            if (currStructure.settlementLocation.owner.isMajorNonPlayer) {
                                LocationGridTile randomPassableTile = currStructure.GetRandomPassableTile();
                                if (randomPassableTile != null && ((relativeTo != null && relativeTo.movementComponent.HasPathTo(randomPassableTile)) || PathfindingManager.Instance.HasPath(this, randomPassableTile))) {
                                    float dist = Vector2.Distance(randomPassableTile.localLocation, localLocation);
                                    if (nearestStructure == null || dist < nearestDist) {
                                        nearestStructure = currStructure;
                                        nearestDist = dist;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return nearestStructure;
        }
        public LocationGridTile GetNearestUnoccupiedTileFromThisWithStructure(STRUCTURE_TYPE structureType) {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                if (structure != null) {
                    LocationGridTile nearestTile = null;
                    float nearestDist = 99999f;
                    for (int i = 0; i < structure.unoccupiedTiles.Count; i++) {
                        LocationGridTile currTile = structure.unoccupiedTiles.ElementAt(i);
                        if (currTile != this && currTile.groundType != Ground_Type.Water && currTile.structure != null && currTile.structure.structureType == structureType) {
                            float dist = Vector2.Distance(currTile.localLocation, localLocation);
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
        public LocationGridTile GetNearestEdgeTileFromThis() {
            if (IsAtEdgeOfWalkableMap() && structure != null) {
                return this;
            }

            LocationGridTile nearestEdgeTile = null;
            List<LocationGridTile> neighbours = neighbourList;
            for (int i = 0; i < neighbours.Count; i++) {
                if (neighbours[i].IsAtEdgeOfWalkableMap() && neighbours[i].structure != null) {
                    nearestEdgeTile = neighbours[i];
                    break;
                }
            }
            if (nearestEdgeTile == null) {
                float nearestDist = -999f;
                for (int i = 0; i < parentMap.allEdgeTiles.Count; i++) {
                    LocationGridTile currTile = parentMap.allEdgeTiles[i];
                    float dist = Vector2.Distance(currTile.localLocation, localLocation);
                    if (nearestDist == -999f || dist < nearestDist) {
                        if (currTile.structure != null) {
                            nearestEdgeTile = currTile;
                            nearestDist = dist;
                        }
                    }
                }
            }
            return nearestEdgeTile;
        }
        public LocationGridTile GetRandomUnoccupiedNeighbor() {
            List<LocationGridTile> unoccupiedNeighbours = UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count > 0) {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
            return null;
        }
        public LocationGridTile GetRandomNeighbor() {
            return neighbourList[Random.Range(0, neighbourList.Count)];
        }
        public bool IsAtEdgeOfWalkableMap() {
            if ((localPlace.y == InnerTileMap.SouthEdge && localPlace.x >= InnerTileMap.WestEdge && localPlace.x <= parentMap.width - InnerTileMap.EastEdge - 1)
                || (localPlace.y == parentMap.height - InnerTileMap.NorthEdge - 1 && localPlace.x >= InnerTileMap.WestEdge && localPlace.x <= parentMap.width - InnerTileMap.EastEdge - 1)
                || (localPlace.x == InnerTileMap.WestEdge && localPlace.y >= InnerTileMap.SouthEdge && localPlace.y <= parentMap.height - InnerTileMap.NorthEdge - 1) 
                || (localPlace.x == parentMap.width - InnerTileMap.EastEdge - 1 && localPlace.y >= InnerTileMap.SouthEdge && localPlace.y <= parentMap.height - InnerTileMap.NorthEdge - 1)) {
                return true;
            }
            return false;
        }
        private bool HasCardinalNeighbourOfDifferentGroundType(out Dictionary<GridNeighbourDirection, LocationGridTile> differentTiles) {
            bool hasDiff = false;
            differentTiles = new Dictionary<GridNeighbourDirection, LocationGridTile>();
            Dictionary<GridNeighbourDirection, LocationGridTile> cardinalNeighbours = FourNeighboursDictionary();
            foreach (KeyValuePair<GridNeighbourDirection, LocationGridTile> keyValuePair in cardinalNeighbours) {
                if (keyValuePair.Value.groundType != groundType) {
                    hasDiff = true;
                    differentTiles.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }
            return hasDiff;
        }
        public List<ITraitable> GetTraitablesOnTile() {
            List<ITraitable> traitables = new List<ITraitable>();
            traitables.Add(genericTileObject);
            for (int i = 0; i < walls.Count; i++) {
                StructureWallObject structureWallObject = walls[i];
                traitables.Add(structureWallObject);
            }
            if (objHere != null) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)) {//|| (objHere is SpecialToken && (objHere as SpecialToken).mapObjectState == MAP_OBJECT_STATE.BUILT)
                    traitables.Add(objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                traitables.Add(character);
            }
            return traitables;
        }
        public void PerformActionOnTraitables(TraitableCallback callback) {
            callback.Invoke(genericTileObject);
            for (int i = 0; i < walls.Count; i++) {
                StructureWallObject structureWallObject = walls[i];
                callback.Invoke(structureWallObject);
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                callback.Invoke(character);
            }
            if (objHere is TileObject tileObject && tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                callback.Invoke(objHere);
                //Sleeping characters in bed should also receive damage
                //https://trello.com/c/kFZAHo11/1203-sleeping-characters-in-bed-should-also-receive-damage
                if (tileObject is Bed bed) {
                    if (bed.users != null && bed.users.Length > 0) {
                        for (int i = 0; i < bed.users.Length; i++) {
                            Character user = bed.users[i];
                            //Should only apply if user is not part of charactersHere list so that no duplicate calls shall take place
                            if (!charactersHere.Contains(user)) {
                                callback.Invoke(user);
                            }
                        }
                    }
                }
            }
            Messenger.Broadcast(Signals.ACTION_PERFORMED_ON_TILE_TRAITABLES, this, callback);
        }
        public List<IPointOfInterest> GetPOIsOnTile() {
            List<IPointOfInterest> pois = new List<IPointOfInterest>();
            pois.Add(genericTileObject);
            if (objHere != null) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                    pois.Add(objHere);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                pois.Add(character);
            }
            return pois;
        }
        public void AddTraitToAllPOIsOnTile(string traitName) {
            genericTileObject.traitContainer.AddTrait(genericTileObject, traitName);
            if (objHere != null) {
                if ((objHere is TileObject && (objHere as TileObject).mapObjectState == MAP_OBJECT_STATE.BUILT)) {
                    objHere.traitContainer.AddTrait(objHere, traitName);
                }
            }
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                character.traitContainer.AddTrait(character, traitName);
            }
        }
        public int GetNeighbourOfTypeCount(Ground_Type type, bool useFourNeighbours = false) {
            int count = 0;
            Dictionary<GridNeighbourDirection, LocationGridTile> n = neighbours;
            if (useFourNeighbours) {
                n = FourNeighboursDictionary();
            }
            for (int i = 0; i < n.Values.Count; i++) {
                if (neighbours.Values.ElementAt(i).groundType == type) {
                    count++;
                }
            }
            return count;
        }
        public bool IsPartOfSettlement(out BaseSettlement settlement) {
            if (collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null) {
                settlement = collectionOwner.partOfHextile.hexTileOwner.settlementOnTile;
                return true;
            }
            settlement = null;
            return false;
        }
        public bool IsPartOfSettlement(BaseSettlement settlement) {
            return collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile == settlement;
        }
        public bool IsPartOfSettlement() {
            return collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null;
        }
        public bool IsPartOfHumanElvenSettlement() {
            return collectionOwner.isPartOfParentRegionMap && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile != null && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.locationType == LOCATION_TYPE.SETTLEMENT;
        }
        public bool IsPartOfActiveHumanElvenSettlement() {
            return IsPartOfHumanElvenSettlement() && collectionOwner.partOfHextile.hexTileOwner.settlementOnTile.residents.Count > 0;
        }
        public bool IsNextToSettlement(out BaseSettlement settlement) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement(out settlement)) {
                    return true;
                }
            }
            settlement = null;
            return false;
        }
        public bool IsNextToSettlement() {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement()) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNextToSettlement(BaseSettlement settlement) {
            for (int i = 0; i < neighbourList.Count; i++) {
                LocationGridTile tile = neighbourList[i];
                if (tile.IsPartOfSettlement(settlement)) {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Is this tile part of an area that is next to the given settlement.
        /// </summary>
        /// <param name="settlement">The settlement to check</param>
        /// <returns>True or false.</returns>
        public bool IsNextToSettlementArea(BaseSettlement settlement) {
            if (collectionOwner.isPartOfParentRegionMap == false) {
                return false;
            }
            HexTile hexTile = collectionOwner.partOfHextile.hexTileOwner;
            for (int i = 0; i < hexTile.AllNeighbours.Count; i++) {
                HexTile neighbour = hexTile.AllNeighbours[i];
                if (neighbour.settlementOnTile == settlement) {
                    return true;
                }
            }
            return false;
        }
        public bool IsNextToOrPartOfSettlement(out BaseSettlement settlement) {
            return IsPartOfSettlement(out settlement) || IsNextToSettlement(out settlement);
        }
        public bool IsNextToOrPartOfSettlement(BaseSettlement settlement) {
            return IsPartOfSettlement(settlement) || IsNextToSettlement(settlement);
        }
        public bool IsNextToSettlementAreaOrPartOfSettlement(BaseSettlement settlement) {
            return IsPartOfSettlement(settlement) || IsNextToSettlementArea(settlement);
        }
        public List<LocationGridTile> GetTilesInRadius(int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false, bool includeImpassable = true) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
            int mapSizeX = parentMap.map.GetUpperBound(0);
            int mapSizeY = parentMap.map.GetUpperBound(1);
            int x = localPlace.x;
            int y = localPlace.y;
            if (includeCenterTile) {
                tiles.Add(this);
            }
            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;


            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if (dx == x && dy == y) {
                            continue;
                        }
                        if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = parentMap.map[dx, dy];
                        if (result.structure == null) { continue; } //do not include tiles with no structures
                        if (!includeTilesInDifferentStructure 
                            && (result.structure != structure && (!result.structure.structureType.IsOpenSpace() || !structure.structureType.IsOpenSpace()))) { continue; }
                        if(!includeImpassable && !result.IsPassable()) { continue; }
                        tiles.Add(result);
                    }
                }
            }
            return tiles;
        }
        //public void SetIsOuterTile(bool state) {
        //    isOuterTile = state;
        //}
        public bool IsPassable() {
            return (objHere == null || !(objHere is BlockWall)) && groundType != Ground_Type.Water;
        }
        #endregion

        #region Building
        public void SetHasBlueprint(bool hasBlueprint) {
            this.hasBlueprint = hasBlueprint;
        }
      
        #endregion

        #region Walls
        public void AddWallObject(StructureWallObject structureWallObject) {
            walls.Add(structureWallObject);
        }
        public void ClearWallObjects() {
            walls.Clear();
        }
        #endregion

        #region Corruption
        public void CorruptTile() {
            SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
            CreateSeamlessEdgesForSelfAndNeighbours();
            if (objHere != null) {
                if (objHere is TreeObject tree) {
                    (tree.mapObjectVisual as TileObjectGameObject).UpdateTileObjectVisual(tree);
                } else if (objHere is BlockWall blockWall) {
                    blockWall.SetWallType(WALL_TYPE.Demon_Stone);
                    blockWall.UpdateVisual(this);
                } else {
                    structure.RemovePOI(objHere);
                }
            }
        }
        public void UnCorruptTile() {
            RevertTileToOriginalPerlin();
            CreateSeamlessEdgesForSelfAndNeighbours();
            if (objHere != null) {
                structure.RemovePOI(objHere);
            }
        }
        #endregion

        #region Landmine
        public void SetHasLandmine(bool state) {
            if(hasLandmine != state) {
                hasLandmine = state;
                if (hasLandmine) {
                    _landmineEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Landmine, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    ObjectPoolManager.Instance.DestroyObject(_landmineEffect);
                    _landmineEffect = null;
                }
            }
        }
        private IEnumerator TriggerLandmine(Character triggeredBy) {
            GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Landmine_Explosion);
            genericTileObject.traitContainer.AddTrait(genericTileObject, "Danger Remnant");
            yield return new WaitForSeconds(0.5f);
            SetHasLandmine(false);
            List<LocationGridTile> tiles = GetTilesInRadius(3, includeCenterTile: true, includeTilesInDifferentStructure: true);
            BurningSource bs = null;
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                List<IPointOfInterest> pois = tile.GetPOIsOnTile();
                for (int j = 0; j < pois.Count; j++) {
                    IPointOfInterest poi = pois[j];
                    if (poi.gridTileLocation == null) {
                        continue; //skip
                    }
                    if (poi is TileObject obj) {
                        if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                            obj.AdjustHP(-500, ELEMENTAL_TYPE.Normal, true,
                                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs), showHPBar: true);
                        } else {
                            CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Normal, obj,
                                elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs));
                        }
                    } else if (poi is Character character) {
                        character.AdjustHP(-500, ELEMENTAL_TYPE.Normal, true,
                            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs), showHPBar: true);
                    } else {
                        poi.AdjustHP(-500, ELEMENTAL_TYPE.Normal, true,
                            elementalTraitProcessor: (target, trait) => TraitManager.Instance.ProcessBurningTrait(target, trait, ref bs), showHPBar: true);
                    }
                }
            }
        }
        #endregion

        #region Freezing Trap
        public void SetHasFreezingTrap(bool state, TrapChecker freezingTrapChecker = null) {
            if (hasFreezingTrap != state) {
                hasFreezingTrap = state;
                if (hasFreezingTrap) {
                    if (collectionOwner.isPartOfParentRegionMap) {
                        collectionOwner.partOfHextile.hexTileOwner.AddFreezingTrapInHexTile();
                    }
                    _freezingTrapChecker = freezingTrapChecker;
                    _freezingTrapEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Freezing_Trap, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    if (collectionOwner.isPartOfParentRegionMap) {
                        collectionOwner.partOfHextile.hexTileOwner.RemoveFreezingTrapInHexTile();
                    }
                    ObjectPoolManager.Instance.DestroyObject(_freezingTrapEffect);
                    _freezingTrapEffect = null;
                    _freezingTrapChecker = null;
                }
            }
        }
        private void TriggerFreezingTrap(Character triggeredBy) {
            GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Freezing_Trap_Explosion);
            AudioManager.Instance.CreateAudioObject(
                PlayerSkillManager.Instance.GetPlayerSkillData<FreezingTrapSkillData>(SPELL_TYPE.FREEZING_TRAP).trapExplosionSound, this, 1, false);
            SetHasFreezingTrap(false);
            for (int i = 0; i < 3; i++) {
                if (triggeredBy.traitContainer.HasTrait("Frozen")) {
                    break;
                } else {
                    triggeredBy.traitContainer.AddTrait(triggeredBy, "Freezing", bypassElementalChance: true);
                }
            }
        }
        #endregion

        #region Freezing Trap
        public void SetHasSnareTrap(bool state) {
            if (hasSnareTrap != state) {
                hasSnareTrap = state;
                if (hasSnareTrap) {
                    _snareTrapEffect = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Snare_Trap, InnerMapManager.DetailsTilemapSortingOrder - 1);
                } else {
                    ObjectPoolManager.Instance.DestroyObject(_snareTrapEffect);
                    _snareTrapEffect = null;
                }
            }
        }
        private void TriggerSnareTrap(Character triggeredBy) {
            GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Snare_Trap_Explosion);
            SetHasSnareTrap(false);
            triggeredBy.traitContainer.AddTrait(triggeredBy, "Ensnared");
        }
        #endregion

        #region Hextile
        public HexTile GetNearestHexTileWithinRegion() {
            if (collectionOwner.isPartOfParentRegionMap) {
                HexTile hex = collectionOwner.partOfHextile.hexTileOwner;
                if (hex.elevationType != ELEVATION.WATER && hex.elevationType != ELEVATION.MOUNTAIN) {
                    return hex;
                }
            }

            HexTile nearestHex = null;
            float nearestDist = 0f;
            for (int i = 0; i < collectionOwner.region.tiles.Count; i++) {
                HexTile hex = collectionOwner.region.tiles[i];
                if (hex.elevationType != ELEVATION.WATER && hex.elevationType != ELEVATION.MOUNTAIN) {
                    float dist = GetDistanceTo(hex.GetCenterLocationGridTile());
                    if (nearestHex == null || dist < nearestDist) {
                        nearestHex = hex;
                        nearestDist = dist;
                    }
                }
            }
            return nearestHex;
        }
        public HexTile GetNearestHexTileWithinRegionThatMeetCriteria(System.Func<HexTile, bool> validityChecker) {
            if (collectionOwner.isPartOfParentRegionMap) {
                HexTile hex = collectionOwner.partOfHextile.hexTileOwner;
                if (validityChecker.Invoke(hex)) {
                    return hex;
                }
            }

            HexTile nearestHex = null;
            float nearestDist = 0f;
            for (int i = 0; i < collectionOwner.region.tiles.Count; i++) {
                HexTile hex = collectionOwner.region.tiles[i];
                if (validityChecker.Invoke(hex)) {
                    float dist = GetDistanceTo(hex.GetCenterLocationGridTile());
                    if (nearestHex == null || dist < nearestDist) {
                        nearestHex = hex;
                        nearestDist = dist;
                    }
                }
            }
            return nearestHex;
        }
        #endregion

        #region Pathfinding
        public GraphNode graphNode { get; private set; }
        public void PredetermineGraphNode() {
            graphNode = AstarPath.active.GetNearest(centeredWorldLocation).node;
        }
        #endregion
    }

    [Serializable]
    public struct TwoTileDirections {
        public GridNeighbourDirection from;
        public GridNeighbourDirection to;

        public TwoTileDirections(GridNeighbourDirection from, GridNeighbourDirection to) {
            this.from = from;
            this.to = to;
        }
    }
}