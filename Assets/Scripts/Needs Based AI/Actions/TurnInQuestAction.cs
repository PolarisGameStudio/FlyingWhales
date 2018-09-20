﻿using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInQuestAction : CharacterAction {

    public TurnInQuestAction() : base(ACTION_TYPE.TURN_IN_QUEST) {}

    #region overrides
    public override CharacterAction Clone() {
        TurnInQuestAction action = new TurnInQuestAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        Character mainCharacter = party.owner as Character;
        if (mainCharacter.currentQuest == null) {
            //if the main character's current quest is null,
            //this would ususally happen because something unusual happened to the
            //current quest of the character. So therefore, it was set to null
            EndAction(party, targetObject);
        } else {
            //turn in the quest
            //mainCharacter.TurnInQuest();
            mainCharacter.currentQuest.OnQuestTurnedIn();
            mainCharacter.SetQuest(null);
            EndAction(party, targetObject);
        }
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        if ((party.owner as Character).dailySchedule.currentPhase.phaseType != SCHEDULE_PHASE_TYPE.WORK) {
            //the turn in quest action has reached another phase, disband party after doing this action
            (party.owner as Character).AddActionToQueue(party.characterObject.currentState.GetAction(ACTION_TYPE.DISBAND_PARTY), party.characterObject);
        }
    }
    #endregion
}
