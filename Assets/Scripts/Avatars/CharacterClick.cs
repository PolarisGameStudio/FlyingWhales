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
            CharacterAction attackAction = icon.icharacter.icharacterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
            if (attackAction.CanBeDone() && attackAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) { //TODO: Change this checker to relationship status checking instead of just faction
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
                UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
                return;
            }
        }else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
            CharacterAction joinBattleAction = icon.icharacter.icharacterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
            if (joinBattleAction.CanBeDone() && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) { //TODO: Change this checker to relationship status checking instead of just faction
                UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(joinBattleAction);
                UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
                return;
            }
        }
        if (icon.icharacter is ECS.Character) {
            UIManager.Instance.ShowCharacterInfo(icon.icharacter as ECS.Character);
        }
        
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (icon.icharacter is Character) {
            Character thisCharacter = icon.icharacter as Character;
            if (thisCharacter.actionData.currentAction != null) {
                if (other.tag == "Character" && thisCharacter.actionData.currentAction.actionType == ACTION_TYPE.ATTACK) {
                    AttackAction attackAction = thisCharacter.actionData.currentAction as AttackAction;
                    CharacterIcon enemy = other.GetComponent<CharacterClick>().icon;
                    if (attackAction.icharacterObj.icharacter.id == enemy.icharacter.id) {
                        thisCharacter.actionData.DoAction();
                    }
                }
            }
        }

    }
}
