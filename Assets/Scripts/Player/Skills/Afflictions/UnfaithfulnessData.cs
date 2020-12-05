﻿using System.Collections;
using Logs;
using UnityEngine;

public class UnfaithfulnessData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UNFAITHFULNESS;
    public override string name => "Unfaithfulness";
    public override string description => $"This Affliction will make a Villager Unfaithful. Unfaithful Villagers may flirt and develop Affairs even if they already have a Husband or a Wife.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public UnfaithfulnessData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        AfflictPOIWith("Unfaithful", targetPOI, name);
        OnExecuteSpellActionAffliction();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Unfaithful", "Beast")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Unfaithful")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}