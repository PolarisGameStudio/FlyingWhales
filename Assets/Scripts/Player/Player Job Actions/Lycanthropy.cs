﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Lycanthropy : PlayerSpell {

    private int _level;

    public Lycanthropy() : base(SPELL_TYPE.LYCANTHROPY) {
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER, SPELL_TARGET.TILE_OBJECT };
        //abilityTags.Add(ABILITY_TAG.MAGIC);
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
                    //Trait newTrait = new Lycanthrope();
                    //newTrait.SetLevel(_level);
                    //currTarget.traitContainer.AddTrait(currTarget, newTrait);
                    LycanthropeData lycanthropeData = new LycanthropeData(currTarget);
                    //AlterEgoData alterEgoData = currTarget.GetAlterEgoData("Lycanthrope");
                    //alterEgoData.SetLevel(_level);
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, "Lycanthrope", LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }
            }
            base.ActivateAction(targets[0]);
        }
    }

    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
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
        if (targetCharacter.isDead) {
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(targetCharacter.race) || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Lycanthrope")) {
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
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (level == 1) {
            _level = 1;
        } else if (level == 2) {
            _level = 3;
        } else if (level == 3) {
            _level = 6;
        }
    }
    #endregion

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (UtilityScripts.GameUtilities.IsRaceBeast(targetCharacter.race) || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Lycanthrope")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class LycanthropyData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.LYCANTHROPY;
    public override string name { get { return "Lycanthropy"; } }
    public override string description { get { return "Makes a character transform into a wild wolf whenever he/she sleeps."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.MONSTER; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.AFFLICTION;


    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        //targetPOI.traitContainer.AddTrait(targetPOI, "Lycanthrope");
        LycanthropeData lycanthropeData = new LycanthropeData(targetPOI as Character);
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "Lycanthrope", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.race == RACE.SKELETON || targetCharacter.traitContainer.HasTrait("Lycanthrope", "Beast")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}