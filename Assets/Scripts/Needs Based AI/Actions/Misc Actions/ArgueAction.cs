﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ArgueAction : CharacterAction {

    public ArgueAction() : base(ACTION_TYPE.ARGUE) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        if (iparty is CharacterParty) {
            Character arguer = iparty.mainCharacter as Character;
            arguer.SetDoNotDisturb(true);
        }
    }
    public override void OnFirstEncounter(NewParty party, IObject targetObject) {
        //base.OnFirstEncounter(party, targetObject);
        if (targetObject == null) {
            return;
        }
        if (targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty is CharacterParty) {
                CharacterParty targetParty = icharacterObject.iparty as CharacterParty;
                Character targetCharacter = targetParty.mainCharacter as Character;
                if (targetParty.actionData.currentAction == null) {
                    ArgueAction actionToAssign = targetParty.mainCharacter.GetMiscAction(_actionData.actionType) as ArgueAction;
                    targetParty.actionData.AssignAction(actionToAssign, party.icharacterObject);

                    Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ArgueAction", "start_argue");
                    log.AddToFillers(party.mainCharacter, party.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(icharacterObject.iparty.mainCharacter, icharacterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    party.mainCharacter.AddHistory(log);
                    icharacterObject.iparty.mainCharacter.AddHistory(log);
                } else {
                    if (targetParty.actionData.currentAction.actionData.actionType != ACTION_TYPE.ARGUE) {
                        targetParty.actionData.currentAction.EndAction(targetParty, targetParty.actionData.currentTargetObject);
                        ArgueAction actionToAssign = targetParty.mainCharacter.GetMiscAction(_actionData.actionType) as ArgueAction;
                        targetParty.actionData.AssignAction(actionToAssign, party.icharacterObject);

                        Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ArgueAction", "start_argue");
                        log.AddToFillers(party.mainCharacter, party.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(icharacterObject.iparty.mainCharacter, icharacterObject.iparty.mainCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        party.mainCharacter.AddHistory(log);
                        icharacterObject.iparty.mainCharacter.AddHistory(log);
                    }
                }
                    
            }
        }
    }
    public override void PerformAction(NewParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override IObject GetTargetObject(CharacterParty sourceParty) {
        Character mainCharacter = sourceParty.mainCharacter as Character;
        if (mainCharacter.GetAttribute(ATTRIBUTE.INTROVERT) == null && mainCharacter.GetAttribute(ATTRIBUTE.BELLIGERENT) != null) {
            List<CharacterParty> targetCandidates = new List<CharacterParty>();
            for (int i = 0; i < mainCharacter.specificLocation.charactersAtLocation.Count; i++) {
                NewParty targetParty = mainCharacter.specificLocation.charactersAtLocation[i];
                if (targetParty != sourceParty && targetParty is CharacterParty) {
                    Character targetMainCharacter = targetParty.mainCharacter as Character;
                    if (targetMainCharacter.doNotDisturb || targetParty.icon.isTravelling) {
                        continue;
                    }
                    Relationship relationship = mainCharacter.GetRelationshipWith(targetMainCharacter);
                    if (relationship != null && relationship.IsNegative()) {
                        targetCandidates.Add(targetParty as CharacterParty);
                    }
                }
            }
            if (targetCandidates.Count > 0) {
                return targetCandidates[Utilities.rng.Next(0, targetCandidates.Count)].characterObject;
            }
        }
        return null;
    }
    public override void EndAction(NewParty party, IObject targetObject) {
        if(!(party is CharacterParty)) {
            return;
        }
        CharacterParty characterParty = party as CharacterParty;
        if (characterParty.actionData.isDone) {
            return;
        }
        Character arguer = characterParty.mainCharacter as Character;
        arguer.SetDoNotDisturb(false);

        if (targetObject is ICharacterObject) {
            ICharacterObject icharacterObject = targetObject as ICharacterObject;
            if (icharacterObject.iparty is CharacterParty) {
                CharacterParty targetParty = icharacterObject.iparty as CharacterParty;
                Character targetCharacter = targetParty.mainCharacter as Character;
                targetCharacter.SetDoNotDisturb(false);
            }
        }
        //Relationship effects
    }
    public override CharacterAction Clone() {
        ArgueAction action = new ArgueAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
