﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ore : TileObject, IPointOfInterest {
    public LocationStructure location { get; private set; }

    public int yield { get; private set; }

    private const int Supply_Per_Mine = 50;

    public Ore(LocationStructure location) {
        this.location = location;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.MINE_ACTION, INTERACTION_TYPE.TILE_OBJECT_DESTROY, };
        Initialize(TILE_OBJECT_TYPE.ORE);
        yield = Random.Range(15, 36);
    }

    public override string ToString() {
        return "Ore " + id.ToString();
    }

    public override void SetGridTileLocation(LocationGridTile tile) {
        //if (tile != null) {
        //    tile.SetTileAccess(LocationGridTile.Tile_Access.Impassable);
        //}
        base.SetGridTileLocation(tile);
    }

    public int GetSupplyPerMine() {
        if (yield < Supply_Per_Mine) {
            return yield;
        }
        return Supply_Per_Mine;
    }
    public void AdjustYield(int amount) {
        yield += amount;
        yield = Mathf.Max(0, yield);
        if (yield == 0) {
            LocationGridTile loc = gridTileLocation;
            location.RemovePOI(this);
            SetGridTileLocation(loc); //so that it can still be targetted by aware characters.
        }
    }
}
