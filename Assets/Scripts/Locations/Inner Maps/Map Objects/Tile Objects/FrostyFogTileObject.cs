﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class FrostyFogTileObject : MovingTileObject {

    private FrostyFogMapObjectVisual _frostyFogMapVisual;
    
    public FrostyFogTileObject() {
        Initialize(TILE_OBJECT_TYPE.FROSTY_FOG, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.RemoveTrait(this, "Flammable");
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _frostyFogMapVisual = mapVisual as FrostyFogMapObjectVisual;
        Assert.IsNotNull(_frostyFogMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _frostyFogMapVisual.Expire();
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Frosty Fog";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
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
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        if (elementalDamageType == ELEMENTAL_TYPE.Fire) {
            //Wet
            List<LocationGridTile> tiles = tileLocation.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].AddTraitToAllPOIsOnTile("Wet");
            }
            _frostyFogMapVisual.Expire();
        } else if (elementalDamageType == ELEMENTAL_TYPE.Electric) {
            //2 Ball Lightnings
            for (int i = 0; i < 2; i++) {
                BallLightningTileObject ballLightning = new BallLightningTileObject();
                ballLightning.SetGridTileLocation(tileLocation);
                ballLightning.OnPlacePOI();
            }
            _frostyFogMapVisual.Expire();
        } else if (currentHP == 0) {
            //object has been destroyed
            _frostyFogMapVisual.Expire();
        }
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_frostyFogMapVisual != null) {
            if (_frostyFogMapVisual.isSpawned) {
                tile = _frostyFogMapVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion
}