﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Summon {

	public Wolf() : base(SUMMON_TYPE.Wolf, CharacterRole.BEAST, "Ravager", RACE.WOLF, UtilityScripts.Utilities.GetRandomGender()) { }
    public Wolf(SaveDataCharacter data) : base(data) { }

    #region Overrides
    //public override void OnPlaceSummon(LocationGridTile tile) {
    //    base.OnPlaceSummon(tile);
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.settlement);
    //    //state.SetIsUnending(true);
    //    //Messenger.AddListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
    //    GoToWorkArea();
    //}
    //protected override void IdlePlans() {
    //    base.IdlePlans();
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    #endregion
}
