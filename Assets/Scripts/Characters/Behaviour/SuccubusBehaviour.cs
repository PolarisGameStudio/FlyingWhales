﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;

public class SuccubusBehaviour : CharacterBehaviourComponent {
	public SuccubusBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a succubus";
        if (character.reactionComponent.disguisedCharacter != null) {
            if (character.currentStructure == character.homeStructure || character.IsInTerritory()) {
                if (character.previousCurrentActionNode != null && (character.previousCurrentActionNode.associatedJobType == JOB_TYPE.IDLE_RETURN_HOME || character.previousCurrentActionNode.associatedJobType == JOB_TYPE.RETURN_TERRITORY)) {
                    character.reactionComponent.SetDisguisedCharacter(null);
                    return true;
                }
            }
            if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.MAKE_LOVE && character.previousCurrentActionNode.actionStatus == ACTION_STATUS.SUCCESS) {
                if (character.currentStructure != character.homeStructure && !character.IsInTerritory()) {
                    if (character.jobComponent.PlanIdleReturnHome(out producedJob)) {
                        return true;
                    }
                }
            } else {
                Character targetCharacter = character.currentRegion.GetRandomCharacterThatMeetCriteria(c => c.gender == GENDER.MALE && !c.isDead && c.homeSettlement != null
                    && c.homeSettlement.GetFirstTileObjectOfTypeThatMeetCriteria<Bed>(b => b.mapObjectState == MAP_OBJECT_STATE.BUILT && b.IsAvailable() && b.GetActiveUserCount() == 0) != null);
                if (targetCharacter != null) {
                    log += $"\n-Target for make love is: " + targetCharacter.name;
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(targetCharacter.gridTileLocation)) {
                        if (character.jobComponent.TriggerMakeLoveJob(targetCharacter, out producedJob)) {
                            return true;
                        }
                    } else {
                        log += $"\n-No path to target, drop disguise";
                        character.reactionComponent.SetDisguisedCharacter(null);
                        return true;
                    }
                } else {
                    log += $"\n-No Character to be targeted for make love, drop disguise";
                    character.reactionComponent.SetDisguisedCharacter(null);
                    return true;
                }
            }
        } else {
            log += $"\n-Character is not in disguise, 1% chance to disguise";
            int roll = UnityEngine.Random.Range(0, 100);
            log += "\nRoll: " + roll;
            if(roll < 1) {
                Character targetCharacter = character.currentRegion.GetRandomAliveVillagerCharacterWithGender(GENDER.FEMALE);
                if(targetCharacter != null) {
                    log += $"\n-Target for disguise is: " + targetCharacter.name;
                    if(character.currentRegion.GetRandomCharacterThatMeetCriteria(c => c.gender == GENDER.MALE && !c.isDead && c.homeSettlement != null
                    && c.homeSettlement.GetFirstTileObjectOfTypeThatMeetCriteria<Bed>(b => b.mapObjectState == MAP_OBJECT_STATE.BUILT && b.IsAvailable() && b.GetActiveUserCount() == 0) != null) != null) {
                        if (character.jobComponent.TriggerDisguiseJob(targetCharacter, out producedJob)) {
                            return true;
                        }
                    }
                }
            }
        }
        log += $"\n-Character will roam";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
	}
}
