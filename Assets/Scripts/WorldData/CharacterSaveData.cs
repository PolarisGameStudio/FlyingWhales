﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData {
    public int id;
    public string name;
    public RACE race;
    public GENDER gender;
    public CHARACTER_ROLE role;
    public CHARACTER_JOB job;
    public string className;
    public LOCATION_IDENTIFIER locationType;
    public int locationID;
    public int homeID;
    public int homeLandmarkID;
    public int factionID;
    public PortraitSettings portraitSettings;
    public List<string> equipmentData;
    public List<string> inventoryData;
    public List<RelationshipSaveData> relationshipsData;

    public CharacterSaveData(ECS.Character character) {
        id = character.id;
        name = character.name;
        race = character.raceSetting.race;
        gender = character.gender;
        role = character.role.roleType;
        if (character.role != null) {
            job = character.role.job.jobType;
        } else {
            job = CHARACTER_JOB.NONE;
        }
        
        className = character.characterClass.className;

        if (character.party.specificLocation != null) {
            locationType = character.party.specificLocation.locIdentifier;
            locationID = character.party.specificLocation.id;
        } else {
            locationID = -1;
        }

        if (character.home != null) {
            homeID = character.home.id;
        } else {
            homeID = -1;
        }

        if (character.homeLandmark != null) {
            homeLandmarkID = character.homeLandmark.id;
        } else {
            homeLandmarkID = -1;
        }

        if (character.faction != null) {
            factionID = character.faction.id;
        } else {
            factionID = -1;
        }
        
        portraitSettings = character.portraitSettings;

        equipmentData = new List<string>();
        for (int i = 0; i < character.equippedItems.Count; i++) {
            ECS.Item item = character.equippedItems[i];
            equipmentData.Add(item.itemName);
        }

        inventoryData = new List<string>();
        for (int i = 0; i < character.inventory.Count; i++) {
            ECS.Item item = character.inventory[i];
            inventoryData.Add(item.itemName);
        }

        relationshipsData = new List<RelationshipSaveData>();
        foreach (KeyValuePair<ECS.Character, Relationship> kvp in character.relationships) {
            relationshipsData.Add(new RelationshipSaveData(kvp.Value));
        }
    }
}
