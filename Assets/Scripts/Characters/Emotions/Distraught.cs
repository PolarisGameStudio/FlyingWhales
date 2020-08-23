﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distraught : Emotion {

    public Distraught() : base(EMOTION.Distraught) {
        responses = new[] { "Distraught" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null) {
        if(target is Character targetCharacter) {
            if (targetCharacter.IsInDanger()) {
                if (witness.characterClass.IsCombatant()) {
                    Party activeRescueParty = witness.faction.GetActivePartywithTarget(PARTY_TYPE.Rescue, targetCharacter);
                    if (activeRescueParty != null && !activeRescueParty.isWaitTimeOver && !activeRescueParty.isDisbanded) {
                        activeRescueParty.AddMember(witness);
                    } else {
                        witness.jobComponent.TriggerRescueJob(targetCharacter);
                    }
                } else {
                    witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry_Request, targetCharacter, targetCharacter.name + " is in danger");
                }
            } else {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
            }
        }
        return base.ProcessEmotion(witness, target, status, goapNode);
    }
    #endregion
}