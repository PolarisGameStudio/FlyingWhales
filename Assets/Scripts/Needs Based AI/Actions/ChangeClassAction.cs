﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ChangeClassAction : CharacterAction {
    public NewParty partyAssigned;
    public string advertisedClassName;

    public ChangeClassAction() : base(ACTION_TYPE.CHANGE_CLASS) {
    }

    #region Overrides
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        partyAssigned = iparty;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if(partyAssigned != null && partyAssigned.icharacters[0] is Character) {
            Character character = partyAssigned.icharacters[0] as Character;
            character.ChangeClass(advertisedClassName);
        }
    }
    public override bool CanBeDone(IObject targetObject) {
        if(partyAssigned != null) {
            return false;
        }
        return base.CanBeDone(targetObject);
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if(party.icharacters[0].characterClass != null) {
            if(targetObject.objectLocation.tileLocation.areaOfTile.excessClasses.Contains(party.icharacters[0].characterClass.className)
                && targetObject.objectLocation.tileLocation.areaOfTile.missingClasses.Contains(advertisedClassName)) { //TODO: Subject for change
                return true;
            }
        }
        return false;
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        partyAssigned = null;
    }
    #endregion

    public void SetAdvertisedClass(string className) {
        advertisedClassName = className;
    }
}
