﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

[System.Obsolete]
public class InterfereData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.INTERFERE;
    public override string name { get { return "Interfere"; } }
    public override string description { get { return "Interfere"; } }

    public InterfereData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) { }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            if (structure is Inner_Maps.Location_Structures.Meddler) {
                return true;
            }
            return false;
        }
        return canPerform;
    }
    #endregion
}