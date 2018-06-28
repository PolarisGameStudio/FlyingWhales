﻿using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Player : ILeader{

    private int _corruption;
    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int snatchCredits { get; private set; }

    public int id {
        get {
            return -645;
        }
    }

    public string name {
        get {
            return "Player";
        }
    }

    private List<ECS.Character> _snatchedCharacters;

    //#region getters/setters
    //public Area playerArea {
    //    get { return _playerArea; }
    //}
    //#endregion

    public Player() {
        _corruption = 0;
        playerArea = null;
        snatchCredits = 0;
        _snatchedCharacters = new List<ECS.Character>();
    }

    #region Corruption
    public void AdjustCorruption(int adjustment) {
        _corruption += adjustment;
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        BaseLandmark demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        SetPlayerArea(playerArea);
    }
    private void SetPlayerArea(Area area) {
        playerArea = area;
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction playerFaction = FactionManager.Instance.CreateNewFaction();
        playerFaction.SetLeader(this);
        SetPlayerFaction(playerFaction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    #endregion

    #region Snatch
    public void AdjustSnatchCredits(int adjustment) {
        snatchCredits += adjustment;
        snatchCredits = Mathf.Max(0, snatchCredits);
    }
    public void SnatchCharacter(ECS.Character character) {
        AdjustSnatchCredits(-1);
        //check for success
        if (IsSnatchSuccess(character)) {
            if (!_snatchedCharacters.Contains(character)) {
                _snatchedCharacters.Add(character);
                character.OnCharacterSnatched();
                Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Successfully snatched " + character.name, MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
                Debug.Log("Snatched " + character.name);
            }
        } else {
            Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Failed to snatch " + character.name, MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
            Debug.Log("Failed to snatch " + character.name);
        }
        
    }
    private bool IsSnatchSuccess(ECS.Character character) {
        if (character.role == null) {
            return true;
        }
        if (character.role.roleType == CHARACTER_ROLE.CIVILIAN) {
            return true; //100%
        } else if (character.role.roleType == CHARACTER_ROLE.HERO) {
            int chance = 50; //low happiness
            if (character.role.happiness >= 100f) {
                chance = 25; //high happiness
            }
            if (Random.Range(0, 100) < chance) {
                return true;
            }
        } else if (character.role.roleType == CHARACTER_ROLE.KING) {
            int chance = 25; //low happiness
            if (character.role.happiness >= 100f) {
                chance = 10; //high happiness
            }
            if (Random.Range(0, 100) < chance) {
                return true;
            }
        }
        return false;
    }
    public bool IsCharacterSnatched(ECS.Character character) {
        return _snatchedCharacters.Contains(character);
    }
    public BaseLandmark GetAvailableSnatcherLair() {
        for (int i = 0; i < playerArea.landmarks.Count; i++) {
            BaseLandmark currLandmark = playerArea.landmarks[i];
            if (currLandmark.specificLandmarkType == LANDMARK_TYPE.SNATCHER_DEMONS_LAIR) {
                if (!HasSnatchedCharacter(currLandmark)) {
                    return currLandmark;
                }
            }
        }
        return null;
    }
    public bool HasSnatchedCharacter(BaseLandmark landmark) {
        for (int i = 0; i < _snatchedCharacters.Count; i++) {
            ECS.Character currCharacter = _snatchedCharacters[i];
            if (landmark.charactersAtLocation.Contains(currCharacter)) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
