﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class SeizeObjectData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SEIZE_OBJECT;
    public override string name { get { return "Seize Object"; } }
    public override string description { get { return "Seize Object"; } }

    public SeizeObjectData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject targetTileObject) {
        bool canPerform = base.CanPerformAbilityTowards(targetTileObject);
        if (canPerform) {
            return !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && targetTileObject.mapVisual != null && (targetTileObject.isBeingCarriedBy != null || targetTileObject.gridTileLocation != null);
        }
        return canPerform;
    }
    #endregion
}