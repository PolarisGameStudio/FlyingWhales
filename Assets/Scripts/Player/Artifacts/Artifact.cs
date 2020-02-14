﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Artifact : TileObject {
    public ArtifactData data { get; private set; }
    public bool hasBeenActivated { get; private set; }
    
    #region getters/setters
    public string worldObjectName => name;
    public WORLD_OBJECT_TYPE worldObjectType => WORLD_OBJECT_TYPE.ARTIFACT;
    public ARTIFACT_TYPE type => data.type;
    #endregion

    public Artifact(ARTIFACT_TYPE type) {
        data = PlayerManager.Instance.GetArtifactData(type);
        // TILE_OBJECT_TYPE parsed = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), type.ToString(), true);
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(TILE_OBJECT_TYPE.ARTIFACT);
    }
    //public Artifact(SaveDataArtifactSlot data) {
    //    this.type = data.type;
    //    level = 1;
    //    TILE_OBJECT_TYPE parsed = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), type.ToString(), true);
    //    poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TILE_OBJECT_DESTROY };
    //    Initialize(data, parsed);
    //}
    public Artifact(SaveDataArtifact data) {
        this.data = PlayerManager.Instance.GetArtifactData(data.artifactType);
        advertisedActions = new List<INTERACTION_TYPE>();
        Initialize(data);
    }

    #region Overrides
    public override string ToString() {
        return name;
    }
    protected override string GenerateName() { return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString()); }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (gridTileLocation.buildSpotOwner.hexTileOwner == PlayerManager.Instance.player.portalTile) {
            PlayerManager.Instance.player.AddArtifact(this);
        }
    }
    #endregion

    public virtual void Activate() {
        hasBeenActivated = true;
        for (int i = 0; i < data.unlocks.Length; i++) {
            ArtifactUnlockable unlockable = data.unlocks[i];
            Unlock(unlockable);    
        }
    }
    private void Unlock(ArtifactUnlockable unlockable) {
        switch (unlockable.unlockableType) {
            case ARTIFACT_UNLOCKABLE_TYPE.Action:
                PlayerManager.Instance.player.archetype.AddAction(unlockable.identifier);
                break;
            case ARTIFACT_UNLOCKABLE_TYPE.Structure:
                LANDMARK_TYPE landmarkType = (LANDMARK_TYPE)Enum.Parse(typeof(LANDMARK_TYPE), unlockable.identifier);
                PlayerManager.Instance.player.archetype.AddDemonicStructure(landmarkType);
                break;
        }
    }
    
}

public class ArtifactSlot {
    public int level;
    public Artifact artifact;
    public bool isLocked {
        get { return false; } //PlayerManager.Instance.player.GetIndexForArtifactSlot(this) >= PlayerManager.Instance.player.maxArtifactSlots
    }

    public ArtifactSlot() {
        level = 1;
        artifact = null;
    }

    public void SetArtifact(Artifact artifact) {
        this.artifact = artifact;
        // if(this.artifact != null) {
        //     this.artifact.SetLevel(level);
        // }
    }
    
    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_ARTIFACT);
        // if (this.artifact != null) {
        //     this.artifact.SetLevel(level);
        // }
        Messenger.Broadcast(Signals.PLAYER_GAINED_ARTIFACT_LEVEL, this);
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_ARTIFACT);
        // if (this.artifact != null) {
        //     this.artifact.SetLevel(level);
        // }
        Messenger.Broadcast(Signals.PLAYER_GAINED_ARTIFACT_LEVEL, this);
    }
}