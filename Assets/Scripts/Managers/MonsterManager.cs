﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour {
    public static MonsterManager Instance = null;

    private Dictionary<string, Monster> _monstersDictionary;

    #region getters/setters
    public Dictionary<string, Monster> monstersDictionary {
        get { return _monstersDictionary; }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ConstructAllMonsters();
    }

    private void ConstructAllMonsters() {
        _monstersDictionary = new Dictionary<string, Monster>();
        string path = Utilities.dataPath + "Monsters/";
        string[] classes = System.IO.Directory.GetFiles(path, "*.json");
        for (int i = 0; i < classes.Length; i++) {
            MonsterComponent monsterComponent = JsonUtility.FromJson<MonsterComponent>(System.IO.File.ReadAllText(classes[i]));
            Monster monster = new Monster();
            monster.SetData(monsterComponent);
            _monstersDictionary.Add(monster.name, monster);
        }
    }
}
