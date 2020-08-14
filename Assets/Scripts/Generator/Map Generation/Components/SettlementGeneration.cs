﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Tile_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class SettlementGeneration : MapGenerationComponent {
	
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Creating settlements...");
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasTileWithFeature(TileFeatureDB.Inhabited_Feature)) {
				yield return MapGenerator.Instance.StartCoroutine(CreateSettlement(region, data));
			}
			// region.innerMap.PlaceBuildSpotTileObjects();
		}
		ApplyPreGeneratedRelationships(data);
		yield return null;
	}

	#region Scenario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		if (scenarioMapData.villageSettlementTemplates != null) {
			for (int i = 0; i < scenarioMapData.villageSettlementTemplates.Length; i++) {
				SettlementTemplate settlementTemplate = scenarioMapData.villageSettlementTemplates[i];
				HexTile[] tilesInSettlement = settlementTemplate.GetTilesInTemplate(GridMap.Instance.map);

				Region region = tilesInSettlement[0].region;
				
				//create village landmark on settlement tiles
				for (int j = 0; j < tilesInSettlement.Length; j++) {
					HexTile villageTile = tilesInSettlement[j];
					LandmarkManager.Instance.CreateNewLandmarkOnTile(villageTile, LANDMARK_TYPE.VILLAGE);
				}
				
				//create faction
				FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(settlementTemplate.factionRace);
				Faction faction = FactionManager.Instance.CreateNewFaction(factionType);
				faction.factionType.SetAsDefault();
				
				LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
			
				NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement(region, locationType, tilesInSettlement);
				npcSettlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
				LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
				
				StructureSetting[] structureSettings = settlementTemplate.structureSettings;
				yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltStructuresForSettlement(npcSettlement, region.innerMap, structureSettings));
				yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjects());
				
				int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
				//Add combatant classes from faction type to location class manager
				for (int j = 0; j < faction.factionType.combatantClasses.Count; j++) {
					npcSettlement.classManager.AddCombatantClass(faction.factionType.combatantClasses[j]);
				}
				List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data, settlementTemplate.minimumVillagerCount);

				//update objects owners in dwellings
				List<TileObject> objectsInDwellings = npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
				for (int j = 0; j < objectsInDwellings.Count; j++) {
					TileObject tileObject = objectsInDwellings[j];
					tileObject.UpdateOwners();
				}
			
				CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);
				npcSettlement.Initialize();
				yield return null;
			}
		}
	}
	#endregion
	
	#region Saved World
	public override IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData) {
		yield return MapGenerator.Instance.StartCoroutine(ExecuteRandomGeneration(data));
	}
	#endregion
	
	private IEnumerator CreateSettlement(Region region, MapGenerationData data) {
		List<HexTile> settlementTiles = region.GetTilesWithFeature(TileFeatureDB.Inhabited_Feature);
		if (WorldConfigManager.Instance.isTutorialWorld) {
			Assert.IsTrue(settlementTiles.Count == 4, "Settlement tiles of demo build is not 4!");
		}

		//create village landmark on settlement tiles
		for (int i = 0; i < settlementTiles.Count; i++) {
			HexTile villageTile = settlementTiles[i];
			LandmarkManager.Instance.CreateNewLandmarkOnTile(villageTile, LANDMARK_TYPE.VILLAGE);
		}

		List<RACE> validRaces = WorldConfigManager.Instance.isTutorialWorld
			? new List<RACE>() {RACE.HUMANS, RACE.ELVES}
			: WorldSettings.Instance.worldSettingsData.races;

		RACE neededRace = GetFactionRaceForRegion(region);
		if (validRaces.Contains(neededRace)) {
			Faction faction = GetFactionToOccupySettlement(neededRace);
			LOCATION_TYPE locationType = GetLocationTypeForRace(faction.race);
			
			NPCSettlement npcSettlement = LandmarkManager.Instance.CreateNewSettlement
				(region, locationType, settlementTiles.ToArray());
			npcSettlement.AddStructure(region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
			LandmarkManager.Instance.OwnSettlement(faction, npcSettlement);
			List<StructureSetting> structureTypes;
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
				structureTypes = new List<StructureSetting>() {
					new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE), 
					new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 
					new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE), 
					new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE)
				};
			} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
				structureTypes = new List<StructureSetting>() {
					new StructureSetting(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE), 
					new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE),
					new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
				};
			} else {
				structureTypes = GenerateStructures(npcSettlement, faction);
			}
			yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltStructuresForSettlement(npcSettlement, region.innerMap, structureTypes.ToArray()));
			yield return MapGenerator.Instance.StartCoroutine(npcSettlement.PlaceInitialObjects());

			int dwellingCount = npcSettlement.structures[STRUCTURE_TYPE.DWELLING].Count;
			//Add combatant classes from faction type to location class manager
			for (int i = 0; i < faction.factionType.combatantClasses.Count; i++) {
				npcSettlement.classManager.AddCombatantClass(faction.factionType.combatantClasses[i]);
			}
			List<Character> spawnedCharacters = GenerateSettlementResidents(dwellingCount, npcSettlement, faction, data);
			

			List<TileObject> objectsInDwellings =
				npcSettlement.GetTileObjectsFromStructures<TileObject>(STRUCTURE_TYPE.DWELLING, o => true);
			for (int i = 0; i < objectsInDwellings.Count; i++) {
				TileObject tileObject = objectsInDwellings[i];
				tileObject.UpdateOwners();
			}
			
			CharacterManager.Instance.PlaceInitialCharacters(spawnedCharacters, npcSettlement);
			npcSettlement.Initialize();
			
		}
	}

	#region Settlement Structures
	private List<StructureSetting> GenerateStructures(NPCSettlement settlement, Faction faction) {
		List<StructureSetting> structures = new List<StructureSetting> { faction.factionType.GetStructureSettingFor(STRUCTURE_TYPE.CITY_CENTER) };
		List<STRUCTURE_TYPE> createdStructureTypes = new List<STRUCTURE_TYPE>();
		for (int i = 1; i < settlement.tiles.Count; i++) {
			HexTile tile = settlement.tiles[i];
			WeightedDictionary<StructureSetting> structuresChoices = GetStructureWeights(tile, createdStructureTypes, faction);
			StructureSetting chosenSetting = structuresChoices.PickRandomElementGivenWeights();
			structures.Add(chosenSetting);
			createdStructureTypes.Add(chosenSetting.structureType);
		}
		return structures;
	}
	private WeightedDictionary<StructureSetting> GetStructureWeights(HexTile tile, List<STRUCTURE_TYPE> structureTypes, Faction faction) {
		WeightedDictionary<StructureSetting> structureWeights = new WeightedDictionary<StructureSetting>();
		if (faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
			if (structureTypes.Contains(STRUCTURE_TYPE.APOTHECARY) == false) {
				//Apothecary: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.APOTHECARY, RESOURCE.WOOD), 6);
			}
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD), 1); //Farm: +1
			if (tile.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.WOOD),
					structureTypes.Contains(STRUCTURE_TYPE.FARM) == false ? 15 : 2);
			}
			if (tile.HasNeighbourWithFeature(TileFeatureDB.Wood_Source_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD),
					structureTypes.Contains(STRUCTURE_TYPE.LUMBERYARD) == false ? 15 : 2);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.CEMETERY) == false) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD), 2);
			}
		} else if (faction.factionType.type == FACTION_TYPE.Human_Empire) {
			if (structureTypes.Contains(STRUCTURE_TYPE.MAGE_QUARTERS) == false) {
				//Mage Quarter: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE), 6);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.PRISON) == false) {
				//Prison: +3 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.PRISON, RESOURCE.STONE), 3); //3
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.BARRACKS) == false) {
				//Barracks: +6 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE));
			}
			structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE), 1); //Farm: +1
			if (tile.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, RESOURCE.STONE),
					structureTypes.Contains(STRUCTURE_TYPE.FARM) == false ? 15 : 2);
			}
			if (tile.HasNeighbourWithFeature(TileFeatureDB.Metal_Source_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE),
					structureTypes.Contains(STRUCTURE_TYPE.MINE_SHACK) == false ? 15 : 2);
			}
			if (tile.HasNeighbourWithFeature(TileFeatureDB.Game_Feature)) {
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE),
					structureTypes.Contains(STRUCTURE_TYPE.HUNTER_LODGE) == false ? 15 : 2);
			}
			if (structureTypes.Contains(STRUCTURE_TYPE.CEMETERY) == false) {
				//Wooden Graveyard: +2 (disable if already selected from previous hex tile)
				structureWeights.AddElement(new StructureSetting(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE), 2);
			}
		}
		return structureWeights;
	}
	#endregion

	#region Residents
	private List<Character> GenerateSettlementResidents(int dwellingCount, NPCSettlement npcSettlement, Faction faction, MapGenerationData data, int providedCitizenCount = -1) {
		List<Character> createdCharacters = new List<Character>();
		int citizenCount = 0;
		for (int i = 0; i < dwellingCount; i++) {
			int roll = Random.Range(0, 100);
			
			int coupleChance = 35;
			if (providedCitizenCount > 0) {
				if (citizenCount >= providedCitizenCount) {
					break;
				}
				
				//if number of citizens are provided, check if the current citizen count + 2 (Couple), is still less than the given amount
				//if it is, then increase chance to spawn a couple
				int afterCoupleGenerationAmount = citizenCount + 2;
				if (afterCoupleGenerationAmount < providedCitizenCount) {
					coupleChance = 70;
				}
			}
			
			List<Dwelling> availableDwellings = GetAvailableDwellingsAtSettlement(npcSettlement);
			if (availableDwellings.Count == 0) {
				break; //no more dwellings
			}

			Dwelling dwelling = CollectionUtilities.GetRandomElement(availableDwellings);
			if (roll < coupleChance) {
				//couple
				List<Couple> couples = GetAvailableCouplesToBeSpawned(faction.race, data);
				if (couples.Count > 0) {
					Couple couple = CollectionUtilities.GetRandomElement(couples);
					createdCharacters.AddRange(SpawnCouple(couple, dwelling, faction, npcSettlement));
					citizenCount += 2;
				} else {
					//no more couples left	
					List<Couple> siblingCouples = GetAvailableSiblingCouplesToBeSpawned(faction.race, data);
					if (siblingCouples.Count > 0) {
						Couple couple = CollectionUtilities.GetRandomElement(siblingCouples);
						createdCharacters.AddRange( SpawnCouple(couple, dwelling, faction, npcSettlement));
						citizenCount += 2;
					} else {
						//no more sibling Couples	
						PreCharacterData singleCharacter =
							GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
						if (singleCharacter != null) {
							createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, npcSettlement));
							citizenCount += 1;
						} else {
							//no more characters to spawn
							Debug.LogWarning("Could not find any more characters to spawn. Generating a new family tree.");
							FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
							data.familyTreeDatabase.AddFamilyTree(newFamily);
							singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
							Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
							createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
								dwelling, faction, npcSettlement));
							citizenCount += 1;
						}
					}
				}
			} else {
				//single
				PreCharacterData singleCharacter =
					GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
				if (singleCharacter != null) {
					createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, npcSettlement));
					citizenCount += 1;
				} else {
					//no more characters to spawn
					Debug.LogWarning("Could not find any more characters to spawn");
					FamilyTree newFamily = FamilyTreeGenerator.GenerateFamilyTree(faction.race);
					data.familyTreeDatabase.AddFamilyTree(newFamily);
					singleCharacter = GetAvailableSingleCharacterForSettlement(faction.race, data, npcSettlement);
					Assert.IsNotNull(singleCharacter, $"Generation tried to generate a new family for spawning a needed citizen. But still could not find a single character!");
					createdCharacters.Add(SpawnCharacter(singleCharacter, npcSettlement.classManager.GetCurrentClassToCreate(), 
						dwelling, faction, npcSettlement));
					citizenCount += 1;
				}
			}
		}
		return createdCharacters;
	}
	private List<Couple> GetAvailableCouplesToBeSpawned(RACE race, MapGenerationData data) {
		List<Couple> couples = new List<Couple>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, data.familyTreeDatabase);
					if (lover != null && lover.hasBeenSpawned == false) {
						Couple couple = new Couple(familyMember, lover);
						if (couples.Contains(couple) == false) {
							couples.Add(couple);
						}
					}
				}
			}
		}
		return couples;
	}
	private List<Couple> GetAvailableSiblingCouplesToBeSpawned(RACE race, MapGenerationData data) {
		List<Couple> couples = new List<Couple>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			if (familyTree.children != null && familyTree.children.Count >= 2) {
				List<PreCharacterData> unspawnedChildren = familyTree.children.Where(x => x.hasBeenSpawned == false).ToList();
				if (unspawnedChildren.Count >= 2) {
					PreCharacterData random1 = CollectionUtilities.GetRandomElement(unspawnedChildren);
					unspawnedChildren.Remove(random1);
					PreCharacterData random2 = CollectionUtilities.GetRandomElement(unspawnedChildren);
					Couple couple = new Couple(random1, random2);
					if (couples.Contains(couple) == false) {
						couples.Add(couple);
					}
				}
			}
		}
		return couples;
	}
	private PreCharacterData GetAvailableSingleCharacterForSettlement(RACE race, MapGenerationData data, NPCSettlement npcSettlement) {
		List<PreCharacterData> availableCharacters = new List<PreCharacterData>();
		List<FamilyTree> familyTrees = data.familyTreesDictionary[race];
		for (int i = 0; i < familyTrees.Count; i++) {
			FamilyTree familyTree = familyTrees[i];
			for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
				PreCharacterData familyMember = familyTree.allFamilyMembers[j];
				if (familyMember.hasBeenSpawned == false) {
					PreCharacterData lover = familyMember.GetCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER, data.familyTreeDatabase);
					//check if the character has a lover, if it does, check if its lover has been spawned, if it has, check that the lover was spawned in a different npcSettlement
					if (lover == null || lover.hasBeenSpawned == false || 
					    CharacterManager.Instance.GetCharacterByID(lover.id).homeSettlement != npcSettlement) {
						availableCharacters.Add(familyMember);
					}
				}
			}
		}

		if (availableCharacters.Count > 0) {
			return CollectionUtilities.GetRandomElement(availableCharacters);
		}
		return null;
	}
	private List<Dwelling> GetAvailableDwellingsAtSettlement(NPCSettlement npcSettlement) {
		List<Dwelling> dwellings = new List<Dwelling>();
		if (npcSettlement.structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
			List<LocationStructure> locationStructures = npcSettlement.structures[STRUCTURE_TYPE.DWELLING];
			for (int i = 0; i < locationStructures.Count; i++) {
				LocationStructure currStructure = locationStructures[i];
				Dwelling dwelling = currStructure as Dwelling;
				if (dwelling.residents.Count == 0) {
					dwellings.Add(dwelling);	
				}
			}
		}
		return dwellings;
	}
	private List<Character> SpawnCouple(Couple couple, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement) {
		List<Character> characters = new List<Character>() {
			SpawnCharacter(couple.character1, npcSettlement.classManager.GetCurrentClassToCreate(), dwelling, faction, npcSettlement),
			SpawnCharacter(couple.character2, npcSettlement.classManager.GetCurrentClassToCreate(), dwelling, faction, npcSettlement)	
		};
		return characters;
	}
	private Character SpawnCharacter(PreCharacterData data, string className, Dwelling dwelling, Faction faction, NPCSettlement npcSettlement) {
		return CharacterManager.Instance.CreateNewCharacter(data, className, faction, npcSettlement, dwelling);
	}
	#endregion

	#region Relationships
	private void ApplyPreGeneratedRelationships(MapGenerationData data) {
		foreach (var pair in data.familyTreesDictionary) {
			for (int i = 0; i < pair.Value.Count; i++) {
				FamilyTree familyTree = pair.Value[i];
				for (int j = 0; j < familyTree.allFamilyMembers.Count; j++) {
					PreCharacterData characterData = familyTree.allFamilyMembers[j];
					if (characterData.hasBeenSpawned) {
						Character character = CharacterManager.Instance.GetCharacterByID(characterData.id);
						foreach (var kvp in characterData.relationships) {
							PreCharacterData targetCharacterData = data.familyTreeDatabase.GetCharacterWithID(kvp.Key);
							IRelationshipData relationshipData = character.relationshipContainer
								.GetOrCreateRelationshipDataWith(character, targetCharacterData.id,
									targetCharacterData.firstName, targetCharacterData.gender);
							
							character.relationshipContainer.SetOpinion(character, targetCharacterData.id, 
								targetCharacterData.firstName, targetCharacterData.gender,
								"Base", kvp.Value.baseOpinion, true);
							
							relationshipData.opinions.SetCompatibilityValue(kvp.Value.compatibility);
							
							for (int k = 0; k < kvp.Value.relationships.Count; k++) {
								RELATIONSHIP_TYPE relationshipType = kvp.Value.relationships[k];
								relationshipData.AddRelationship(relationshipType);
							}
						}
					}
				}
			}
		}
	}
	#endregion

	#region Settlement Generation Utilities
	private RACE GetFactionRaceForRegion(Region region) {
		if (region.coreTile.biomeType == BIOMES.FOREST || region.coreTile.biomeType == BIOMES.SNOW) {
			return RACE.ELVES;
		} else if (region.coreTile.biomeType == BIOMES.DESERT || region.coreTile.biomeType == BIOMES.GRASSLAND) {
			return RACE.HUMANS;
		}
		throw new Exception($"Could not get race type for region with biome type {region.coreTile.biomeType.ToString()}");
	}
	private LOCATION_TYPE GetLocationTypeForRace(RACE race) {
		switch (race) {
			case RACE.HUMANS:
			case RACE.ELVES:
				return LOCATION_TYPE.SETTLEMENT;
			default:
				throw new Exception($"There was no location type provided for race {race.ToString()}");
		}
	}
	private Faction GetFactionToOccupySettlement(RACE race) {
		FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(race);
		List<Faction> factions = FactionManager.Instance.GetMajorFactionWithRace(race);
		Faction chosenFaction;
		if (factions == null) {
			chosenFaction = FactionManager.Instance.CreateNewFaction(factionType);
			chosenFaction.factionType.SetAsDefault();
		} else {
			if (GameUtilities.RollChance(35)) {
				chosenFaction = CollectionUtilities.GetRandomElement(factions);
			} else {
				chosenFaction = FactionManager.Instance.CreateNewFaction(factionType);
				chosenFaction.factionType.SetAsDefault();
			}
		}
		return chosenFaction;
	}
	#endregion
	
}

public class Couple : IEquatable<Couple> {
	public PreCharacterData character1 { get; }
	public PreCharacterData character2 { get; }

	public Couple(PreCharacterData _character1, PreCharacterData _character2) {
		character1 = _character1;
		character2 = _character2;
	}
	public bool Equals(Couple other) {
		if (other == null) {
			return false;
		}
		return (character1.id == other.character1.id && character2.id == other.character2.id) ||
		       (character1.id == other.character2.id && character2.id == other.character1.id);
	}
	public override bool Equals(object obj) {
		return Equals(obj as  Couple);
	}
	public override int GetHashCode() {
		return character1.id + character2.id;
	}
}