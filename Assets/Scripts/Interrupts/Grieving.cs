﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Grieving : Interrupt {
        public Grieving() : base(INTERRUPT.Grieving) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
            return true;
        }
        #endregion
    }
}