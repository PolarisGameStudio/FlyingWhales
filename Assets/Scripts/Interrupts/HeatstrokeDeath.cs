﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class HeatstrokeDeath : Interrupt {
        public HeatstrokeDeath() : base(INTERRUPT.Heatstroke_Death) {
            interruptIconString = GoapActionStateDB.Injured_Icon;
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.Death("Heatstroke", _deathLog: interruptHolder.effectLog, interrupt: this);
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
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
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