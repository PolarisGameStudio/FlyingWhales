﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UnityEngine.Assertions;

public class AnkhOfAnubis : Artifact {

    public bool isActivated { get; private set; }
    public override System.Type serializedData => typeof(SaveDataAnkhOfAnubis);
    
    public AnkhOfAnubis() : base(ARTIFACT_TYPE.Ankh_Of_Anubis) {
        maxHP = 700;
        currentHP = maxHP;
        traitContainer.AddTrait(this, "Treasure");
        traitContainer.AddTrait(this, "Indestructible");
    }
    public AnkhOfAnubis(SaveDataTileObject data) : base(data) {
        SaveDataAnkhOfAnubis saveDataAnkhOfAnubis = data as SaveDataAnkhOfAnubis;
        Assert.IsNotNull(saveDataAnkhOfAnubis);
        isActivated = saveDataAnkhOfAnubis.isActivated;
    }

    #region Overrides
    public override void ActivateTileObject() {
        if(gridTileLocation != null) {
            base.ActivateTileObject();
            isActivated = true;
            traitContainer.RemoveTrait(this, "Treasure");
            GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Ankh_Of_Anubis_Activate);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
        }
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
    }
    public override void OnTileObjectDroppedBy(Character inventoryOwner, LocationGridTile tile) {
        if (inventoryOwner.isDead && inventoryOwner.isNormalCharacter) {
            ActivateTileObject();
        }
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (isActivated) {
            GameManager.Instance.CreateParticleEffectAt(this, PARTICLE_EFFECT.Ankh_Of_Anubis_Activate);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
        }
    }
    #endregion

    private void OnCharacterDeath(Character characterThatDied) {
        if (isActivated && gridTileLocation != null) {
            if(characterThatDied.isNormalCharacter && currentRegion == characterThatDied.currentRegion && characterThatDied.marker != null && characterThatDied.visuals.HasBlood()) {
                Summon vengefulGhost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Vengeful_Ghost, FactionManager.Instance.undeadFaction, null, currentRegion);
                vengefulGhost.SetName(characterThatDied.name);
                CharacterManager.Instance.PlaceSummon(vengefulGhost, gridTileLocation); //characterThatDied.gridTileLocation

                Log log = new Log(GameManager.Instance.Today(), "Artifact", "Ankh Of Anubis", "spawn_vengeful_ghost");
                log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, characterThatDied.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToInvolvedObjects();
                if(gridTileLocation != null) {
                    PlayerManager.Instance.player.ShowNotificationFrom(gridTileLocation.structure.location, log);
                }
            }
        }
    }
}

#region Save Data
public class SaveDataAnkhOfAnubis : SaveDataArtifact {
    public bool isActivated;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        AnkhOfAnubis artifact = tileObject as AnkhOfAnubis;
        Assert.IsNotNull(artifact);
        isActivated = artifact.isActivated;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        AnkhOfAnubis ankhOfAnubis = tileObject as AnkhOfAnubis;
        Assert.IsNotNull(ankhOfAnubis);
        return tileObject;
    }
}
#endregion
