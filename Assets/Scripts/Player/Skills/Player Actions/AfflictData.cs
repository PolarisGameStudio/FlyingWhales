﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class AfflictData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.AFFLICT;
    public override string name { get { return "Afflict"; } }
    public override string description { get { return "Afflict"; } }

    public AfflictData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            UIManager.Instance.characterInfoUI.ShowAfflictUI();
        }
    }
    #endregion
}