﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using Inner_Maps.Location_Structures;

public class Rock : TileObject{
    public int yield { get; private set; }
    public override Type serializedData => typeof(SaveDataRock);
    public Rock() {
        Initialize(TILE_OBJECT_TYPE.ROCK, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.MINE_STONE);

        SetYield(50);
        BaseSettlement.onSettlementBuilt += UpdateSettlementResourcesParent;
    }
    public Rock(SaveDataTileObject data) { }

    public override StructureConnector structureConnector { get; protected set; }

    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0) {
            LocationGridTile loc = gridTileLocation;
            structureLocation.RemovePOI(this);
            SetGridTileLocation(loc); //so that it can still be targetted by aware characters.
        }
    }

    public void SetYield(int amount) {
        yield = amount;
    }

    public override void UpdateSettlementResourcesParent() {
        BaseSettlement.onSettlementBuilt -= UpdateSettlementResourcesParent;
        if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.AllNeighbours.ForEach((eachNeighboringHexTile) => {
                if (eachNeighboringHexTile.settlementOnTile != null) {
                    if (!eachNeighboringHexTile.settlementOnTile.SettlementResources.rocks.Contains(this)) {
                        eachNeighboringHexTile.settlementOnTile.SettlementResources.rocks.Add(this);
                        parentSettlement = eachNeighboringHexTile.settlementOnTile;
                    }
                }
            });
        }
    }

    public override void RemoveFromSettlementResourcesParent() {
        if (parentSettlement != null) {
            parentSettlement.SettlementResources.rocks.Remove(this);
        }
    }
}
#region Save Data
public class SaveDataRock : SaveDataTileObject {
    public int yield;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Rock obj = tileObject as Rock;
        yield = obj.yield;
    }

    public override TileObject Load() {
        Rock obj = base.Load() as Rock;
        obj.SetYield(yield);
        return obj;
    }
}
#endregion