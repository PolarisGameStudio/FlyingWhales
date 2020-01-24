﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DazedBehaviour : CharacterBehaviourComponent {
    public DazedBehaviour() {
        priority = 20;
        //attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += "\n-" + character.name + " is dazed, will only stroll";
        character.PlanIdleStrollOutside();
        return true;
    }
}
