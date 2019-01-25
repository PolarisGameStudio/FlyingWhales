﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

public class AttributeManager : MonoBehaviour {
    public static AttributeManager Instance;

    private Dictionary<string, Trait> _allTraits;
    private Dictionary<string, Trait> _allPositiveTraits;
    private Dictionary<string, Trait> _allIlnesses;

    #region getters/setters
    public Dictionary<string, Trait> allTraits {
        get { return _allTraits; }
    }
    public Dictionary<string, Trait> allPositiveTraits {
        get { return _allPositiveTraits; }
    }
    public Dictionary<string, Trait> allIllnesses {
        get { return _allIlnesses; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allTraits = new Dictionary<string, Trait>();
        _allPositiveTraits = new Dictionary<string, Trait>();
        _allIlnesses = new Dictionary<string, Trait>();
        string path = Utilities.dataPath + "CombatAttributes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait attribute = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            _allTraits.Add(attribute.name, attribute);
            if(attribute.effect == TRAIT_EFFECT.POSITIVE) {
                _allPositiveTraits.Add(attribute.name, attribute);
            } else if (attribute.type == TRAIT_TYPE.ILLNESS) {
                _allIlnesses.Add(attribute.name, attribute);
            }
        }
    }
    public Action<Character> GetBehavior(ATTRIBUTE_BEHAVIOR type) {
        switch (type) {
            case ATTRIBUTE_BEHAVIOR.NONE:
            return null;
        }
        return null;
    }
    public string GetRandomPositiveTrait() {
        int random = UnityEngine.Random.Range(0, _allPositiveTraits.Count);
        int count = 0;
        foreach (string traitName in _allPositiveTraits.Keys) {
            if (count == random) {
                return traitName;
            }
            count++;
        }
        return string.Empty;
    }

    public string GetRandomIllness() {
        //TODO: Optimize this for performance
        int random = UnityEngine.Random.Range(0, _allIlnesses.Count);
        return _allIlnesses.Keys.ElementAt(random);
    }
}
