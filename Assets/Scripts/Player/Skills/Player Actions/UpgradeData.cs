﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps.Location_Structures;

public class UpgradeData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UPGRADE;
    public override string name => "Upgrade";
    public override string description => $"Use the Biolab to upgrade your Plague affliction.";
    public UpgradeData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is Biolab biolab) {
            UIManager.Instance.ShowBiolabUI();
        }
        base.ActivateAbility(structure);
    }
    #endregion
}