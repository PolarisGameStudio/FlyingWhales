﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class Tombstone : TileObject {
    public override Vector2 selectableSize => new Vector2(1f, 0.8f);
    public override Vector3 worldPosition {
        get {
            Vector3 pos = mapVisual.transform.position;
            pos.y += 0.15f;
            return pos;
        }
    }
    public override Character[] users {
        get { return new[] { character }; }
    }
    public Character character { get; private set; }
    public Tombstone() {
        AddAdvertisedAction(INTERACTION_TYPE.REMEMBER_FALLEN);
        AddAdvertisedAction(INTERACTION_TYPE.SPIT);
        // AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
        AddAdvertisedAction(INTERACTION_TYPE.RAISE_CORPSE);
    }
    public Tombstone(SaveDataTileObject data) {
        AddAdvertisedAction(INTERACTION_TYPE.REMEMBER_FALLEN);
        AddAdvertisedAction(INTERACTION_TYPE.SPIT);
        // AddAdvertisedAction(INTERACTION_TYPE.BUTCHER);
        AddAdvertisedAction(INTERACTION_TYPE.RAISE_CORPSE);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        character.marker.PlaceMarkerAt(gridTileLocation);
        character.DisableMarker();
        // character.marker.TryCancelExpiry();
        character.SetGrave(this);
        if (character.race == RACE.HUMANS || character.race == RACE.ELVES) {
            AddPlayerAction(SPELL_TYPE.RAISE_DEAD);
        }
        Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        RemovePlayerAction(SPELL_TYPE.RAISE_DEAD);
        character.EnableMarker();
        // character.marker.ScheduleExpiry();
        LocationGridTile tile = previousTile;
        if (tile.isOccupied) {
            tile = previousTile.GetRandomUnoccupiedNeighbor();
        }
        character.marker.PlaceMarkerAt(tile, false);
        character.SetGrave(null);
        character.jobComponent.TriggerBuryMe();
        Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
    }
    public override string ToString() {
        return $"Tombstone of {character.name}";
    }

    public void SetCharacter(Character character) {
        this.character = character;
        Initialize(TILE_OBJECT_TYPE.TOMBSTONE, false);
    }
    public void SetCharacter(Character character, SaveDataTileObject data) {
        this.character = character;
        //Initialize(data, false);
    }
}

//public class SaveDataTombstone : SaveDataTileObject {
//    public int characterID;

//    public override void Save(TileObject tileObject) {
//        base.Save(tileObject);
//        Tombstone obj = tileObject as Tombstone;
//        characterID = obj.character.id;
//    }

//    public override TileObject Load() {
//        Tombstone obj = base.Load() as Tombstone;
//        obj.SetCharacter(CharacterManager.Instance.GetCharacterByID(characterID), this);
//        return obj;
//    }
//}