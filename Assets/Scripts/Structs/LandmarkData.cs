﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LandmarkData {
    [Header("General Data")]
    public string landmarkTypeString;
    public LANDMARK_TYPE landmarkType;
    public int minimumTileCount = 1; //how many tiles does this landmark need
    public HEXTILE_DIRECTION connectedTileDirection;
    public List<LANDMARK_TAG> uniqueTags;
    public Sprite landmarkObjectSprite;
    public Sprite landmarkTypeIcon;
    public BiomeLandmarkSpriteListDictionary biomeTileSprites;
    public List<LandmarkStructureSprite> neutralTileSprites; //These are the sprites that will be used if landmark is not owned by a race
    public List<LandmarkStructureSprite> humansLandmarkTileSprites;
    public List<LandmarkStructureSprite> elvenLandmarkTileSprites;
    public List<PASSABLE_TYPE> possibleSpawnPoints;
    public bool isUnique;
    
    [Header("Monster Spawner")]
    public MonsterPartyComponent startingMonsterSpawn;
    public bool isMonsterSpawner;
    public List<MonsterSet> monsterSets;
    public int monsterSpawnCooldown;


    #region getter/setters
    #endregion
}
