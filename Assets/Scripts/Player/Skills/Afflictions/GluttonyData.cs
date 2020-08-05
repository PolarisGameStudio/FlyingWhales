﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class GluttonyData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.GLUTTONY;
    public override string name => "Gluttony";
    public override string description => "This Affliction will turn a Villager into a Gluttons. Gluttons get hungry faster.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    public GluttonyData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Glutton");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Glutton")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Glutton")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}