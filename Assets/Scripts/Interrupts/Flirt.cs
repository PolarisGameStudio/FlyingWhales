﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Flirt : Interrupt {
        public Flirt() : base(INTERRUPT.Flirt) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.nonActionEventsComponent.NormalFlirtCharacter(interruptHolder.target as Character, ref overrideEffectLog);
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            if(target is Character targetCharacter) {
                if (target != witness) {
                    bool isActorLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                    bool isTargetLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);

                    if (isActorLoverOrAffairOfWitness) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                    } else if (isTargetLoverOrAffairOfWitness) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status);
                        //response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                        if(witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                        }
                    } else {
                        Character loverOfActor = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                        if (loverOfActor != null && loverOfActor != targetCharacter) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
                        } else if (witness.relationshipContainer.IsFriendsWith(actor)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
                        }
                    }
                } else {
                    //target is witness
                    if (status == REACTION_STATUS.INFORMED) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status);
                    }
                }
            }
            return response;
        }
        #endregion
    }
}