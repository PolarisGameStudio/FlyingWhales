﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance = null;

    public int totalLifestonesInWorld;
    public bool isChoosingStartingTile = false;
    public Player player = null;
    public Character playerCharacter;

    [SerializeField] private Sprite[] _playerAreaFloorSprites;
    [SerializeField] private Sprite[] _playerAreaDefaultStructureSprites;
    [SerializeField] private Sprite _supplySprite, _manaSprite, _impSprite;

    #region getters/setters
    public Sprite[] playerAreaFloorSprites {
        get { return _playerAreaFloorSprites; }
    }
    public Sprite[] playerAreaDefaultStructureSprites {
        get { return _playerAreaDefaultStructureSprites; }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {

    }
    public void LoadStartingTile() {
        BaseLandmark portal = LandmarkManager.Instance.GetLandmarkOfType(LANDMARK_TYPE.DEMONIC_PORTAL);
        if (portal == null) {
            //choose a starting tile
            ChooseStartingTile();
        } else {
            OnLoadStartingTile(portal);
        }
    }
    public void ChooseStartingTile() {
        Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Pick a starting tile", false);
        isChoosingStartingTile = true;
        Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
        UIManager.Instance.SetTimeControlsState(false);
    }
    private void OnChooseStartingTile(HexTile tile) {
        if (tile.areaOfTile != null || tile.landmarkOnTile != null || !tile.isPassable) {
            Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "That is not a valid starting tile!", false);
            return;
        }
        player = new Player();
        player.CreatePlayerFaction();
        player.CreatePlayerArea(tile);
        player.SetMaxMinions(9);
        player.CreateInitialMinions();
        LandmarkManager.Instance.OwnArea(player.playerFaction, player.playerArea);
        Messenger.RemoveListener<HexTile>(Signals.TILE_LEFT_CLICKED, OnChooseStartingTile);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        GameManager.Instance.StartProgression();
        isChoosingStartingTile = false;
        UIManager.Instance.SetTimeControlsState(true);
        PlayerUI.Instance.UpdateUI();
        //LandmarkManager.Instance.CreateNewArea(tile, AREA_TYPE.DEMONIC_INTRUSION);
    }
    private void OnLoadStartingTile(BaseLandmark portal) {
        player = new Player();
        player.CreatePlayerFaction();
        Area existingPlayerArea = LandmarkManager.Instance.GetAreaByName("Player Area");
        if (existingPlayerArea == null) {
            player.CreatePlayerArea(portal);
        } else {
            player.LoadPlayerArea(existingPlayerArea);
        }
        player.SetMaxMinions(9);
        player.CreateInitialMinions();
        LandmarkManager.Instance.OwnArea(player.playerFaction, player.playerArea);
        portal.SetIsBeingInspected(true);
        portal.SetHasBeenInspected(true);
        GameManager.Instance.StartProgression();
        UIManager.Instance.SetTimeControlsState(true);
        PlayerUI.Instance.UpdateUI();
    }

    public void PurchaseTile(HexTile tile) {
        if(player.lifestones > 0) {
            player.AdjustLifestone(-1);
            AddTileToPlayerArea(tile);
        }
    }
    public void AddTileToPlayerArea(HexTile tile) {
        player.playerArea.AddTile(tile);
        tile.SetCorruption(true);
        //tile.ActivateMagicTransferToPlayer();
    }
    public void CreatePlayerLandmarkOnTile(HexTile location, LANDMARK_TYPE landmarkType) {
        BaseLandmark landmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(location, landmarkType);
        OnPlayerLandmarkCreated(landmark);
        location.ScheduleCorruption();
    }

    private void OnPlayerLandmarkCreated(BaseLandmark newLandmark) {
        newLandmark.SetIsBeingInspected(true);
        newLandmark.SetHasBeenInspected(true);
        switch (newLandmark.specificLandmarkType) {
            case LANDMARK_TYPE.SNATCHER_DEMONS_LAIR:
                player.AdjustSnatchCredits(1);
                break;
            case LANDMARK_TYPE.DWELLINGS:
                //add 2 minion slots
                player.AdjustMaxMinions(2);
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                //adds 1 Imp capacity
                player.AdjustMaxImps(1);
                break;
            case LANDMARK_TYPE.RAMPART:
                //bonus 25% HP to all Defenders
                for (int i = 0; i < player.playerArea.landmarks.Count; i++) {
                    BaseLandmark currLandmark = player.playerArea.landmarks[i];
                    currLandmark.AddDefenderBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //if (currLandmark.defenders != null) {
                    //    currLandmark.defenders.AddBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //}
                }
                break;
            default:
                break;
        }
        //player.playerArea.DetermineExposedTiles();
        Messenger.Broadcast(Signals.PLAYER_LANDMARK_CREATED, newLandmark);
    }
    public void OnPlayerLandmarkRuined(BaseLandmark landmark) {
        switch (landmark.specificLandmarkType) {
            case LANDMARK_TYPE.DWELLINGS:
                //add 2 minion slots
                player.AdjustMaxMinions(-2);
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                //adds 1 Imp capacity
                player.AdjustMaxImps(-1);
                break;
            case LANDMARK_TYPE.DEMONIC_PORTAL:
                //player loses if the Portal is destroyed
                throw new System.Exception("Demonic Portal Was Destroyed! Game Over!");
            case LANDMARK_TYPE.RAMPART:
                //remove bonus 25% HP to all Defenders
                for (int i = 0; i < player.playerArea.landmarks.Count; i++) {
                    BaseLandmark currLandmark = player.playerArea.landmarks[i];
                    currLandmark.RemoveDefenderBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //if (currLandmark.defenders != null) {
                    //    currLandmark.defenders.RemoveBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //}
                }
                break;
            default:
                break;
        }
    }

    public void AdjustTotalLifestones(int amount) {
        totalLifestonesInWorld += amount;
        Debug.Log("Adjusted lifestones in world by " + amount + ". New total is " + totalLifestonesInWorld);
    }

    public Sprite GetSpriteByCurrency(CURRENCY currency) {
        if(currency == CURRENCY.IMP) {
            return _impSprite;
        }else if (currency == CURRENCY.MANA) {
            return _manaSprite;
        }else if (currency == CURRENCY.SUPPLY) {
            return _supplySprite;
        }
        return null;
    }

    #region Snatch
    public bool CanSnatch() {
        if (player == null) {
            return false;
        }
        return player.snatchCredits > 0;
    }
    #endregion

    #region Utilities
    public bool CanCreateLandmarkOnTile(LANDMARK_TYPE type, HexTile tile) {
        switch (type) {
            case LANDMARK_TYPE.MANA_EXTRACTOR:
                return tile.data.manaOnTile > 0;
            default:
                return true;
        }
    }
    #endregion

    #region Minion
    public Minion CreateNewMinion(DEMON_TYPE type, int level = 1) {
        Minion minion = new Minion(CharacterManager.Instance.CreateNewCharacter("Farmer", RACE.HUMANS, GENDER.MALE,
            player.playerFaction, player.demonicPortal, false), player.GetAbility("Inspect"), type);
        minion.SetLevel(level);
        return minion;
    }
    #endregion
}
