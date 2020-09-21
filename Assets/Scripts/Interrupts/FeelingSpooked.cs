﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingSpooked : Interrupt {
        public FeelingSpooked() : base(INTERRUPT.Feeling_Spooked) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Cowering_Icon;
            logTags = new[] {LOG_TAG.Misc};
        }

        //#region Overrides
        //public override bool ExecuteInterruptEffect(Character actor, IPointOfInterest target) {
        //    actor.combatComponent.AddAvoidInRange(target, true, "embarassed");
        //    return true;
        //}
        //#endregion
    }
}
