﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Unfaithfulness : PlayerSpell {

    public Unfaithfulness() : base(SPELL_TYPE.UNFAITHFULNESS) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
        //abilityTags.Add(ABILITY_TAG.CRIME);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        List<Character> targets = new List<Character>();
        if (targetPOI is Character) {
            targets.Add(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) { targets.AddRange(to.users); }
        } else {
            return;
        }
        if (targets.Count > 0) {
            for (int i = 0; i < targets.Count; i++) {
                Character currTarget = targets[i];
                if (CanPerformActionTowards(currTarget)) {
                    Trait newTrait = new Unfaithful();
                    newTrait.InitializeInstancedTrait();
                    currTarget.traitContainer.AddTrait(currTarget, newTrait);
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(newTrait, newTrait.name, LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }
            }
            base.ActivateAction(targets[0]);
        }
    }

    protected virtual bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    bool canTarget = CanPerformActionTowards(currUser);
                    if (canTarget) { return true; }
                }
            }
        }
        return false;
    }

    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(targetCharacter.race) || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Unfaithful")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character, ref hoverText);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    if (currUser != null) {
                        bool canTarget = CanTarget(currUser, ref hoverText);
                        if (canTarget) { return true; }
                    }
                }
            }
        }
        return false;
    }
    #endregion

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(targetCharacter.race) || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Unfaithful")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class UnfaithfulnessData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.UNFAITHFULNESS;
    public override string name => "Unfaithfulness";
    public override string description => $"This Affliction will make a Villager Unfaithful. Unfaithful Villagers may flirt and develop Affairs even if they already have a Husband or a Wife.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.AFFLICTION;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;

    public UnfaithfulnessData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Unfaithful");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Unfaithful", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        base.ActivateAbility(targetPOI);
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