﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class Mock : Interrupt {
        public Mock() : base(INTERRUPT.Mock) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Mock_Icon;
            logTags = new[] {LOG_TAG.Social, LOG_TAG.Misc};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character targetCharacter = interruptHolder.target as Character;
            if (targetCharacter != interruptHolder.actor) {
                targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, interruptHolder.actor, "Base", -3);
            }
            return true;
        }
        #endregion
    }
}