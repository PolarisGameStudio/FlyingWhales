﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodPile : ResourcePile {

    public FoodPile() : base(RESOURCE.FOOD) {
        Initialize(TILE_OBJECT_TYPE.FOOD_PILE);
        //SetResourceInPile(50);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    public FoodPile(SaveDataTileObject data) : base(RESOURCE.FOOD) {
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return "Food Pile " + id.ToString();
    }
    public override bool CanBeReplaced() {
        return true;
    }
    #endregion
}

public class SaveDataFoodPile : SaveDataTileObject {
    public int foodInPile;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        FoodPile obj = tileObject as FoodPile;
        foodInPile = obj.resourceInPile;
    }

    public override TileObject Load() {
        FoodPile obj = base.Load() as FoodPile;
        obj.SetResourceInPile(foodInPile);
        return obj;
    }
}