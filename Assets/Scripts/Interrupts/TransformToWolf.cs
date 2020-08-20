﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class TransformToWolf : Interrupt {
        public TransformToWolf() : base(INTERRUPT.Transform_To_Wolf) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Transforming");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.lycanData.TurnToWolf();
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            Character originalForm = actor;
            if(actor.lycanData != null) {
                originalForm = actor.lycanData.originalForm;
            }
            if (!witness.isLycanthrope) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, originalForm, status);
                // response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm);

                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(originalForm);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                    opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, originalForm, status);
                }
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, originalForm, status);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, originalForm, status);
                }
                CrimeManager.Instance.ReactToCrime(witness, originalForm, this, CRIME_SEVERITY.HEINOUS);
            }
            return response;
        }
        #endregion
    }
}
