﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using JetBrains.Annotations;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Collections;
using Debug = System.Diagnostics.Debug;

public partial class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;
    public static readonly int Max_Connections = 3;
    public const int DELAY_DIVINE_INTERVENTION_DURATION = 144;
    public const int SUMMON_MINION_DURATION = 96;
    public const int MAX_RESOURCE_PILE = 500;

    public int initialLandmarkCount;

    [SerializeField] private List<LandmarkData> landmarkData;
    public List<AreaData> areaData;

    public int corruptedLandmarksCount;

    public List<BaseLandmark> allLandmarks;
    public List<Settlement> allSetttlements;
    public List<Settlement> allNonPlayerSettlements;

    [SerializeField] private GameObject landmarkGO;

    private Dictionary<LANDMARK_TYPE, LandmarkData> landmarkDataDict;

    public AreaTypeSpriteDictionary locationPortraits;
    [Header("Connections")]
    [SerializeField] private GameObject landmarkConnectionPrefab;

    public List<LocationEvent> locationEventsData { get; private set; }

    public STRUCTURE_TYPE[] humanSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] humanUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] humanCombatStructures { get; private set; }
    public STRUCTURE_TYPE[] elfSurvivalStructures { get; private set; }
    public STRUCTURE_TYPE[] elfUtilityStructures { get; private set; }
    public STRUCTURE_TYPE[] elfCombatStructures { get; private set; }

    //The Anvil
    public Dictionary<string, AnvilResearchData> anvilResearchData;

    public void Initialize() {
        corruptedLandmarksCount = 0;
        allSetttlements = new List<Settlement>();
        allNonPlayerSettlements = new List<Settlement>();
        ConstructLandmarkData();
        LoadLandmarkTypeDictionary();
        ConstructAnvilResearchData();
        ConstructLocationEventsData();
        ConstructRaceStructureRequirements();
    }

    #region Monobehaviours
    private void Awake() {
        Instance = this;
    }
    #endregion

    #region Landmarks
    private void ConstructLocationEventsData() {
        locationEventsData = new List<LocationEvent>() {
            new NewResidentEvent(),
        };
    }
    private void ConstructLandmarkData() {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData data = landmarkData[i];
            data.ConstructData();
        }
    }
    private void LoadLandmarkTypeDictionary() {
        landmarkDataDict = new Dictionary<LANDMARK_TYPE, LandmarkData>();
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData data = landmarkData[i];
            landmarkDataDict.Add(data.landmarkType, data);
        }
    }
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType, bool addFeatures) {
        if (location.landmarkOnTile != null) {
            //Destroy landmark on tile
            DestroyLandmarkOnTile(location);
        }
        BaseLandmark newLandmark = location.CreateLandmarkOfType(landmarkType, addFeatures);
        newLandmark.tileLocation.AdjustUncorruptibleLandmarkNeighbors(1);
        location.UpdateBuildSprites();
        Messenger.Broadcast(Signals.LANDMARK_CREATED, newLandmark);
        return newLandmark;
    }
    public void DestroyLandmarkOnTile(HexTile tile) {
        BaseLandmark landmarkOnTile = tile.landmarkOnTile;
        if (landmarkOnTile == null) {
            return;
        }
        landmarkOnTile.DestroyLandmark();
        tile.UpdateBuildSprites();
        tile.RemoveLandmarkVisuals();
        tile.RemoveLandmarkOnTile();
        Messenger.Broadcast(Signals.LANDMARK_DESTROYED, landmarkOnTile, tile);
        if (landmarkOnTile.specificLandmarkType.IsPlayerLandmark() && tile.region.locationType.IsSettlementType() == false) {
            Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, 
                tile.region.regionTileObject as IPointOfInterest, 
                "target has been destroyed", JOB_TYPE.ATTACK_DEMONIC_REGION);    
        }
    }
    public BaseLandmark LoadLandmarkOnTile(HexTile location, BaseLandmark landmark) {
        BaseLandmark newLandmark = location.LoadLandmark(landmark);
        return newLandmark;
    }
    public GameObject GetLandmarkGO() {
        return this.landmarkGO;
    }
    public bool AreAllNonPlayerAreasCorrupted() {
        List<Settlement> areas = allNonPlayerSettlements;
        for (int i = 0; i < areas.Count; i++) {
            Settlement settlement = areas[i];
            for (int j = 0; j < settlement.tiles.Count; j++) {
                HexTile currTile = settlement.tiles[j];
                if (!currTile.isCorrupted) {
                    return false;
                }    
            }
           
        }
        return true;
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, LANDMARK_TYPE type) {
        if (type.IsPlayerLandmark()) {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(type.ToString());
            System.Type systemType = System.Type.GetType(typeName);
            if (systemType != null) {
                return System.Activator.CreateInstance(systemType, location, type) as BaseLandmark;
            }
            return null;
        } else {
            return new BaseLandmark(location, type);
        }
    }
    public BaseLandmark CreateNewLandmarkInstance(HexTile location, SaveDataLandmark data) {
        if (data.landmarkType.IsPlayerLandmark()) {
            var typeName = Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(data.landmarkType.ToString());
            System.Type systemType = System.Type.GetType(typeName);
            if (systemType != null) {
                return System.Activator.CreateInstance(systemType, location, data) as BaseLandmark;
            }
            return null;
        } else {
            return new BaseLandmark(location, data);
        }
    }
    #endregion

    #region Landmark Generation
    public LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
        if (landmarkDataDict.ContainsKey(landmarkType)) {
            return landmarkDataDict[landmarkType];
        }
        throw new System.Exception("There is no landmark data for " + landmarkType.ToString());
    }
    public LandmarkData GetLandmarkData(string landmarkName) {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.landmarkTypeString == landmarkName) {
                return currData;
            }
        }
        throw new System.Exception("There is no landmark data for " + landmarkName);
    }
    public Island GetIslandOfRegion(Region region, List<Island> islands) {
        for (int i = 0; i < islands.Count; i++) {
            Island currIsland = islands[i];
            if (currIsland.regionsInIsland.Contains(region)) {
                return currIsland;
            }
        }
        return null;
    }
    private void MergeIslands(Island island1, Island island2, ref List<Island> islands) {
        island1.regionsInIsland.AddRange(island2.regionsInIsland);
        islands.Remove(island2);
    }
    #endregion

    #region Utilities
    public BaseLandmark GetLandmarkByID(int id) {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.id == id) {
                return currLandmark;
            }
        }
        return null;
    }
    public BaseLandmark GetLandmarkByName(string name) {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
                return currLandmark;
            }
        }
        //for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
        //    Region currRegion = GridMap.Instance.allRegions[i];
        //    if (currRegion.mainLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
        //        return currRegion.mainLandmark;
        //    }
        //    for (int j = 0; j < currRegion.landmarks.Count; j++) {
        //        BaseLandmark currLandmark = currRegion.landmarks[j];
        //        if (currLandmark.landmarkName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
        //            return currLandmark;
        //        }
        //    }
        //}
        return null;
    }
    public BaseLandmark GetLandmarkOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            if (currLandmark.specificLandmarkType == landmarkType) {
                return currLandmark;
            }
        }
        return null;
    }
    public List<BaseLandmark> GetAllLandmarks() {
        List<BaseLandmark> landmarks = new List<BaseLandmark>();
        List<HexTile> choices = GridMap.Instance.normalHexTiles;
        for (int i = 0; i < choices.Count; i++) {
            HexTile currTile = choices[i];
            if (currTile.landmarkOnTile != null) {
                landmarks.Add(currTile.landmarkOnTile);
            }
        }
        return landmarks;
    }
    public List<LandmarkStructureSprite> GetLandmarkTileSprites(HexTile tile, LANDMARK_TYPE landmarkType, RACE race = RACE.NONE) {
        LandmarkData data = GetLandmarkData(landmarkType);
        if (data.biomeTileSprites.Count > 0) { //if the landmark type has a biome type tile sprite set, use that instead
            if (data.biomeTileSprites.ContainsKey(tile.biomeType)) {
                return data.biomeTileSprites[tile.biomeType]; //prioritize biome type sprites
            }
        }
        if (race == RACE.HUMANS) {
            return data.humansLandmarkTileSprites;
        } else if (race == RACE.ELVES) {
            return data.elvenLandmarkTileSprites;
        } else {
            if (data.neutralTileSprites.Count > 0) {
                return data.neutralTileSprites;
            } else {
                return null;
            }
        }
        
    }
    public List<Character> GetAllDeadCharactersInLocation(ILocation location) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.isDead && character.currentRegion.IsSameCoreLocationAs(location) && !(character is Summon)) {
                if(character.marker != null || character.grave != null) { //Only resurrect characters who are in the tombstone or still has a marker in the settlement
                    characters.Add(character);
                }
            }
        }
        return characters;
    }
    #endregion

    #region Areas
    public AreaData GetAreaData(LOCATION_TYPE locationType) {
        for (int i = 0; i < areaData.Count; i++) {
            AreaData currData = areaData[i];
            if (currData.locationType == locationType) {
                return currData;
            }
        }
        throw new System.Exception("No settlement data for type " + locationType);
    }
    public Settlement CreateNewSettlement(Region region, LOCATION_TYPE locationType, int citizenCount, params HexTile[] tiles) {
        Settlement newSettlement = new Settlement(region, locationType, citizenCount);
        if (locationPortraits.ContainsKey(newSettlement.locationType)) {
            newSettlement.SetLocationPortrait(locationPortraits[newSettlement.locationType]);
        }
        newSettlement.AddTileToSettlement(tiles);
        Messenger.Broadcast(Signals.AREA_CREATED, newSettlement);
        allSetttlements.Add(newSettlement);
        if(locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
            allNonPlayerSettlements.Add(newSettlement);
        }
        return newSettlement;
    }
    public void RemoveArea(Settlement settlement) {
        allSetttlements.Remove(settlement);
    }
    public Settlement CreateNewArea(SaveDataArea saveDataArea) {
        Settlement newSettlement = new Settlement(saveDataArea);

        if (locationPortraits.ContainsKey(newSettlement.locationType)) {
            newSettlement.SetLocationPortrait(locationPortraits[newSettlement.locationType]);
        }
        Messenger.Broadcast(Signals.AREA_CREATED, newSettlement);
        allSetttlements.Add(newSettlement);
        if (saveDataArea.locationType != LOCATION_TYPE.DEMONIC_INTRUSION) {
            allNonPlayerSettlements.Add(newSettlement);
        }
        return newSettlement;
    }

    public Settlement GetAreaByID(int id) {
        for (int i = 0; i < allSetttlements.Count; i++) {
            Settlement settlement = allSetttlements[i];
            if (settlement.id == id) {
                return settlement;
            }
        }
        return null;
    }
    public Settlement GetAreaByName(string name) {
        for (int i = 0; i < allSetttlements.Count; i++) {
            Settlement settlement = allSetttlements[i];
            if (settlement.name.Equals(name)) {
                return settlement;
            }
        }
        return null;
    }
    public void OwnSettlement(Faction newOwner, Settlement settlement) {
        if (settlement.owner != null) {
            UnownSettlement(settlement);
        }
        newOwner.AddToOwnedSettlements(settlement);
        settlement.SetOwner(newOwner);
        settlement.TintStructures(newOwner.factionColor);
    }
    public void UnownSettlement(Settlement settlement) {
        settlement.owner?.RemoveFromOwnedSettlements(settlement);
        settlement.SetOwner(null);
        settlement.TintStructures(Color.white);
    }
    public void LoadAdditionalAreaData() {
        for (int i = 0; i < allSetttlements.Count; i++) {
            Settlement currSettlement = allSetttlements[i];
            currSettlement.LoadAdditionalData();
        }
    }
    public Vector2 GetNameplatePosition(HexTile tile) {
        Vector2 defaultPos = tile.transform.position;
        defaultPos.y -= 1.25f;
        return defaultPos;
    }
    #endregion

    #region Burning Source
    public BurningSource GetBurningSourceByID(int id) {
        for (int i = 0; i < allSetttlements.Count; i++) {
            Settlement currSettlement = allSetttlements[i];
            if (currSettlement.innerMap != null) {
                for (int j = 0; j < currSettlement.innerMap.activeBurningSources.Count; j++) {
                    BurningSource source = currSettlement.innerMap.activeBurningSources[j];
                    if (source.id == id) {
                        return source;
                    }
                }
            }
        }
        return null;
    }
    #endregion

    #region Location Structures
    public LocationStructure CreateNewStructureAt(ILocation location, STRUCTURE_TYPE type, Settlement settlement = null) {
        LocationStructure createdStructure = null;
        switch (type) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(location);
                break;
            default:
                createdStructure = new LocationStructure(type, location);
                break;
        }
        location.AddStructure(createdStructure);
        settlement?.AddStructure(createdStructure);
        return createdStructure;
    }
    public LocationStructure LoadStructureAt(ILocation location, SaveDataLocationStructure data) {
        LocationStructure createdStructure = data.Load(location);
        if (createdStructure != null) {
            location.AddStructure(createdStructure);
        }
        return createdStructure;
    }
    private void ConstructRaceStructureRequirements() {
        //humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };
        //elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE };

        humanSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.APOTHECARY };
        humanUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP, STRUCTURE_TYPE.INN };
        humanCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD, STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfSurvivalStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.APOTHECARY, STRUCTURE_TYPE.MAGE_QUARTERS };
        elfUtilityStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.CEMETERY, STRUCTURE_TYPE.PRISON, STRUCTURE_TYPE.GRANARY, STRUCTURE_TYPE.MINER_CAMP };
        elfCombatStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.SMITHY, STRUCTURE_TYPE.BARRACKS, STRUCTURE_TYPE.RAIDER_CAMP, STRUCTURE_TYPE.ASSASSIN_GUILD };
    }
    public STRUCTURE_TYPE[] GetRaceStructureRequirements(RACE race, string category) {
        if(race == RACE.HUMANS) {
            if (category == "Survival") { return humanSurvivalStructures; }
            else if (category == "Utility") { return humanUtilityStructures; }
            else if (category == "Combat") { return humanCombatStructures; }
        } else if (race == RACE.ELVES) {
            if (category == "Survival") { return elfSurvivalStructures; }
            else if (category == "Utility") { return elfUtilityStructures; }
            else if (category == "Combat") { return elfCombatStructures; }
        }
        return null;
    }
    #endregion

    #region Regions
    public TileFeature CreateTileFeature([NotNull] string featureName) {
        try {
            Debug.Assert(featureName != null, nameof(featureName) + " != null");
            return System.Activator.CreateInstance(System.Type.GetType(featureName)) as TileFeature;
        } catch {
            throw new System.Exception("Cannot create region feature with name " + featureName);
        }
        
    }
    public Region GetRandomRegionWithFeature(string feature) {
        List<Region> choices = new List<Region>();
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            if (region.HasTileWithFeature(feature)) {
                choices.Add(region);
            }
        }
        if (choices.Count > 0) {
            return Utilities.GetRandomElement(choices);
        }
        return null;
    }
    #endregion

    #region The Anvil
    private void ConstructAnvilResearchData() {
        anvilResearchData = new Dictionary<string, AnvilResearchData>() {
            { TheAnvil.Improved_Spells_1,
                new AnvilResearchData() {
                    effect = 2,
                    description = "Increase Spell level to 2.",
                    manaCost = 150,
                    durationInHours = 8,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Spell Level increased!",
                }
            },
            { TheAnvil.Improved_Spells_2,
                new AnvilResearchData() {
                    effect = 3,
                    description = "Increase Spell level to 3.",
                    manaCost = 300,
                    durationInHours = 24,
                    preRequisiteResearch = TheAnvil.Improved_Spells_1,
                    researchDoneNotifText = "Spell Level increased!",
                }
            },
            { TheAnvil.Improved_Artifacts_1,
                new AnvilResearchData() {
                    effect = 2,
                    description = "Increase Artifact level to 2.",
                    manaCost = 100,
                    durationInHours = 4,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Artifact Level increased!",
                }
            },
            { TheAnvil.Improved_Artifacts_2,
                new AnvilResearchData() {
                    effect = 3,
                    description = "Increase Artifact level to 3.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = TheAnvil.Improved_Artifacts_1,
                    researchDoneNotifText = "Artifact Level increased!",
                }
            },
            { TheAnvil.Improved_Summoning_1,
                new AnvilResearchData() {
                    effect = 2,
                    description = "Increase Summon level to 2.",
                    manaCost = 100,
                    durationInHours = 4,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Summon Level increased!",
                }
            },
            { TheAnvil.Improved_Summoning_2,
                new AnvilResearchData() {
                    effect = 3,
                    description = "Increase Summon level to 3.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = TheAnvil.Improved_Summoning_1,
                    researchDoneNotifText = "Summon Level increased!",
                }
            },
            { TheAnvil.Faster_Invasion,
                new AnvilResearchData() {
                    effect = 20,
                    description = "Invasion is 20% faster.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Invasion rate increased!",
                }
            },
            { TheAnvil.Improved_Construction,
                new AnvilResearchData() {
                    effect = 20,
                    description = "Construction is 20% faster.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Construction rate increased!",
                }
            },
            { TheAnvil.Increased_Mana_Capacity,
                new AnvilResearchData() {
                    effect = 600,
                    description = "Maximum Mana increased to 600.",
                    manaCost = 200,
                    durationInHours = 12,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Maximum mana increased!",
                }
            },
            { TheAnvil.Increased_Mana_Regen,
                new AnvilResearchData() {
                    effect = 5,
                    description = "Mana Regen increased by 5.",
                    manaCost = 200,
                    durationInHours = 24,
                    preRequisiteResearch = string.Empty,
                    researchDoneNotifText = "Mana regeneration rate increased!",
                }
            },
        };
    }
    #endregion
}

public class Island {

    public List<Region> regionsInIsland;

    public Island(Region region) {
        regionsInIsland = new List<Region>();
        regionsInIsland.Add(region);
    }

    public bool TryGetLandmarkThatCanConnectToOtherIsland(Island otherIsland, List<Island> allIslands, out Region regionToConnectTo, out Region regionThatWillConnect) {
        for (int i = 0; i < regionsInIsland.Count; i++) {
            Region currRegion = regionsInIsland[i];
            List<Region> adjacent = currRegion.AdjacentRegions().Where(x => LandmarkManager.Instance.GetIslandOfRegion(x, allIslands) != this).ToList(); //get all adjacent regions, that does not belong to this island.
            if (adjacent.Count > 0) {
                regionToConnectTo = adjacent[Random.Range(0, adjacent.Count)];
                regionThatWillConnect = currRegion;
                return true;

            }
        }
        regionToConnectTo = null;
        regionThatWillConnect = null;
        return false;
    }
}