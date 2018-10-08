﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class CharacterClick : MonoBehaviour {
    public CharacterIcon icon;

    //private void OnMouseOver() {
    //    if (Input.GetMouseButton(0)) {
    //        MouseDown();
    //    }
    //}
    private void OnMouseDown() {
        MouseDown();
    }
    private void MouseDown() {
        if (UIManager.Instance.IsMouseOnUI()) {
            return;
        }
        if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
            CharacterAction action = icon.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
            AttackAction attackAction = action as AttackAction;
            if (attackAction.CanBeDoneByTesting(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party, icon.iparty.icharacterObject)) { //TODO: Change this checker to relationship status checking instead of just faction
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(attackAction, icon.iparty.icharacterObject);
                UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                return;
            }
        }else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
            CharacterAction joinBattleAction = icon.iparty.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
            if (joinBattleAction.CanBeDone(icon.iparty.icharacterObject) && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party, icon.iparty.icharacterObject)) { //TODO: Change this checker to relationship status checking instead of just faction
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party.actionData.AssignAction(joinBattleAction, icon.iparty.icharacterObject);
                UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
                return;
            }
        }
        Party iparty = icon.iparty;
        if (iparty.icharacters.Count > 1) {
            UIManager.Instance.ShowPartyInfo(iparty as Party);
        } else if (iparty.icharacters.Count == 1) {
            if (iparty.mainCharacter is ECS.Character) {
                UIManager.Instance.ShowCharacterInfo(iparty.mainCharacter as ECS.Character);
            }
        }

    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (icon.iparty is CharacterParty) {
            CharacterParty thisParty = icon.iparty as CharacterParty;
            if (thisParty.actionData.currentAction != null) {
                if (other.tag == "Character") { //|| thisParty.actionData.currentAction.actionType == ACTION_TYPE.CHAT
                    CharacterIcon enemy = other.GetComponent<CharacterClick>().icon;
                    if (thisParty.actionData.currentAction.actionType == ACTION_TYPE.ATTACK && thisParty.actionData.currentTargetObject is ICharacterObject) {
                        ICharacterObject icharacterObject = thisParty.actionData.currentTargetObject as ICharacterObject;
                        if (icharacterObject.iparty.id == enemy.iparty.id) {//attackAction.icharacterObj.iparty == enemy.iparty.icharacterType && 
                            thisParty.actionData.DoAction();
                        }
                    } else {
                        if (thisParty.faction == null) {
                            throw new System.Exception(thisParty.name + " has no faction!");
                        }
                        if (enemy.iparty == null) {
                            throw new System.Exception(enemy.name + " has no party!");
                        }
                        if (enemy.iparty.faction == null) {
                            throw new System.Exception(enemy.name + " has no faction!");
                        }
                        if(thisParty.faction.id != enemy.iparty.faction.id) {
                            //Check if hostile faction, if it is, check mode for combat chances
                            FactionRelationship factionRelationship = thisParty.faction.GetRelationshipWith(enemy.iparty.faction);
                            if (factionRelationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                                int combatChance = Utilities.rng.Next(0, 100);
                                int value = Utilities.combatChanceGrid[thisParty.currentMode][enemy.iparty.currentMode];
                                if (combatChance < value) {
                                    //Combat
                                    thisParty.StartCombatWith(enemy.iparty);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
