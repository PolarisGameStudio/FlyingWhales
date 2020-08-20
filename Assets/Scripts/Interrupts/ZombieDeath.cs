﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class ZombieDeath : Interrupt {
        public ZombieDeath() : base(INTERRUPT.Zombie_Death) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sick_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.Death("Zombie Virus", interrupt: this, _deathLog: interruptHolder.effectLog);
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel == RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
            }
            if(status == REACTION_STATUS.WITNESSED) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
                }
            }
            return response;
        }
        #endregion
    }
}