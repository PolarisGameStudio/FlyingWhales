﻿using System.Collections.Generic;
using Inner_Maps;
using Interrupts;
using Traits;
using UnityEngine;

public class Sludge : Summon {
    public override string raceClassName => characterClass.className;
    public Sludge() : base(SUMMON_TYPE.Sludge, "Sludge", RACE.SLUDGE,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Sludge(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null,
        Log _deathLog = null, LogFiller[] deathLogFillers = null, Interrupt interrupt = null) {
        List<LocationGridTile> affectedTiles =
            gridTileLocation.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables(ApplyDamageTo);
        }
        base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, deathLogFillers, interrupt);
    }
    #endregion

    private void ApplyDamageTo(ITraitable traitable) {
        if (traitable is Character) { return; } //ignore characters
        traitable.AdjustHP(-50, combatComponent.elementalDamage.type, true, this);
    }
}