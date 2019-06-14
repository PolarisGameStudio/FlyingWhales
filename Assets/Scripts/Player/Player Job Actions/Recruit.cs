﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recruit : PlayerJobAction {

    public object target { get; private set; }
    public JOB_ACTION_TARGET currentTargetType { get; private set; }

    public Recruit() {
        name = "Corrupt";
        SetDefaultCooldownTime(24);
        currentTargetType = JOB_ACTION_TARGET.NONE;
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        base.ActivateAction(assignedCharacter, targetPOI);
        Character targetCharacter = targetPOI as Character;
        currentTargetType = JOB_ACTION_TARGET.CHARACTER;
        target = targetPOI;
        Debug.Log(GameManager.Instance.TodayLogString() + assignedCharacter.name + " is now recruiting " + targetCharacter.name);
        CreateRecruitInteraction(targetCharacter);
    }
    public override void DeactivateAction() {
        base.DeactivateAction();
        currentTargetType = JOB_ACTION_TARGET.NONE;
        target = null;
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        return false; //always deactivate for now
        if (targetCharacter.isDead) {
            return false;
        }
        if (character.id == targetCharacter.id && targetCharacter.faction == PlayerManager.Instance.player.playerFaction) {
            return false;
        }
        if (targetCharacter.isLeader || targetCharacter.currentParty.icon.isTravelling || targetCharacter.isDefender || !targetCharacter.IsInOwnParty() || targetCharacter.doNotDisturb > 0) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    protected override void OnCharacterDied(Character characterThatDied) {
        base.OnCharacterDied(characterThatDied);
        if (!this.isActive) {
            return; //if this action is no longer active, do not check if the character that died was the target
        }
        if (currentTargetType == JOB_ACTION_TARGET.CHARACTER) {
            if (characterThatDied.id == (target as Character).id) {
                DeactivateAction();
            }
        }
    }

    private void CreateRecruitInteraction(Character target) {
        //Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MINION_RECRUIT_CHARACTER, target.specificLocation);
        //interaction.SetDefaultInvestigatorCharacter(assignedCharacter);
        //target.AddInteraction(interaction);
        //InteractionUI.Instance.OpenInteractionUI(interaction);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        return false;
    }
}
