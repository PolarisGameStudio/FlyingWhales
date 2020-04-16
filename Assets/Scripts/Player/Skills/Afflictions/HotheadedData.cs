﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class HotheadedData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.HOTHEADED;
    public override string name { get { return "Hotheaded"; } }
    public override string description { get { return "Hotheaded"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.AFFLICTION; } }

    public HotheadedData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Hothead");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, name, LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Hothead")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}