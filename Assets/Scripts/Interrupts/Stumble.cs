﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stumble : Interrupt {
        public Stumble() : base(INTERRUPT.Stumble) {
            duration = 2;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Injured_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            int randomHpToLose = UnityEngine.Random.Range(1, 6);
            float percentMaxHPToLose = randomHpToLose / 100f;
            int actualHPToLose = Mathf.CeilToInt(interruptHolder.actor.maxHP * percentMaxHPToLose);
            Debug.Log(
                $"Stumble of {interruptHolder.actor.name} percent: {percentMaxHPToLose}, max hp: {interruptHolder.actor.maxHP}, lost hp: {actualHPToLose}");
            interruptHolder.actor.AdjustHP(-actualHPToLose, ELEMENTAL_TYPE.Normal, showHPBar: true);
            if (interruptHolder.actor.currentHP <= 0) {
                interruptHolder.actor.Death("Stumble");
            }
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            if (witness.relationshipContainer.IsFriendsWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (witness.relationshipContainer.IsEnemiesWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
            }
            return response;
        }
        #endregion
    }
}