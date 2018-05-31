﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexTileData {
    [Header("General Tile Details")]
    public int id;
    public int xCoordinate;
    public int yCoordinate;
    public int tileTag;
    public string tileName;
    public int regionID;

    [Space(10)]
    [Header("Biome Settings")]
    public float elevationNoise;
    public float moistureNoise;
    public float temperature;
    public BIOMES biomeType;
    public ELEVATION elevationType;
}