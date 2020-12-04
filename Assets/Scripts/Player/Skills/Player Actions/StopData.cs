﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class StopData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.STOP;
    public override string name { get { return "Stop"; } }
    public override string description { get { return "Stop"; } }

    public StopData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            character.jobComponent.TriggerStopJobs();
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}