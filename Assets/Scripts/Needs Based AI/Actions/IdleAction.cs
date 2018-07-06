﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class IdleAction : CharacterAction {
    public IdleAction() : base(ACTION_TYPE.IDLE) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        IdleAction idleAction = new IdleAction();
        SetCommonData(idleAction);
        idleAction.Initialize();
        return idleAction;
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        //if (character.characterObject.objectLocation == null || character.characterObject.objectLocation.id != _state.obj.objectLocation.id || character.homeStructure.objectLocation.id != _state.obj.objectLocation.id) {
        if (party.characterObject.objectLocation == null || party.homeStructure.objectLocation.id != targetObject.objectLocation.id) {
            //the characters location is null or the object that this action belongs to is not the home of the character
            return false;
        }
        return base.CanBeDoneBy(party, targetObject);
    }
    #endregion
}
