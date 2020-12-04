﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class LearnSpellData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.LEARN_SPELL;
    public override string name { get { return "Learn Spell"; } }
    public override string description { get { return "Learn Spell"; } }

    public LearnSpellData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.STRUCTURE };
    }

    #region Overrides
    public override void ActivateAbility(LocationStructure structure) {
        if (structure is Inner_Maps.Location_Structures.Ostracizer theSpire) {
            theSpire.TryLearnASpellOrAffliction();
        }
    }
    public override bool CanPerformAbilityTowards(LocationStructure structure) {
        bool canPerform = base.CanPerformAbilityTowards(structure);
        if (canPerform) {
            if (structure is Inner_Maps.Location_Structures.Ostracizer theSpire) {
                return theSpire.CanLearnSpell();
            }
            return false;
        }
        return canPerform;
    }
    #endregion
}