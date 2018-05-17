﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LandmarkManager : MonoBehaviour {

    public static LandmarkManager Instance = null;

    public List<CharacterProductionWeight> characterProductionWeights;
	//public DungeonEncounterChances[] dungeonEncounterChances;
    public int initialResourceLandmarks;
    public int initialDungeonLandmarks;
    public int initialSettlementLandmarks;

    public int initialLandmarkCount;

    public List<BaseLandmarkData> baseLandmarkData;
    public List<LandmarkData> landmarkData;

	//Crater
	public BaseLandmark craterLandmark;
    private void Awake() {
        Instance = this;
    }

    /*
     Create a new landmark on a specified tile.
     */
    public BaseLandmark CreateNewLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkType);
        BASE_LANDMARK_TYPE baseLandmarkType = landmarkData.baseLandmarkType;
        BaseLandmark newLandmark = location.CreateLandmarkOfType(baseLandmarkType, landmarkType);
        newLandmark.tileLocation.SetCanBeCorrupted(false);
        newLandmark.GenerateDiagonalLeftTiles();
        newLandmark.GenerateDiagonalRightTiles();
        newLandmark.GenerateHorizontalTiles();
        newLandmark.GenerateWallTiles();
        for (int i = 0; i < location.AllNeighbours.Count; i++) {
            location.AllNeighbours[i].SetCanBeCorrupted(false);
        }
        //ConstructLandmarkObjects(landmarkData, newLandmark);
        //		AddInitialLandmarkItems (newLandmark);
        return newLandmark;
    }
    public void ConstructAllLandmarkObjects() {
        List<BaseLandmark> allLandmarks = GetAllLandmarks();
        for (int i = 0; i < allLandmarks.Count; i++) {
            BaseLandmark currLandmark = allLandmarks[i];
            LandmarkData data = LandmarkManager.Instance.GetLandmarkData(currLandmark.specificLandmarkType);
            BaseLandmarkData baseData = LandmarkManager.Instance.GetBaseLandmarkData(LandmarkManager.Instance.GetBaseLandmarkType(currLandmark.specificLandmarkType));
            ConstructLandmarkObjects(baseData, data, currLandmark);
        }
    }
    public void ConstructLandmarkObjects(BaseLandmarkData baseData, LandmarkData data, BaseLandmark landmark) {
        IObject createdObject = null;
        for (int i = 0; i < baseData.initialObjects.Count; i++) {
            createdObject = ObjectManager.Instance.CreateNewObject(baseData.initialObjects[i].name, landmark);
            if (createdObject.objectType == OBJECT_TYPE.STRUCTURE) {
                if(createdObject.currentState.stateName.Equals("Under Construction")) {
                    ObjectState defaultOrEmptyState = createdObject.GetState("Default");
                    if (defaultOrEmptyState == null) {
                        defaultOrEmptyState = createdObject.GetState("Empty");
                    }
                    createdObject.ChangeState(defaultOrEmptyState);
                }
                StructureObj structure = createdObject as StructureObj;
                structure.SetHP(100);
                structure.SetIsMadeOf(RESOURCE.OAK);
            }
        }
        ObjectManager.Instance.CreateNewObject("Landmark Object", landmark);
        for (int i = 0; i < data.initialObjects.Count; i++) {
            int chance = UnityEngine.Random.Range(0, 100);
            if(chance < data.initialObjects[i].spawnChance) {
                createdObject = ObjectManager.Instance.CreateNewObject(data.initialObjects[i].obj.name, landmark);
            } else {
                if(data.initialObjects[i].fallbackObject != null) {
                    createdObject = ObjectManager.Instance.CreateNewObject(data.initialObjects[i].fallbackObject.name, landmark);
                }
            }
            if (createdObject != null && createdObject.objectType == OBJECT_TYPE.STRUCTURE) {
                if (createdObject.currentState.stateName.Equals("Under Construction")) {
                    ObjectState defaultOrEmptyState = createdObject.GetState("Default");
                    if (defaultOrEmptyState == null) {
                        defaultOrEmptyState = createdObject.GetState("Empty");
                    }
                    createdObject.ChangeState(defaultOrEmptyState);
                }
                StructureObj structure = createdObject as StructureObj;
                structure.SetHP(100);
                structure.SetIsMadeOf(RESOURCE.OAK);
            }
        }
    }
    /*
     Occupy a specified landmark.
         */
    public void OccupyLandmark(BaseLandmark landmark, Faction occupant) {
        landmark.OccupyLandmark(occupant);
    }
    /*
     Occupy the main settlement in a region
         */
    public void OccupyLandmark(Region region, Faction occupant) {
        region.centerOfMass.landmarkOnTile.OccupyLandmark(occupant);
    }
	public void OccupyLandmark(HexTile hexTile, Faction occupant) {
		hexTile.landmarkOnTile.OccupyLandmark(occupant);
	}

    #region Landmark Generation
    //public bool GenerateLandmarks() {
    //    List<BaseLandmark> createdLandmarks = new List<BaseLandmark>();
    //    List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles);

    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        Utilities.ListRemoveRange(elligibleTiles, currRegion.outerTiles);
    //    }

    //    //create landmarks on all regions' center of mass first
    //    for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
    //        Region currRegion = GridMap.Instance.allRegions[i];
    //        if (currRegion.centerOfMass.landmarkOnTile == null) {
    //            WeightedDictionary<LANDMARK_TYPE> landmarkWeights = GetLandmarkAppearanceWeights(currRegion);
    //            LANDMARK_TYPE chosenType = landmarkWeights.PickRandomElementGivenWeights();
    //            BaseLandmark createdLandmark = CreateNewLandmarkOnTile(currRegion.centerOfMass, chosenType);
    //            elligibleTiles.Remove(createdLandmark.tileLocation);
    //            Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
    //            createdLandmarks.Add(createdLandmark);
    //        }
    //    }

    //    //all factions must have 1 kings castle in 1 of their owned regions
    //    for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
    //        Faction currTribe = FactionManager.Instance.allTribes[i];
    //        if (currTribe.HasAccessToLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE)) {
    //            //occupy that kings castle instead
    //            BaseLandmark kingsCastle = currTribe.GetAccessibleLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE);
    //            kingsCastle.OccupyLandmark(currTribe);
    //        } else {
    //            //the currTribe doesn't own a region that has a kings castle create a new one and occupy it
    //            List<HexTile> tilesToChooseFrom = new List<HexTile>();
    //            currTribe.ownedRegions.ForEach(x => tilesToChooseFrom.AddRange(x.tilesInRegion.Where(y => elligibleTiles.Contains(y))));
    //            HexTile chosenTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
    //            BaseLandmark createdLandmark = CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.KINGS_CASTLE);
    //            createdLandmark.OccupyLandmark(currTribe);
    //            elligibleTiles.Remove(createdLandmark.tileLocation);
    //            Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
    //            createdLandmarks.Add(createdLandmark);
    //        }
    //    }
        
    //    while (createdLandmarks.Count < initialLandmarkCount) {
    //        if (elligibleTiles.Count <= 0) {
    //            return false; //ran out of tiles
    //        }
    //        HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
    //        WeightedDictionary<LANDMARK_TYPE> landmarkWeights = GetLandmarkAppearanceWeights(chosenTile.region);
    //        LANDMARK_TYPE chosenType = landmarkWeights.PickRandomElementGivenWeights();
    //        BaseLandmark createdLandmark = CreateNewLandmarkOnTile(chosenTile, chosenType);
    //        elligibleTiles.Remove(createdLandmark.tileLocation);
    //        Utilities.ListRemoveRange(elligibleTiles, createdLandmark.tileLocation.GetTilesInRange(3)); //remove tiles in range (3)
    //        createdLandmarks.Add(createdLandmark);
    //    }

    //    Debug.Log("Created " + createdLandmarks.Count + " landmarks.");

    //    return true;
    //}
    public void GeneratePlayerLandmarks(Region chosenRegion) {
        //generate landmarks owned by the player in the empty region
        //Create Demonic Portal on the tile nearest to the center of the region
        HexTile tileToUse = null;
        LandmarkData demonicPortalData = GetLandmarkData(LANDMARK_TYPE.DEMONIC_PORTAL);
        if (chosenRegion.centerOfMass.isPassable && demonicPortalData.possibleSpawnPoints.Contains(chosenRegion.centerOfMass.passableType)) {
            tileToUse = chosenRegion.centerOfMass;
        } else {
            float nearestDistance = 9999f;
            List<HexTile> passableTilesInRegion = chosenRegion.tilesInRegion.Where(x => x.isPassable && x.id != chosenRegion.centerOfMass.id && demonicPortalData.possibleSpawnPoints.Contains(x.passableType)).ToList();
            for (int i = 0; i < passableTilesInRegion.Count; i++) {
                HexTile currTile = passableTilesInRegion[i];
                float distance = currTile.GetDistanceTo(chosenRegion.centerOfMass);
                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    tileToUse = currTile;
                }
            }
        }

        if (tileToUse != null) {
            CreateNewLandmarkOnTile(tileToUse, LANDMARK_TYPE.DEMONIC_PORTAL);
        } else {
            throw new System.Exception("Cannot create Demonic Portal for Player!");
        }
    }
    public bool GenerateFactionLandmarks() {
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            Tribe currTribe = FactionManager.Instance.allTribes[i];
            LEVEL wealth = LEVEL.HIGH;
            LEVEL population = LEVEL.HIGH;
            LEVEL might = LEVEL.HIGH;
            LEVEL needProviders = LEVEL.HIGH;
            LEVEL characters = LEVEL.HIGH;
            Dictionary<LANDMARK_TYPE, int> landmarkSettings = GetLandmarkSettings(wealth, population, might, needProviders, currTribe);
            if (!GenerateLandmarksForFaction(landmarkSettings, currTribe)) {
                return false;
            }
        }
        return true;
    }
    private Dictionary<LANDMARK_TYPE, int> GetLandmarkSettings(LEVEL wealthLvl, LEVEL populationLvl, LEVEL mightLvl, LEVEL needLvl, Tribe tribe) {
        Dictionary<LANDMARK_TYPE, int> landmarkSettings = new Dictionary<LANDMARK_TYPE, int>();
        AddWealthSettings(wealthLvl, landmarkSettings);
        AddPopulationSettings(populationLvl, landmarkSettings, tribe);
        AddMightSettings(mightLvl, landmarkSettings);
        AddNeedSettings(needLvl, landmarkSettings);
        return landmarkSettings;
    }
    private void AddWealthSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings) {
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 2);
                landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 2);
                break;
            case LEVEL.AVERAGE:
                if (Random.Range(0, 2) == 0) {
                    landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 1);
                    landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 2);
                } else {
                    landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 2);
                    landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 1);
                }
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(LANDMARK_TYPE.IRON_MINES, 1);
                landmarksSettings.Add(LANDMARK_TYPE.OAK_LUMBERYARD, 1);
                break;
            default:
                break;
        }
    }
    private void AddPopulationSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings, Tribe tribe) {
        LANDMARK_TYPE settlementTypeToUse = LANDMARK_TYPE.HUMAN_SETTLEMENT;
        if (tribe.race == RACE.ELVES) {
            settlementTypeToUse = LANDMARK_TYPE.ELVEN_SETTLEMENT;
        }
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(settlementTypeToUse, 6);
                break;
            case LEVEL.AVERAGE:
                landmarksSettings.Add(settlementTypeToUse, 4);
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(settlementTypeToUse, 2);
                break;
            default:
                break;
        }
    }
    private void AddMightSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings) {
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(LANDMARK_TYPE.OAK_FORTIFICATION, 4);
                landmarksSettings.Add(LANDMARK_TYPE.IRON_FORTIFICATION, 3);
                break;
            case LEVEL.AVERAGE:
                landmarksSettings.Add(LANDMARK_TYPE.OAK_FORTIFICATION, 3);
                landmarksSettings.Add(LANDMARK_TYPE.IRON_FORTIFICATION, 2);
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(LANDMARK_TYPE.OAK_FORTIFICATION, 2);
                landmarksSettings.Add(LANDMARK_TYPE.IRON_FORTIFICATION, 1);
                break;
            default:
                break;
        }
    }
    private void AddNeedSettings(LEVEL level, Dictionary<LANDMARK_TYPE, int> landmarksSettings) {
        switch (level) {
            case LEVEL.HIGH:
                landmarksSettings.Add(LANDMARK_TYPE.INN, 2);
                landmarksSettings.Add(LANDMARK_TYPE.HUNTING_GROUNDS, 2);
                landmarksSettings.Add(LANDMARK_TYPE.PUB, 2);
                landmarksSettings.Add(LANDMARK_TYPE.TEMPLE, 2);
                break;
            case LEVEL.AVERAGE:
                landmarksSettings.Add(LANDMARK_TYPE.INN, 2);
                landmarksSettings.Add(LANDMARK_TYPE.HUNTING_GROUNDS, 2);
                landmarksSettings.Add(LANDMARK_TYPE.PUB, 1);
                landmarksSettings.Add(LANDMARK_TYPE.TEMPLE, 1);
                break;
            case LEVEL.LOW:
                landmarksSettings.Add(LANDMARK_TYPE.INN, 1);
                landmarksSettings.Add(LANDMARK_TYPE.HUNTING_GROUNDS, 1);
                landmarksSettings.Add(LANDMARK_TYPE.PUB, 1);
                landmarksSettings.Add(LANDMARK_TYPE.TEMPLE, 1);
                break;
            default:
                break;
        }
    }
    private LEVEL RandomizeLevel(int highPercent, int averagePercent, int lowPercent) {
        int chance = Random.Range(0, 100);
        if (chance <= lowPercent) {
            return LEVEL.LOW;
        } else if (chance > lowPercent && chance <= averagePercent) {
            return LEVEL.AVERAGE;
        } else {
            return LEVEL.HIGH;
        }
    }
    private bool GenerateLandmarksForFaction(Dictionary<LANDMARK_TYPE, int> landmarkSettings, Tribe tribe) {
        Region tribeRegion = tribe.ownedRegions[0];
        List<HexTile> elligibleTiles = tribeRegion.tilesInRegion.Where(x => x.CanBuildLandmarkHere()).ToList();
        string log = "Created " + landmarkSettings.Sum(x => x.Value).ToString() + " landmarks on " + tribeRegion.name;
        foreach (KeyValuePair<LANDMARK_TYPE, int> kvp in landmarkSettings) {
            LANDMARK_TYPE landmarkType = kvp.Key;
            LandmarkData data = GetLandmarkData(landmarkType);
            log += "\n" + landmarkType.ToString() + " - " + kvp.Value.ToString();
            for (int i = 0; i < kvp.Value; i++) {
                List<HexTile> tilesToChooseFrom = elligibleTiles.Where(x => data.possibleSpawnPoints.Contains(x.passableType)).ToList();
                if (tilesToChooseFrom.Count <= 0) {
                    //Debug.LogError("There are no more tiles in " + tribeRegion.name + " to build a " + landmarkType.ToString(), tribeRegion.centerOfMass);
                    return false;
                }
                HexTile chosenTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
                CreateNewLandmarkOnTile(chosenTile, landmarkType);
                //remove the tiles within 1 range of the chosen tile from elligible tiles
                elligibleTiles.Remove(chosenTile);
                Utilities.ListRemoveRange(elligibleTiles, chosenTile.GetTilesInRange(1));
            }
        }
        Debug.Log(log, tribeRegion.centerOfMass);
        return true;
    }

  //  /*
  //   Generate new landmarks (Lairs, Dungeons)
  //       */
  //  public void GenerateOtherLandmarks() {
		//AddAllCenterOfMassToRegionLandmarksList ();
  //      GenerateDungeonLandmarks();
  //      GenerateSettlementLandmarks();
  //  }
    public void InitializeLandmarks() {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                currLandmark.Initialize();
            }
        }
    }
    //private WeightedDictionary<LANDMARK_TYPE> GetLandmarkAppearanceWeights(Region region) {
    //    WeightedDictionary<LANDMARK_TYPE> landmarkAppearanceWeights = new WeightedDictionary<LANDMARK_TYPE>();
    //    for (int i = 0; i < landmarkData.Count; i++) {
    //        LandmarkData currData = landmarkData[i];
    //        if (currData.onOccupiedOnly && !region.isOwned) {
    //            continue; //skip
    //        }
    //        if (currData.isUnique && HasLandmarkOfType(currData.landmarkType)) {
    //            continue; //skip
    //        }
    //        //landmarkAppearanceWeights.AddElement(currData.landmarkType, currData.appearanceWeight);
    //    }
    //    return landmarkAppearanceWeights;
    //}
    public LandmarkData GetLandmarkData(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < landmarkData.Count; i++) {
            LandmarkData currData = landmarkData[i];
            if (currData.landmarkType == landmarkType) {
                return currData;
            }
        }
        throw new System.Exception("There is no landmark data for " + landmarkType.ToString());
    }
	private void AddInitialLandmarkItems(BaseLandmark landmark){
		//List<ECS.Item> items = new List<ECS.Item> ();
		//switch(landmark.specificLandmarkType){
		//case LANDMARK_TYPE.CAVE:
		//	ECS.Item neuroctus = ItemManager.Instance.CreateNewItemInstance ("Neuroctus");
		//	items.Add (neuroctus);
		//	break;
		//}
		//landmark.AddItemsInLandmark (items);
	}
    #endregion

    #region ECS.Character Production
    /*
     Get the character role weights for a faction.
     This will not include roles that the faction has already reached the cap of.
         */
    public WeightedDictionary<CHARACTER_ROLE> GetCharacterRoleProductionDictionary() {
        WeightedDictionary<CHARACTER_ROLE> characterWeights = new WeightedDictionary<CHARACTER_ROLE>();
        for (int i = 0; i < characterProductionWeights.Count; i++) {
            CharacterProductionWeight currWeight = characterProductionWeights[i];
			characterWeights.AddElement(currWeight.role, currWeight.weight);
        }
        return characterWeights;
    }
    public WeightedDictionary<CHARACTER_CLASS> GetCharacterClassProductionDictionary(BaseLandmark landmark) {
        WeightedDictionary<CHARACTER_CLASS> classes = new WeightedDictionary<CHARACTER_CLASS>();
        CHARACTER_CLASS[] allClasses = Utilities.GetEnumValues<CHARACTER_CLASS>();
   //     Settlement settlement = null;
   //     if (landmark is Settlement) {
   //         settlement = landmark as Settlement;
   //     } else {
			//settlement = landmark.tileLocation.region.mainLandmark as Settlement;
   //     }
        for (int i = 1; i < allClasses.Length; i++) {
            CHARACTER_CLASS charClass = allClasses[i];
            if (landmark.CanProduceClass(charClass)) { //Does the settlement have the required technologies to produce this class
                classes.AddElement(charClass, 200);
            }
        }
        return classes;
    }
    #endregion

    #region Material Generation
    /*
     Generate Materials
         */
    public void GenerateMaterials() {
        List<HexTile> elligibleTiles = new List<HexTile>(GridMap.Instance.hexTiles.Where(x => x.elevationType != ELEVATION.WATER && !x.isHabitable && !x.isRoad)); //Get tiles that aren't water and are not habitable
        //Tiles that are within 2 tiles of a habitable tile, cannot have resources
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            List<HexTile> tilesToRemove = currRegion.centerOfMass.GetTilesInRange(2);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);
        }

        WeightedDictionary<MATERIAL> materialWeights = Utilities.GetMaterialWeights();

        //Dictionary<MATERIAL, int> createdMaterials = new Dictionary<MATERIAL, int>();
        Debug.Log("Creating " + initialResourceLandmarks.ToString() + " materials..... ");
        int placedMaterials = 0;

        while (placedMaterials != initialResourceLandmarks) {
            if (elligibleTiles.Count <= 0) {
                Debug.Log("Only created " + placedMaterials.ToString() + " landmarks");
                return;
            }
            HexTile chosenTile = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            elligibleTiles.Remove(chosenTile);
            List<HexTile> tilesToRemove = chosenTile.GetTilesInRange(1);
            Utilities.ListRemoveRange(elligibleTiles, tilesToRemove);

            MATERIAL chosenMaterial = materialWeights.PickRandomElementGivenWeights();
            chosenTile.SetMaterialOnTile(chosenMaterial);

            ////Keep track of number of materials per type
            //if (createdMaterials.ContainsKey(chosenMaterial)) {
            //    createdMaterials[chosenMaterial]++;
            //} else {
            //    createdMaterials.Add(chosenMaterial, 1);
            //}
            placedMaterials++;
        }
        Debug.Log("Created " + placedMaterials.ToString() + " materials");

        //foreach (KeyValuePair<MATERIAL, int> kvp in createdMaterials) {
        //    Debug.Log(kvp.Key.ToString() + " - " + kvp.Value.ToString());
        //}
    }
    #endregion

    #region Utilities
    public BASE_LANDMARK_TYPE GetBaseLandmarkType(LANDMARK_TYPE landmarkType) {
        LandmarkData landmarkData = GetLandmarkData(landmarkType);
        return landmarkData.baseLandmarkType;
    }
    public BaseLandmark GetLandmarkByID(int id) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.mainLandmark.id == id) {
                return currRegion.mainLandmark;
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.id == id) {
                    return currLandmark;
                }
            }
        }
        return null;
    }
    public BaseLandmark GetLandmarkByName(string name) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.mainLandmark.landmarkName == name) {
                return currRegion.mainLandmark;
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.landmarkName == name) {
                    return currLandmark;
                }
            }
        }
        return null;
    }
    public bool HasLandmarkOfType(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.specificLandmarkType == landmarkType) {
                    return true;
                }
            }
        }
        return false;
    }
    public List<BaseLandmark> GetLandmarksOfType(LANDMARK_TYPE landmarkType) {
        List<BaseLandmark> allLandmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (currLandmark.specificLandmarkType == landmarkType) {
                    allLandmarksOfType.Add(currLandmark);
                }
            }
        }
        return allLandmarksOfType;
    }
    public List<BaseLandmark> GetLandmarksOfType(BASE_LANDMARK_TYPE baseLandmarkType) {
        List<BaseLandmark> allLandmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (GetBaseLandmarkType(currLandmark.specificLandmarkType) == baseLandmarkType) {
                    allLandmarksOfType.Add(currLandmark);
                }
            }
        }
        return allLandmarksOfType;
    }
    public List<BaseLandmark> GetLandmarksOfType(BASE_LANDMARK_TYPE baseLandmarkType, List<Region> except) {
        List<BaseLandmark> allLandmarksOfType = new List<BaseLandmark>();
        for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (except.Contains(currRegion)) {
                continue; //skip
            }
            for (int j = 0; j < currRegion.landmarks.Count; j++) {
                BaseLandmark currLandmark = currRegion.landmarks[j];
                if (GetBaseLandmarkType(currLandmark.specificLandmarkType) == baseLandmarkType) {
                    allLandmarksOfType.Add(currLandmark);
                }
            }
        }
        return allLandmarksOfType;
    }
    public BaseLandmarkData GetBaseLandmarkData(BASE_LANDMARK_TYPE baseLandmarkType) {
        for (int i = 0; i < baseLandmarkData.Count; i++) {
            BaseLandmarkData currData = baseLandmarkData[i];
            if (currData.baseLandmarkType == baseLandmarkType) {
                return currData;
            }
        }
        throw new System.Exception("There is no base landmark data for " + baseLandmarkType);
    }
    public List<BaseLandmark> GetAllLandmarks(List<Region> regions = null) {
        List<BaseLandmark> allLandmarks = new List<BaseLandmark>();
        List<Region> choices = GridMap.Instance.allRegions;
        if (regions != null) {
            choices = regions;
        }
        for (int i = 0; i < choices.Count; i++) {
            Region currRegion = choices[i];
            allLandmarks.AddRange(currRegion.landmarks);
        }
        return allLandmarks;
    }
    #endregion
}
