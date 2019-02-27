﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Food : IPointOfInterest {

    public LocationStructure location { get; private set; }
    public FOOD foodType { get; private set; }
    public string foodName { get; private set; }

    private LocationGridTile tile;

    #region getters/setters
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.FOOD; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    #endregion

    public Food(LocationStructure location, FOOD foodType) {
        this.location = location;
        this.foodType = foodType;
        this.foodName = Utilities.NormalizeStringUpperCaseFirstLetters(this.foodType.ToString());
    }

    public override string ToString() {
        return foodName;
    }

    #region Interface
    public void SetGridTileLocation(LocationGridTile tile) {
        if (tile != null) {
            location.AdjustFoodCount(foodType, 1);
        } else {
            location.AdjustFoodCount(foodType, -1);
        }
        this.tile = tile;
    }
    public LocationGridTile GetNearestUnoccupiedTileFromThis(LocationStructure structure, Character otherCharacter) {
        if (gridTileLocation != null && location == structure) {
            List<LocationGridTile> choices = location.unoccupiedTiles.Where(x => x != gridTileLocation).OrderBy(x => Vector2.Distance(gridTileLocation.localLocation, x.localLocation)).ToList();
            if (choices.Count > 0) {
                LocationGridTile nearestTile = choices[0];
                if (otherCharacter.currentStructure == structure && otherCharacter.gridTileLocation != null) {
                    float ogDistance = Vector2.Distance(this.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
                    float newDistance = Vector2.Distance(this.gridTileLocation.localLocation, nearestTile.localLocation);
                    if (newDistance > ogDistance) {
                        return otherCharacter.gridTileLocation; //keep the other character's current tile
                    }
                }
                return nearestTile;
            }
        }
        return null;
    }
    #endregion
}
