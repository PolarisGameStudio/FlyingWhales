﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LeaveFaction : Interrupt {
        public LeaveFaction() : base(INTERRUPT.Leave_Faction) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            Faction prevFaction = actor.faction;
            if (actor.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction)) {
                overrideEffectLog  = new Log(GameManager.Instance.Today(), "Interrupt", "Leave Faction", actor.interruptComponent.identifier);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                return true;
            }
            return base.ExecuteInterruptStartEffect(actor, target, ref overrideEffectLog);
        }
        #endregion
    }
}