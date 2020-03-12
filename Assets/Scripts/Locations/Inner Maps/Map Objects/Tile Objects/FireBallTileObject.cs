﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class FireBallTileObject : MovingTileObject {

    private FireBallMapObjectVisual _fireBallMapVisual;
    
    public FireBallTileObject() {
        Initialize(TILE_OBJECT_TYPE.FIRE_BALL, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _fireBallMapVisual = mapVisual as FireBallMapObjectVisual;
        Assert.IsNotNull(_fireBallMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _fireBallMapVisual.Expire();
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Fire Ball";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        LocationGridTile tileLocation = gridTileLocation;
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { //&& source != null
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter);
        }
        if (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Water) {
            //2 Vapors
            for (int i = 0; i < 2; i++) {
                VaporTileObject vaporTileObject = new VaporTileObject();
                vaporTileObject.SetStacks(2);
                vaporTileObject.SetGridTileLocation(tileLocation);
                vaporTileObject.OnPlacePOI();
            }
        } else if (currentHP == 0 || (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Ice)) {
            //object has been destroyed
            _fireBallMapVisual.Expire();
        }
        //if (amount < 0) {
        //    Messenger.Broadcast(Signals.OBJECT_DAMAGED, this as IPointOfInterest);
        //} else if (currentHP == maxHP) {
        //    Messenger.Broadcast(Signals.OBJECT_REPAIRED, this as IPointOfInterest);
        //}
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_fireBallMapVisual != null) {
            if (_fireBallMapVisual.isSpawned) {
                tile = _fireBallMapVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion
}
