﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Interrupts {
    public class PulledDown : Interrupt {

        public PulledDown() : base(INTERRUPT.Pulled_Down) {
            duration = 12;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            shouldEndOnSeize = true;
            interruptIconString = GoapActionStateDB.Cowering_Icon;
            logTags = new[] { LOG_TAG.Life_Changes };
        }

        #region Overrides
        //NOTE: The actor in this interrupt is the one being pulled down by scorpion, while the target is the one that burned the actor
        //Normally we will also set the actor as the target in this kind of situation, but since we need to know who is the responsible character for the actor's burning at stake, we need a way to pass the responsible charater to this interrupt
        //And the way to do it is set that responsible character as the target
        //The reason why we need to pass the responsible character is for the Dead trait to know who's responsible for the death of the one being burned at stake
        //It might be confusing, hence, this note
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            //interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "pulled_down");
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override bool PerTickInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.AdjustHP(-300, ELEMENTAL_TYPE.Poison);
            if (interruptHolder.actor.currentHP <= 0) {
                (interruptHolder.target as Scorpion).heldCharacter = null;
                interruptHolder.actor.Death(cause: "pulled_down", responsibleCharacter: interruptHolder.target as Character);
            }
            return true;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "pulled_down");
            if (!interruptHolder.actor.isDead) {
                interruptHolder.actor.AdjustHP(-300, ELEMENTAL_TYPE.Poison);
                if (interruptHolder.actor.currentHP <= 0) {
                    (interruptHolder.target as Scorpion).heldCharacter = null;
                    interruptHolder.actor.Death(cause: "pulled_down", responsibleCharacter: interruptHolder.target as Character);
                }
            }
            return true;
        }
        public override bool OnForceEndInterrupt(InterruptHolder interruptHolder) {
            (interruptHolder.target as Scorpion).heldCharacter = null;
            interruptHolder.actor.traitContainer.RemoveTrait(interruptHolder.actor, "pulled_down");
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            reactions.Add(EMOTION.Shock);
            if (opinionLabel == RelationshipManager.Enemy) {
                if (GameUtilities.RollChance(50)) {
                    reactions.Add(EMOTION.Scorn);
                }
            } else if (opinionLabel == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Scorn);
            }
        }
        #endregion
    }
}