﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaos_Orb : Artifact {

    public Chaos_Orb() : base(ARTIFACT_TYPE.Chaos_Orb) {
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.INSPECT };
    }

    public override void OnInspect(Character inspectedBy, out Log result) {
        base.OnInspect(inspectedBy, out result);
        inspectedBy.currentAction.SetEndAction(OnInspectActionDone);
    }
    private void OnInspectActionDone(string result, GoapAction action) {
        action.actor.GoapActionResult(result, action);
        //Characters that inspect the Chaos Orb may be permanently berserked.
        CharacterState state = action.actor.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, action.actor.specificLocation);
        state.SetIsUnending(true);
    }

    public override string ToString() {
        return "Chaos Orb";
    }

}
