﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class SerialKillerBehaviour : CharacterBehaviourComponent {
    public SerialKillerBehaviour() {
        priority = 45;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += "\n-" + character.name + " is a Serial Killer, 15% chance to Hunt Victim if there is one";
        int chance = UnityEngine.Random.Range(0, 100);
        log += "\n  -RNG roll: " + chance;
        if (chance < 15) {
            SerialKiller serialKiller = character.traitContainer.GetNormalTrait<SerialKiller>() as SerialKiller;
            //serialKiller.CheckTargetVictimIfStillAvailable();
            if(serialKiller.targetVictim != null) {
                log += "\n  -Target victim is " + serialKiller.targetVictim.name + ", will try to Hunt Victim";
                if (serialKiller.CreateHuntVictimJob()) {
                    log += "\n  -Created Hunt Victim Job";
                    return true;
                } else {
                    log += "\n  -Cannot hunt victim, already has a Hunt Victim Job in queue";
                }
            } else {
                log += "\n  -No target victim";
            }
        }
        return false;
    }
}