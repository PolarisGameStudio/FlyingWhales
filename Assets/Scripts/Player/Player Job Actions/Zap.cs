﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zap : PlayerJobAction {
    public Zap() {
        name = "Zap";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
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
                if (CanPerformActionTowards(assignedCharacter, currTarget)) {
                    Trait newTrait = new Zapped();
                    currTarget.AddTrait(newTrait);
                    if (UIManager.Instance.characterInfoUI.isShowing) {
                        UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
                    }
                    GameManager.Instance.CreateElectricEffectAt(currTarget);
                }
            }
            base.ActivateAction(assignedCharacter, targets[0]);
        }
    }
    protected override bool CanPerformActionTowards(Character character, IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    bool canTarget = CanPerformActionTowards(character, currUser);
                    if (canTarget) { return true; }
                }
            }
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Zap") != null) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    if (currUser != null) {
                        bool canTarget = CanTarget(currUser);
                        if (canTarget) { return true; }
                    }
                }
            }
        }
        return false;
    }
    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Zap") != null) {
            return false;
        }
        //if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
        //    return false;
        //}
        return true;
    }
}
