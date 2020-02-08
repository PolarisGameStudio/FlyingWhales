﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Chat : Interrupt {
        public Chat() : base(INTERRUPT.Chat) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.nonActionEventsComponent.ForceChatCharacter(target as Character, ref overrideEffectLog);
            return true;
        }
        #endregion
    }
}