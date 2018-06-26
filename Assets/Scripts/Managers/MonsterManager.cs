﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour {
    public static MonsterManager Instance = null;

    private Dictionary<string, Monster> _monstersDictionary;
    public MonsterComponent monsterComponent;

    public GameObject monsterIconPrefab;

    public List<Monster> allMonsters;

    #region getters/setters
    public Dictionary<string, Monster> monstersDictionary {
        get { return _monstersDictionary; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        allMonsters = new List<Monster>();
    }

    public void Initialize() {
        ConstructAllMonsters();
    }

    private void ConstructAllMonsters() {
        _monstersDictionary = new Dictionary<string, Monster>();
        string path = Utilities.dataPath + "Monsters/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(classes[i]), monsterComponent);
            Monster monster = new Monster();
            monster.SetData(monsterComponent);
            _monstersDictionary.Add(monster.name, monster);
        }
    }
    public Monster CreateNewMonster(string monsterName) {
        Monster newMonster = _monstersDictionary[monsterName].CreateNewCopy();
        newMonster.Initialize();
        allMonsters.Add(newMonster);
        return newMonster;
    }
    public Monster CreateNewMonster(MonsterSaveData data) {
        Monster newMonster = _monstersDictionary[data.monsterName].CreateNewCopy();
        newMonster.Initialize(data);
        allMonsters.Add(newMonster);
        return newMonster;
    }
    public Monster SpawnMonsterOnTile(HexTile tile, string monsterName) {
        Monster newMonster = CreateNewMonster(monsterName);
        newMonster.CreateIcon();
        newMonster.icon.SetPosition(tile.transform.position);
        newMonster.SetSpecificLocation(tile);
        return newMonster;
    }
    public Monster SpawnMonsterOnTile(HexTile tile, MonsterSaveData data) {
        Monster newMonster = CreateNewMonster(data);
        newMonster.CreateIcon();
        newMonster.icon.SetPosition(tile.transform.position);
        newMonster.SetSpecificLocation(tile);
        return newMonster;
    }
    public Monster SpawnMonsterOnLandmark(BaseLandmark landmark, string monsterName) {
        Monster newMonster = CreateNewMonster(monsterName);
        newMonster.CreateIcon();
        newMonster.icon.SetPosition(landmark.tileLocation.transform.position);
        landmark.AddCharacterToLocation(newMonster);
        return newMonster;
    }
    public Monster SpawnMonsterOnLandmark(BaseLandmark landmark, MonsterSaveData data) {
        Monster newMonster = CreateNewMonster(data);
        newMonster.CreateIcon();
        newMonster.icon.SetPosition(landmark.tileLocation.transform.position);
        landmark.AddCharacterToLocation(newMonster);
        return newMonster;
    }
    public void RemoveMonster(Monster monster) {
        allMonsters.Remove(monster);
        GameObject.Destroy(monster.icon.gameObject);
    }

    public bool HasMonsterOnTile(HexTile tile) {
        for (int i = 0; i < allMonsters.Count; i++) {
            Monster currMonster = allMonsters[i];
            if (currMonster.specificLocation.locIdentifier == LOCATION_IDENTIFIER.HEXTILE && currMonster.specificLocation.id == tile.id) {
                return true;
            }
        }
        return false;
    }
    public List<Monster> GetMonstersOnTile(HexTile tile) {
        List<Monster> monsters = new List<Monster>();
        for (int i = 0; i < allMonsters.Count; i++) {
            Monster currMonster = allMonsters[i];
            if (currMonster.specificLocation.locIdentifier == LOCATION_IDENTIFIER.HEXTILE && currMonster.specificLocation.id == tile.id) {
                monsters.Add(currMonster);
            }
        }
        return monsters;
    }
    public void RemoveMonstersOnTile(HexTile tile) {
        List<Monster> monsters = GetMonstersOnTile(tile);
        for (int i = 0; i < monsters.Count; i++) {
            RemoveMonster(monsters[i]);
        }
    }
    public void LoadMonsters(WorldSaveData data) {
        if (data.monstersData != null) {
            for (int i = 0; i < data.monstersData.Count; i++) {
                MonsterSaveData monsterData = data.monstersData[i];
                if (monsterData.locationType == LOCATION_IDENTIFIER.HEXTILE) {
#if WORLD_CREATION_TOOL
                    HexTile tile = worldcreator.WorldCreatorManager.Instance.GetHexTile(monsterData.locationID);
#else
                    HexTile tile = GridMap.Instance.GetHexTile(monsterData.locationID);
#endif
                    SpawnMonsterOnTile(tile, monsterData);
                } else if (monsterData.locationType == LOCATION_IDENTIFIER.LANDMARK) {
                    BaseLandmark landmark = LandmarkManager.Instance.GetLandmarkByID(monsterData.locationID);
                    SpawnMonsterOnLandmark(landmark, monsterData);
                }
            }
        }
    }

    public Monster GetMonsterByID(int id) {
        for (int i = 0; i < allMonsters.Count; i++) {
            Monster currMonster = allMonsters[i];
            if (currMonster.id == id) {
                return currMonster;
            }
        }
        return null;
    }
}
