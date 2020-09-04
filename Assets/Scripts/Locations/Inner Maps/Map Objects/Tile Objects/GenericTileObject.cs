﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

public class GenericTileObject : TileObject {
    private bool hasBeenInitialized { get; set; }
    public override LocationGridTile gridTileLocation => _owner;
    private LocationGridTile _owner;
    
    public override System.Type serializedData => typeof(SaveDataGenericTileObject);
    
    public GenericTileObject(LocationGridTile locationGridTile) : base() {
        _owner = locationGridTile;
    }
    public GenericTileObject(SaveDataGenericTileObject data) : base(data) {
        
    }

    #region Override
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this as TileObject, removedBy, removedFrom, destroyTileSlots);
        if (hasCreatedSlots && destroyTileSlots) {
            DestroyTileSlots();
        }
    }
    public override void OnPlacePOI() {
        SetPOIState(POI_STATE.ACTIVE);
    }
    protected override string GenerateName() { return "the floor"; }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) { } //overridden this to reduce unnecessary processing 
    public override void OnDestroyPOI() {
        DisableGameObject();
        OnRemoveTileObject(null, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.gridTileLocation;
        DisableGameObject();
        OnRemoveTileObject(removedBy, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override bool IsValidCombatTargetFor(IPointOfInterest source) {
        return false;
    }
    public override void OnTileObjectGainedTrait(Trait trait) {
        if (trait is Status status) {
            if(status.isTangible) {
                //if status is wet, and this tile is not part of a settlement, then do not create a map visual, since
                //characters do not react to wet tiles outside their settlement.
                bool willCreateVisual = !(status is Wet && gridTileLocation.IsPartOfSettlement() == false);
                if (willCreateVisual) {
                    GetOrCreateMapVisual();
                    SubscribeListeners();    
                } else {
                    //if should not create visual, also do not vote on vision, this is for cases when a tile already has a gameobject
                    //and gained a trait that should not make the tile visible.
                    return;
                }
                
            }
        }
        base.OnTileObjectGainedTrait(trait);
    }
    public override void OnTileObjectLostTrait(Trait trait) {
        base.OnTileObjectLostTrait(trait);
        if (TryDestroyMapVisual()) {
            UnsubscribeListeners();
        }
    }
    public override string ToString() {
        return $"Generic Obj at tile {gridTileLocation}";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) {
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        
        if (currentHP <= 0) {
            //floor has been destroyed
            gridTileLocation.DetermineNextGroundTypeAfterDestruction();
        } 
        if (amount < 0) {
            structureLocation.OnTileDamaged(gridTileLocation, amount);
        } else if (amount > 0) {
            structureLocation.OnTileRepaired(gridTileLocation, amount);
        }

        if (currentHP <= 0) {
            //reset floor hp at end of processing
            currentHP = maxHP;
        }
    }
    public override bool CanBeDamaged() {
        //only damage tiles that are part of non open space structures i.e structures with walls.
        return structureLocation.structureType.IsOpenSpace() == false;
    }
    public override bool CanBeSelected() {
        return false;
    }
    #endregion

    public BaseMapObjectVisual GetOrCreateMapVisual() {
        if (ReferenceEquals(mapVisual, null)) {
            InitializeMapObject(this);
            PlaceMapObjectAt(gridTileLocation);
            OnPlaceTileObjectAtTile(gridTileLocation);
        }
        return mapVisual;
    }
    public bool TryDestroyMapVisual() {
        if (traitContainer.HasTangibleTrait() == false) {
            if (ReferenceEquals(mapVisual, null) == false) {
                DestroyMapVisualGameObject();
            }
            return true;
        }
        return false;
    }
    public void SetTileOwner(LocationGridTile owner) {
        _owner = owner;
    }

    public void ManualInitialize(LocationGridTile tile) {
        if (hasBeenInitialized) {
            return;
        }
        hasBeenInitialized = true;
        Initialize(TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT, false);
        SetGridTileLocation(tile);
        AddAdvertisedAction(INTERACTION_TYPE.PLACE_FREEZING_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.GO_TO_TILE);
        AddAdvertisedAction(INTERACTION_TYPE.FLEE_CRIME);
    }
    public void ManualInitializeLoad(LocationGridTile tile, SaveDataTileObject saveDataTileObject) {
        if (hasBeenInitialized) {
            return;
        }
        hasBeenInitialized = true;
        Initialize(saveDataTileObject);
        SetGridTileLocation(tile);
        AddAdvertisedAction(INTERACTION_TYPE.PLACE_FREEZING_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.GO_TO_TILE);
        AddAdvertisedAction(INTERACTION_TYPE.FLEE_CRIME);
    }
}

#region Save Data
public class SaveDataGenericTileObject : SaveDataTileObject {
    public override TileObject Load() {
        GenericTileObject genericTileObject = InnerMapManager.Instance.LoadTileObject<GenericTileObject>(this);
        return genericTileObject;
    }
}
#endregion