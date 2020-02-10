﻿using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Zap : PlayerSpell {

    private int _zapDuration;
    public Zap() : base(SPELL_TYPE.ZAP) {
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
                    Trait newTrait = new Zapped();
                    newTrait.OverrideDuration(_zapDuration);
                    currTarget.traitContainer.AddTrait(currTarget, newTrait);
                    if (UIManager.Instance.characterInfoUI.isShowing) {
                        UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
                    }
                    GameManager.Instance.CreateElectricEffectAt(currTarget);

                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, "zapped", LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotification(log);
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
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Zapped")) {
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
        if(level == 1) {
            _zapDuration = 3;
        }else if (level == 2) {
            _zapDuration = 6;
        }else if (level == 3) {
            _zapDuration = 9;
        }
    }
    #endregion

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.traitContainer.HasTrait("Zapped")) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class ZapData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.ZAP;
    public override string name { get { return "Zap"; } }
    public override string description { get { return "Stops a character from his/her action and temporarily paralyzes him/her."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Zapped");
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
        }
        if(targetPOI is Character) {
            GameManager.Instance.CreateElectricEffectAt(targetPOI as Character);
        }

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "zapped", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || !targetCharacter.IsInOwnParty() || targetCharacter.traitContainer.HasTrait("Zapped")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    #endregion
}