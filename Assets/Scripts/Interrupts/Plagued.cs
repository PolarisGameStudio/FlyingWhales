﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
	public class Plagued : Interrupt {
		public Plagued() : base(INTERRUPT.Plagued) {
			duration = 0;
			isSimulateneous = true;
			interruptIconString = GoapActionStateDB.No_Icon;
		}

		#region Overrides
		public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
			ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
			if (interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Plagued")) {
				overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Plagued", "contract");
				overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				//log.AddLogToInvolvedObjects();
				return true;
			}
			return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
		}
		#endregion
	}
}