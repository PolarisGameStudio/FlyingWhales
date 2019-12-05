﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;

public class AlterEgoData : Relatable{

    //relatable
    public override string relatableName { get { return owner.name; } }

    //Basic Data
    public Character owner { get; private set; }
    public string name { get; private set; }
    public Faction faction { get; private set; }
    public RACE race { get; private set; }
    public CharacterRole role { get; private set; }
    public CharacterClass characterClass { get; private set; }
    public Dwelling homeStructure { get; private set; }
    public int level { get; private set; }
    public int attackPowerMod { get; protected set; }
    public int speedMod { get; protected set; }
    public int maxHPMod { get; protected set; }
    public int attackPowerPercentMod { get; protected set; }
    public int speedPercentMod { get; protected set; }
    public int maxHPPercentMod { get; protected set; }

    //Awareness
    //public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; private set; }

    //Traits
    public List<Trait> traits { get; private set; }

    public AlterEgoData(Character owner, string name) {
        this.owner = owner;
        this.name = name;
        faction = null;
        race = RACE.NONE;
        role = null;
        characterClass = null;
        homeStructure = null;
        //awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>();
        traits = new List<Trait>();
        level = 1;
    }

    public void SetFaction(Faction faction) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.faction = faction;
    }
    public void SetRace(RACE race) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.race = race;
    }
    public void SetRole(CharacterRole role) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.role = role;
    }
    public void SetCharacterClass(CharacterClass characterClass) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.characterClass = characterClass;
    }
    public void SetHomeStructure(Dwelling homeStructure) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.homeStructure = homeStructure;
    }
    public void SetLevel(int level) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        this.level = level;
    }
    public void SetAttackPowerMod(int amount) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        attackPowerMod = amount;
    }
    public void SetSpeedMod(int amount) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        speedMod = amount;
    }
    public void SetMaxHPMod(int amount) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        maxHPMod = amount;
    }
    public void SetAttackPowerPercentMod(int amount) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        attackPowerPercentMod = amount;
    }
    public void SetSpeedPercentMod(int amount) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        speedPercentMod = amount;
    }
    public void SetMaxHPPercentMod(int amount) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        maxHPPercentMod = amount;
    }

    #region Awareness
    //public void SetAwareness(Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness) {
    //    if (owner.isSwitchingAlterEgo) {
    //        return; //ignore any changes while the owner is switching alter egos
    //    }
    //    this.awareness = awareness;
    //}
    //public bool AddAwareness(IPointOfInterest pointOfInterest) {
    //    if (!HasAwareness(pointOfInterest)) {
    //        if (!awareness.ContainsKey(pointOfInterest.poiType)) {
    //            awareness.Add(pointOfInterest.poiType, new List<IPointOfInterest>());
    //        }
    //        awareness[pointOfInterest.poiType].Add(pointOfInterest);

    //        if (pointOfInterest is TreeObject) {
    //            List<IPointOfInterest> treeAwareness = GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE.TREE_OBJECT);
    //            if (treeAwareness.Count >= Character.TREE_AWARENESS_LIMIT) {
    //                RemoveAwareness(treeAwareness[0]);
    //            }
    //        }
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveAwareness(IPointOfInterest pointOfInterest) {
    //    if (awareness.ContainsKey(pointOfInterest.poiType)) {
    //        List<IPointOfInterest> awarenesses = awareness[pointOfInterest.poiType];
    //        for (int i = 0; i < awarenesses.Count; i++) {
    //            IPointOfInterest iawareness = awarenesses[i];
    //            if (iawareness == pointOfInterest) {
    //                awarenesses.RemoveAt(i);
    //                break;
    //            }
    //        }
    //    }
    //}
    //public void RemoveAwareness(POINT_OF_INTEREST_TYPE poiType) {
    //    if (awareness.ContainsKey(poiType)) {
    //        awareness.Remove(poiType);
    //    }
    //}
    //public bool HasAwareness(IPointOfInterest poi) {
    //    if (awareness.ContainsKey(poi.poiType)) {
    //        List<IPointOfInterest> awarenesses = awareness[poi.poiType];
    //        for (int i = 0; i < awarenesses.Count; i++) {
    //            IPointOfInterest currPOI = awarenesses[i];
    //            if (currPOI == poi) {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //    return false;
    //}
    //public List<IPointOfInterest> GetTileObjectAwarenessOfType(TILE_OBJECT_TYPE tileObjectType) {
    //    List<IPointOfInterest> objects = new List<IPointOfInterest>();
    //    if (awareness.ContainsKey(POINT_OF_INTEREST_TYPE.TILE_OBJECT)) {
    //        List<IPointOfInterest> awarenesses = awareness[POINT_OF_INTEREST_TYPE.TILE_OBJECT];
    //        for (int i = 0; i < awarenesses.Count; i++) {
    //            TileObject iawareness = awarenesses[i] as TileObject;
    //            if (iawareness.tileObjectType == tileObjectType) {
    //                objects.Add(iawareness);
    //            }
    //        }
    //    }
    //    return objects;
    //}
    //private IAwareness CreateNewAwareness(IPointOfInterest poi) {
    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //        return new CharacterAwareness(poi as Character);
    //    } else if (poi.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
    //        return new ItemAwareness(poi as SpecialToken);
    //    } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
    //        return new TileObjectAwareness(poi);
    //    }
    //    return null;
    //}
    //public void ClearAllAwareness() {
    //    POINT_OF_INTEREST_TYPE[] types = Utilities.GetEnumValues<POINT_OF_INTEREST_TYPE>();
    //    for (int i = 0; i < types.Length; i++) {
    //        RemoveAwareness(types[i]);
    //    }
    //}
    #endregion

    #region Traits
    public void AddTrait(Trait trait) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        if (!traits.Contains(trait)) {
            traits.Add(trait);
        }
    }
    public void RemoveTrait(Trait trait) {
        if (owner.isSwitchingAlterEgo) {
            return; //ignore any changes while the owner is switching alter egos
        }
        traits.Remove(trait);
    }
    #endregion

    #region For Testing
    public string GetAlterEgoSummary() {
        string summary = owner.name + "'s alter ego " + name + " summary:";
        summary += "\nFaction: " + faction?.name ?? "Null";
        summary += "\nRace: " + race.ToString();
        summary += "\nRole: " + role?.name ?? "Null";
        summary += "\nCharacter Class: " + characterClass?.className ?? "Null";
        summary += "\nHome Structure: " + homeStructure?.ToString() ?? "Null";
        summary += "\nLevel: " + level.ToString();
        return summary;
    }
    #endregion
}
