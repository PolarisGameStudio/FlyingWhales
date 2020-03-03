﻿using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class MimicTileObject : TileObject {

    private bool _hasBeenAwakened;
    
    public MimicTileObject() {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.MIMIC_TILE_OBJECT);
        RemoveAdvertisedAction(INTERACTION_TYPE.REPAIR);
    }
    public MimicTileObject(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }
    public override void OnDoActionToObject(ActualGoapNode action) {
        if (action.action.actionCategory == ACTION_CATEGORY.DIRECT || action.action.actionCategory == ACTION_CATEGORY.CONSUME) {
            action.actor.StopCurrentActionNode();
            AwakenMimic();
        }
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        if (amount < 0) {
            AwakenMimic();
        }
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source);
    }
    private void AwakenMimic() {
        if (_hasBeenAwakened) { return; }
        _hasBeenAwakened = true;
        Assert.IsNotNull(gridTileLocation, $"{gridTileLocation.localPlace.ToString()} of mimic to awaken was null!");
        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Mimic, FactionManager.Instance.neutralFaction);
        (gridTileLocation.parentMap.location as Region).AddResident(summon);
        CharacterManager.Instance.PlaceSummon(summon, gridTileLocation);
        if (gridTileLocation.buildSpotOwner.isPartOfParentRegionMap) {
            summon.AddTerritory(gridTileLocation.buildSpotOwner.hexTileOwner);    
        } else {
            List<HexTile> tiles = (gridTileLocation.parentMap.location as Region).tiles.Where(x =>
                x.settlementOnTile == null || x.settlementOnTile.locationType == LOCATION_TYPE.DUNGEON).ToList();
            summon.AddTerritory(UtilityScripts.CollectionUtilities.GetRandomElement(tiles));
          Debug.LogWarning($"{summon.name} was awakened from a mimic, but its gridTileLocation " +
                           $"{gridTileLocation.localPlace.ToString()} is not linked to a hextile, so its territory was " +
                           $"set to a random hextile inside the region {summon.territorries[0]}.");  
        }
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            Trait trait = traitContainer.allTraits[i];
            summon.traitContainer.AddTrait(summon, trait.name);
        }
        gridTileLocation.structure.RemovePOI(this);
    }
}