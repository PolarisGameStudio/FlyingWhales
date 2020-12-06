﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class Artifact : TileObject {
    public ArtifactData data { get; }
    private GameObject _artifactEffectGO;

    #region getters/setters
    public ARTIFACT_TYPE type => data.type;
    public override System.Type serializedData => typeof(SaveDataArtifact);
    #endregion

    public Artifact(ARTIFACT_TYPE type) : base() {
        data = ScriptableObjectsManager.Instance.GetArtifactData(type);
        Initialize(TILE_OBJECT_TYPE.ARTIFACT, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
    }
    public Artifact(SaveDataArtifact data) : base(data) {
        //SaveDataArtifact saveDataArtifact = saveData as SaveDataArtifact;
        Assert.IsNotNull(data);
        this.data = ScriptableObjectsManager.Instance.GetArtifactData(data.artifactType);
    }

    #region Overrides
    public override string ToString() {
        return name;
    }
    protected override string GenerateName() { return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString()); }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        // AddPlayerAction(SPELL_TYPE.ACTIVATE);
        AddPlayerAction(PLAYER_SKILL_TYPE.SEIZE_OBJECT);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (_artifactEffectGO) {
            ObjectPoolManager.Instance.DestroyObject(_artifactEffectGO);
            _artifactEffectGO = null;
        }
        _artifactEffectGO = GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Artifact);
        if(characterOwner != null) {
            //Artifacts must clear out ownership if it is placed on a tile, so that it can be picked up again
            SetCharacterOwner(null);
        }
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        if (_artifactEffectGO) {
            ObjectPoolManager.Instance.DestroyObject(_artifactEffectGO);
            _artifactEffectGO = null;
        }
    }
    public override void DestroyMapVisualGameObject() {
        base.DestroyMapVisualGameObject();
        if (_artifactEffectGO) {
            ObjectPoolManager.Instance.DestroyObject(_artifactEffectGO);
            _artifactEffectGO = null;
        }
    }
    #endregion
}

#region Save Data
public class SaveDataArtifact : SaveDataTileObject {
    public ARTIFACT_TYPE artifactType;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Artifact artifact = tileObject as Artifact;
        Assert.IsNotNull(artifact);
        artifactType = artifact.data.type;
    }
    public override TileObject Load() {
        TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
        tileObject.Initialize(this);
        return tileObject;
    }
}
#endregion