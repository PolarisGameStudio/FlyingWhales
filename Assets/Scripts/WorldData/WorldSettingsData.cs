﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettingsData {

    public enum World_Type {
        Tutorial,
        Second_World,
        Custom
    };
    
    public int numOfRegions { get; private set; }
    public bool omnipotentMode { get; private set; }
    public bool noThreatMode { get; private set; }
    public bool chaosVictoryMode { get; private set; }
    public List<RACE> races { get; private set; }
    public List<BIOMES> biomes { get; private set; }
    public World_Type worldType { get; private set; }
    
    public WorldSettingsData() {
        races = new List<RACE>();
        biomes = new List<BIOMES>();
        // SetSecondWorldSettings();
        worldType = World_Type.Custom;
    }

    public void SetNumOfRegions(int amount) {
        numOfRegions = amount;
    }
    public void SetOmnipotentMode(bool state) {
        omnipotentMode = state;
    }
    public void SetNoThreatMode(bool state) {
        noThreatMode = state;
    }
    public void SetChaosVictoryMode(bool state) {
        chaosVictoryMode = state;
    }

    public void AddRace(RACE race) {
        if (!races.Contains(race)) {
            races.Add(race);
        }
    }
    public bool RemoveRace(RACE race) {
        return races.Remove(race);
    }
    public void ClearRaces() {
        races.Clear();
    }

    public void AddBiome(BIOMES biome) {
        if (!biomes.Contains(biome)) {
            biomes.Add(biome);
        }
    }
    public bool RemoveBiome(BIOMES biome) {
        return biomes.Remove(biome);
    }
    public void ClearBiomes() {
        biomes.Clear();
    }

    public bool AreSettingsValid() {
        if (races.Count == 1) {
            //if only 1 race was toggled.
            //check that that races needed biome is also available
            RACE race = races[0];
            if (race == RACE.HUMANS) {
                return biomes.Contains(BIOMES.DESERT) || biomes.Contains(BIOMES.GRASSLAND);
            } else if (race == RACE.ELVES) {
                return biomes.Contains(BIOMES.FOREST) || biomes.Contains(BIOMES.SNOW);
            }
        }
        return races.Count >= 1 && biomes.Count >= 1;
    }
    public void SetTutorialWorldSettings() {
        Debug.Log("Set world settings as Tutorial");
        worldType = World_Type.Tutorial;
        numOfRegions = 1;
        omnipotentMode = false;
        noThreatMode = false;
        AddRace(RACE.HUMANS);
        AddBiome(BIOMES.GRASSLAND);
    }
    public void SetSecondWorldSettings() {
        Debug.Log("Set world settings as Second World");
        worldType = World_Type.Second_World;
        numOfRegions = 1;
        omnipotentMode = false;
        noThreatMode = false;
        AddRace(RACE.HUMANS);
        AddRace(RACE.ELVES);
        AddBiome(BIOMES.DESERT);
    }
    public void SetDefaultCustomWorldSettings() {
        Debug.Log("Set world settings as Default Custom");
        worldType = World_Type.Custom;
        numOfRegions = 3;
        omnipotentMode = false;
        noThreatMode = false;
        AddRace(RACE.HUMANS);
        AddRace(RACE.ELVES);
        AddBiome(BIOMES.DESERT);
        AddBiome(BIOMES.GRASSLAND);
        AddBiome(BIOMES.SNOW);
        AddBiome(BIOMES.FOREST);
    }
}
