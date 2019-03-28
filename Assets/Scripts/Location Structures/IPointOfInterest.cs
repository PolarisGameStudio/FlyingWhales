﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointOfInterest {

    string name { get; }
    POINT_OF_INTEREST_TYPE poiType { get; }
    POI_STATE state { get; }
    LocationGridTile gridTileLocation { get; }
    List<INTERACTION_TYPE> poiGoapActions { get; }
    List<Trait> traits { get; }
    Faction factionOwner { get; }
    POICollisionTrigger collisionTrigger { get; } //Each poi must only hav 1 at a time.

    void SetGridTileLocation(LocationGridTile tile);
    List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions);
    LocationGridTile GetNearestUnoccupiedTileFromThis();

    void SetPOIState(POI_STATE state);

    #region Traits
    bool AddTrait(string traitName);
    bool AddTrait(Trait trait, Character characterResponsible = null, System.Action onRemoveAction = null);
    bool RemoveTrait(Trait trait, bool triggerOnRemove = true);
    bool RemoveTrait(string traitName, bool triggerOnRemove = true);
    void RemoveTrait(List<Trait> traits);
    Trait GetTrait(string traitName);
    #endregion

    #region Collision
    void InitializeCollisionTrigger();
    void PlaceCollisionTriggerAt(LocationGridTile tile);
    void DisableCollisionTrigger();
    void SetCollisionTrigger(POICollisionTrigger trigger);
    void PlaceGhostCollisionTriggerAt(LocationGridTile tile);
    #endregion
}
