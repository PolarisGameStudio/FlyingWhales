﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class TornadoTileObject : MovingTileObject {

    public int radius { get; private set; }
    public int durationInTicks { get; private set; }
    private TornadoMapObjectVisual _tornadoMapObjectVisual;
    public override string neutralizer => "Wind Master";
    protected override int affectedRange => 2;
    public TornadoTileObject() {
        Initialize(TILE_OBJECT_TYPE.TORNADO, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
    }
    public TornadoTileObject(SaveDataTileObject data) { }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _tornadoMapObjectVisual = mapVisual as TornadoMapObjectVisual;
        Assert.IsNotNull(_tornadoMapObjectVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _tornadoMapObjectVisual.Expire();
    }
    public void SetRadius(int radius) {
        this.radius = radius;
    }
    public void SetDuration(int duration) {
        this.durationInTicks = duration;
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return $"Tornado {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (mapVisual != null) {
            TornadoMapObjectVisual tornadoMapObjectVisual = mapVisual as TornadoMapObjectVisual;
            if (tornadoMapObjectVisual.isSpawned) {
                tile = tornadoMapObjectVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion
}
