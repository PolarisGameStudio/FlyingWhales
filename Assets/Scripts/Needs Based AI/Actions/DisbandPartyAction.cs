﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisbandPartyAction : CharacterAction {
    public DisbandPartyAction() : base(ACTION_TYPE.DISBAND_PARTY) {
    }

    public override CharacterAction Clone() {
        DisbandPartyAction action = new DisbandPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }

    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        for (int i = 0; i < party.icharacters.Count; i++) {
            ICharacter character = party.icharacters[i];
        }
    }
}
