﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    public List<Faction> allFactions = new List<Faction>();
    public Faction defaultFaction { get; private set; }

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<Sprite> _emblemBGs;
    [SerializeField] private List<Sprite> _emblems;
    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();

    private void Awake() {
        Instance = this;
    }

    #region Faction Generation
    public void LoadFactions(WorldSaveData data) {
        if (data.factionsData != null) {
            for (int i = 0; i < data.factionsData.Count; i++) {
                FactionSaveData currData = data.factionsData[i];
                Faction currFaction = CreateNewFaction(currData);
#if WORLD_CREATION_TOOL
                worldcreator.WorldCreatorUI.Instance.editFactionsMenu.OnFactionCreated(currFaction);
            }
            worldcreator.WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.LoadFactionDropdownOptions();
#else
            }
#endif
        }
        if (data.HasFactionlessCharacter()) {
            CreateNeutralFaction();
        }
    }
    private void CreateNeutralFaction() {
        Faction newFaction = new Faction();
        allFactions.Add(newFaction);
        defaultFaction = newFaction;
        CreateRelationshipsForFaction(newFaction);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
    }
    /*
     Generate the initital factions,
     races are specified in the inspector (inititalRaces)
     */
    public void GenerateInitialFactions() {
        RACE[] races = new RACE[] { RACE.HUMANS, RACE.ELVES };

        int numOfFactions = 5;
        for (int i = 0; i < numOfFactions; i++) {
            List<Region> elligibleRegions = GridMap.Instance.allRegions.Where(x => x.owner == null).ToList();
            Region chosenRegion = elligibleRegions[Random.Range(0, elligibleRegions.Count)];
            Faction newFaction = CreateNewFaction();
            chosenRegion.SetOwner(newFaction);
            newFaction.OwnRegion(chosenRegion);
            chosenRegion.ReColorBorderTiles(newFaction.factionColor);
            chosenRegion.SetMinimapColor(newFaction.factionColor, 69f / 255f);
        }
    }
    public void GenerateFactionCharacters() {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            for (int j = 0; j < currFaction.ownedLandmarks.Count; j++) {
                BaseLandmark currLandmark = currFaction.ownedLandmarks[j];
                CreateInitialFactionCharacters(currFaction, currLandmark);
            }
            //CreateChieftainForFaction(currTribe);
        }
    }
    /*
     Initital tribes should have a chieftain and a village head.
         */
    private void CreateInitialFactionCharacters(Faction faction, BaseLandmark landmark) {
        int numOfCharacters = Random.Range(1, 3); //Generate 1 to 3 characters in each Village with civilians, limit class based on technologies known by its Faction.
        WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();
        for (int i = 0; i < numOfCharacters; i++) {
            CHARACTER_CLASS chosenClass = CHARACTER_CLASS.WARRIOR;
            CHARACTER_ROLE chosenRole = characterRoleProductionDictionary.PickRandomElementGivenWeights();
            RACE randomRace = RACE.HUMANS;
            if (Random.Range(0, 2) == 1) {
                randomRace = RACE.ELVES;
            }
			ECS.Character newChar = landmark.CreateNewCharacter(randomRace, chosenRole, Utilities.NormalizeString(chosenClass.ToString()));
			//Initial Character tags
			newChar.AssignInitialTags();
        }
    }
	private void EquipFullArmorSet(MATERIAL materialToUse, ECS.Character character){
		if(materialToUse == MATERIAL.NONE){
			return;
		}
		foreach (ARMOR_TYPE armorType in ItemManager.Instance.armorTypeData.Keys) {
			string armorName = Utilities.NormalizeString(materialToUse.ToString()) + " " + Utilities.NormalizeString(armorType.ToString());
			ECS.Item item = ItemManager.Instance.CreateNewItemInstance(armorName);
            if(item != null) {
                character.EquipItem(item);
            }
        }
	}
    public Faction CreateNewFaction(bool isPlayerFaction = false) {
        Faction newFaction = new Faction();
        allFactions.Add(newFaction);
        CreateRelationshipsForFaction(newFaction);
        if (!isPlayerFaction) {
            Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        }
        return newFaction;
    }
    public Faction CreateNewFaction(FactionSaveData data) {
        Faction newFaction = new Faction(data);
        allFactions.Add(newFaction);
        CreateRelationshipsForFaction(newFaction, data);
        Messenger.Broadcast(Signals.FACTION_CREATED, newFaction);
        return newFaction;
    }
    public void DeleteFaction(Faction faction) {
        for (int i = 0; i < faction.ownedAreas.Count; i++) {
            Area ownedArea = faction.ownedAreas[i];
            LandmarkManager.Instance.UnownArea(ownedArea);
        }
        for (int i = 0; i < faction.ownedRegions.Count; i++) {
            Region currRegion = faction.ownedRegions[i];
            currRegion.SetOwner(null);
        }
        RemoveRelationshipsWith(faction);
        Messenger.Broadcast(Signals.FACTION_DELETED, faction);
        allFactions.Remove(faction);
    }
    public void OccupyLandmarksInFactionRegions() {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            for (int j = 0; j < currFaction.ownedRegions.Count; j++) {
                Region currRegion = currFaction.ownedRegions[j];
                for (int k = 0; k < currRegion.landmarks.Count; k++) {
                    BaseLandmark currLandmark = currRegion.landmarks[k];
                    if (!currLandmark.isOccupied) { //currLandmark is Settlement &&
                        currLandmark.OccupyLandmark(currFaction);
                    }
                }
            }
        }
    }
    #endregion

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal Sprite GenerateFactionEmblem(Faction faction) {
        List<Sprite> emblemsToUse = new List<Sprite>(_emblems);
        for (int i = 0; i < emblemsToUse.Count; i++) {
            Sprite currSprite = emblemsToUse[i];
            if (!usedEmblems.Contains(currSprite)) {
                AddEmblemAsUsed(currSprite);
                return currSprite;
            }
        }
        return _emblems[Random.Range(0, _emblems.Count)];
        //throw new System.Exception("There are no more emblems for kingdom: " + faction.name);
    }
    internal Sprite GenerateFactionEmblemBG() {
        return _emblemBGs[Random.Range(0, _emblemBGs.Count)];
    }
    internal void AddEmblemAsUsed(Sprite emblem) {
        if (!usedEmblems.Contains(emblem)) {
            usedEmblems.Add(emblem);
        } else {
            throw new System.Exception("Emblem " + emblem.name + " is already being used!");
        }
    }
    internal void RemoveEmblemAsUsed(Sprite emblem) {
        usedEmblems.Remove(emblem);
    }
    #endregion

    #region Characters
    public List<ECS.Character> GetAllCharactersOfType(CHARACTER_ROLE role) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            characters.AddRange(currFaction.GetCharactersOfType(role));
        }
        return characters;
    }
    public ECS.Character GetCharacterByID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            ECS.Character charInFaction = currFaction.GetCharacterByID(id);
            if(charInFaction != null) {
                return charInFaction;
            }
        }
        return null;
    }
    #endregion

    #region Utilities
    public Faction GetFactionBasedOnID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].id == id) {
                return allFactions[i];
            }
        }
        return null;
    }
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].name.ToLower() == name.ToLower()) {
                return allFactions[i];
            }
        }
        return null;
    }
    #endregion

    #region Relationships
    public void CreateRelationshipsForFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void CreateRelationshipsForFaction(Faction faction, FactionSaveData data) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                FactionRelationship rel = CreateNewRelationshipBetween(otherFaction, faction);
                if (data.relationships.ContainsKey(otherFaction.id)) {
                    rel.ChangeRelationshipStatus(data.relationships[otherFaction.id]);
                }
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
            }
        }
    }
    /*
     Create a new relationship between 2 factions,
     then add add a reference to that relationship, to both of the factions.
         */
    public FactionRelationship CreateNewRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        faction1.AddNewRelationship(faction2, newRel);
        faction2.AddNewRelationship(faction1, newRel);
        return newRel;
    }
    /*
     Utility Function for getting the relationship between 2 factions,
     this just adds a checking for data consistency if, the 2 factions have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each factions GetRelationshipWith() instead.
         */
    public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship faction1Rel = faction1.GetRelationshipWith(faction2);
        FactionRelationship faction2Rel = faction2.GetRelationshipWith(faction1);
        if (faction1Rel == faction2Rel) {
            return faction1Rel;
        }
        throw new System.Exception(faction1.name + " does not have the same relationship object as " + faction2.name + "!");
    }
    public FACTION_RELATIONSHIP_STATUS GetRelationshipStatusBetween(Faction faction1, Faction faction2) {
        FactionRelationship rel = GetRelationshipBetween(faction1, faction2);
        return rel.relationshipStatus;
    }
    public List<Faction> GetFactionsWithByStatus(Faction faction, FACTION_RELATIONSHIP_STATUS status) {
        List<Faction> factions = new List<Faction>();
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in faction.relationships) {
            if (kvp.Value.relationshipStatus == status) {
                factions.Add(kvp.Key);
            }
        }
        return factions;
    }
    #endregion
}
