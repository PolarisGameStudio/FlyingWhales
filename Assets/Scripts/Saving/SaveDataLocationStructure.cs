﻿using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public abstract class SaveDataLocationStructure : SaveData<LocationStructure>{
    public int id;
    public string name;
    public STRUCTURE_TYPE structureType;
    public STRUCTURE_TAG[] structureTags;
    public Point[] tileCoordinates;
    public int currentHP;
    public int[] residentIDs;
    public int occupiedHexTileID;

    public override void Save(LocationStructure structure) {
        id = structure.id;
        name = structure.name;
        structureType = structure.structureType;
        
        //structure tags
        structureTags = new STRUCTURE_TAG[structure.structureTags.Count];
        for (int i = 0; i < structure.structureTags.Count; i++) {
            STRUCTURE_TAG structureTag = structure.structureTags[i];
            structureTags[i] = structureTag;
        }

        //tiles
        tileCoordinates = new Point[structure.tiles.Count];
        for (int i = 0; i < structure.tiles.Count; i++) {
            LocationGridTile tile = structure.tiles[i];
            Point point = new Point(tile.localPlace.x, tile.localPlace.y);
            tileCoordinates[i] = point;
        }

        //hp
        currentHP = structure.currentHP;
        
        //residents
        residentIDs = new int[structure.residents.Count];
        for (int i = 0; i < structure.residents.Count; i++) {
            Character resident = structure.residents[i];
            residentIDs[i] = resident.id;
        }

        //occupied hex tile
        if (structure.occupiedHexTile != null) {
            occupiedHexTileID = structure.occupiedHexTile.hexTileOwner.id;    
        } else {
            occupiedHexTileID = -1;
            Debug.Log($"{structure.name} has no occupied hextile!");
        }
    }
    public LocationStructure InitialLoad(Region region) {
        return LandmarkManager.Instance.LoadNewStructureAt(region, structureType, this);
    }
}

public class SaveDataNaturalStructure : SaveDataLocationStructure {
    //No Unique data for now
}

public class SaveDataManMadeStructure : SaveDataLocationStructure {

    public string structureTemplateName;
    public SaveDataStructureWallObject[] structureWallObjects;
    public RESOURCE wallsMadeOf;
    public Vector3Save structureObjectWorldPosition;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        ManMadeStructure manMadeStructure = locationStructure as ManMadeStructure;
        Assert.IsNotNull(manMadeStructure);
        
        //structure object
        string templateName = manMadeStructure.structureObj.name;
        templateName = templateName.Replace("(Clone)", "");
        structureTemplateName = templateName;
        structureObjectWorldPosition = manMadeStructure.structureObj.transform.position;
        
        //walls
        if (manMadeStructure.structureWalls != null) {
            structureWallObjects = new SaveDataStructureWallObject[manMadeStructure.structureWalls.Count];
            for (int i = 0; i < manMadeStructure.structureWalls.Count; i++) {
                StructureWallObject structureWallObject = manMadeStructure.structureWalls[i];
                SaveDataStructureWallObject saveDataStructureWallObject = new SaveDataStructureWallObject();
                saveDataStructureWallObject.Save(structureWallObject);
                structureWallObjects[i] = saveDataStructureWallObject;
            }
            wallsMadeOf = manMadeStructure.wallsAreMadeOf;
        }
    }
}

public class SaveDataDemonicStructure : SaveDataLocationStructure {
    
    public string structureTemplateName;
    public Vector3Save structureObjectWorldPosition;
    
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        DemonicStructure demonicStructure = locationStructure as DemonicStructure;
        Assert.IsNotNull(demonicStructure);
        
        //structure object
        string templateName = demonicStructure.structureObj.name;
        templateName = templateName.Replace("(Clone)", "");
        structureTemplateName = templateName;
        structureObjectWorldPosition = demonicStructure.structureObj.transform.position;
    }
}