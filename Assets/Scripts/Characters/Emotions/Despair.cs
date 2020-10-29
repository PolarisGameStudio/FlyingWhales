﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despair : Emotion {

    public Despair() : base(EMOTION.Despair) {
        responses = new[] {"in_Despair"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null, string reason = "") {
        witness.needsComponent.AdjustHope(-10);
        witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, target, "feeling despair");
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}